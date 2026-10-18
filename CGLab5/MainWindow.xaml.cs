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

        private void DrawFractal(LSystem lSystem, int iterations, bool isTree = false)
        {
            _currentLSystem = lSystem;
            bool useRandomness = (RandomnessCheckBox.IsChecked == true) && isTree;

            _interpreter = new TurtleInterpreter(lSystem.Angle, useRandomness, isTree);
            string generatedString = lSystem.Generate(iterations);

            double initialDir = isTree ? -90 : lSystem.InitialDirection;

            _currentSegments = _interpreter.Interpret(generatedString, initialDir);

            RenderSegments();
        }

        private void RenderSegments()
        {
            FractalCanvas.Children.Clear();

            if (_currentSegments.Count == 0) return;

            // Находим границы
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

            // Масштаб с отступом 5%
            double padding = 0.95;
            double scale = Math.Min((canvasWidth * padding) / width, (canvasHeight * padding) / height);

            // Центрирование
            double offsetX = (canvasWidth - width * scale) / 2 - minX * scale;
            double offsetY = (canvasHeight - height * scale) / 2 - minY * scale;

            // Создаем линии с правильными координатами
            foreach (var segment in _currentSegments)
            {
                var line = new Line
                {
                    X1 = segment.Start.X * scale + offsetX,
                    Y1 = segment.Start.Y * scale + offsetY,
                    X2 = segment.End.X * scale + offsetX,
                    Y2 = segment.End.Y * scale + offsetY,
                    Stroke = new SolidColorBrush(segment.Color),

                    StrokeThickness = _currentLSystem != null &&
                  (_currentLSystem.Axiom.Contains("[") ||
                   _currentLSystem.Rules.Values.Any(v => v.Contains("[")))
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
                    var lSystem = LSystemParser.ParseFromFile(dialog.FileName);
                    int iterations = int.Parse(IterationsTextBox.Text);
                    bool isTree = lSystem.Axiom.Contains("[") || lSystem.Rules.Values.Any(v => v.Contains("["));
                    DrawFractal(lSystem, iterations, isTree);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FractalTree_Click(object sender, RoutedEventArgs e)
        {
            // ИСПРАВЛЕНО: все ветки теперь внутри скобок
            var lSystem = LSystemParser.CreateFromString(
                "X", 25, -90,
                ('X', "F[+X]F[-X]F[+X]"),  // Было: "F[+X]F[-X]+X"
                ('F', "FF")
            );

            int iterations = Math.Min(int.Parse(IterationsTextBox.Text), 6);
            DrawFractal(lSystem, iterations, true);
        }

        private void Redraw_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLSystem != null)
            {
                int iterations = int.Parse(IterationsTextBox.Text);
                bool isTree = _currentLSystem.Axiom.Contains("[") || _currentLSystem.Rules.Values.Any(v => v.Contains("["));
                DrawFractal(_currentLSystem, iterations, isTree);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            FractalCanvas.Children.Clear();
            _currentSegments.Clear();
            _currentLSystem = null;
        }
    }
}