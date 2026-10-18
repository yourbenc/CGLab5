using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace CGLab5
{
    public partial class MainWindow : Window
    {
        private LSystem _currentLSystem;
        private TurtleInterpreter _interpreter;
        private List<LineSegment> _currentSegments;

        public MainWindow()
        {
            InitializeComponent();
            _currentSegments = new List<LineSegment>();
        }

        private void DrawFractal(LSystem lSystem, int iterations)
        {
            bool isTree = lSystem.Axiom.Contains("[") || lSystem.Rules.Values.Any(v => v.Contains("["));

            _interpreter = new TurtleInterpreter(lSystem, isTree);
            string generatedString = lSystem.Generate(iterations);

            _currentSegments = _interpreter.Interpret(generatedString);

            RenderSegments();
        }

        private void RenderSegments()
        {
            FractalCanvas.Children.Clear();

            if (_currentSegments.Count == 0) return;

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var segment in _currentSegments)
            {
                minX = Math.Min(minX, Math.Min(segment.Start.X, segment.End.X));
                maxX = Math.Max(maxX, Math.Max(segment.Start.X, segment.End.X));
                minY = Math.Min(minY, Math.Min(segment.Start.Y, segment.End.Y));
                maxY = Math.Max(maxY, Math.Max(segment.Start.Y, segment.End.Y));
            }

            double width = maxX - minX;
            double height = maxY - minY;

            if (width < 0.001) width = 1;
            if (height < 0.001) height = 1;

            double canvasWidth = FractalCanvas.ActualWidth > 0 ? FractalCanvas.ActualWidth : 800;
            double canvasHeight = FractalCanvas.ActualHeight > 0 ? FractalCanvas.ActualHeight : 500;

            double padding = 0.95;
            double scale = Math.Min((canvasWidth * padding) / width, (canvasHeight * padding) / height);

            double offsetX = (canvasWidth - width * scale) / 2 - minX * scale;
            double offsetY = (canvasHeight - height * scale) / 2 - minY * scale;

            foreach (var segment in _currentSegments)
            {
                var line = new Line
                {
                    X1 = segment.Start.X * scale + offsetX,
                    Y1 = segment.Start.Y * scale + offsetY,
                    X2 = segment.End.X * scale + offsetX,
                    Y2 = segment.End.Y * scale + offsetY,
                    Stroke = new SolidColorBrush(segment.Color),
                    StrokeThickness = segment.Thickness > 1.0
                        ? Math.Min(8.0, segment.Thickness * Math.Sqrt(scale))
                        : 1.0,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                FractalCanvas.Children.Add(line);
            }
        }

        private void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы L-систем (*.txt)|*.txt|Все файлы (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _currentLSystem = LSystemParser.ParseFromFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _currentLSystem = null;
                }
            }
        }

        private void Redraw_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLSystem == null)
            {
                MessageBox.Show("Сначала загрузите файл L-системы.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int iterations = int.Parse(IterationsTextBox.Text);
                if (iterations < 1)
                {
                    MessageBox.Show("Количество итераций должно быть не меньше 1.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DrawFractal(_currentLSystem, iterations);
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите корректное число итераций.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            FractalCanvas.Children.Clear();
            _currentSegments.Clear();
            _currentLSystem = null;
            IterationsTextBox.Text = "5";
        }
    }
}