using System;
using System.Collections.Generic;
using System.Globalization;
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
        private List<List<System.Windows.Point>> _midpointLevels = new List<List<System.Windows.Point>>();
        private int _currentMidpointLevel;

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

        private void GenerateMidpoint_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(MidpointSegmentsTextBox.Text, out int initialSegments) ||
                !int.TryParse(MidpointIterationsTextBox.Text, out int iterations) ||
                !TryParseDouble(MidpointAmplitudeTextBox.Text, out double amplitude) ||
                !TryParseDouble(MidpointRoughnessTextBox.Text, out double roughness) ||
                !int.TryParse(MidpointSeedTextBox.Text, out int seed))
            {
                MessageBox.Show("Проверьте значения параметров: ожидаются числа.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (initialSegments < 1 || initialSegments > 16 || iterations < 1 || iterations > 12 ||
                !double.IsFinite(amplitude) || amplitude <= 0 || amplitude > 1000 ||
                !double.IsFinite(roughness) || roughness < 0 || roughness > 1)
            {
                MessageBox.Show(
                    "Допустимые значения: начальных отрезков 1–16, итераций 1–12, амплитуда больше 0 и не больше 1000, коэффициент уменьшения от 0 до 1.",
                    "Параметры вне диапазона",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _midpointLevels = MidpointDisplacement.GenerateLevels(
                initialSegments,
                iterations,
                amplitude,
                roughness,
                seed);
            _currentMidpointLevel = 0;
            UpdateMidpointStepDisplay();
        }

        private void PreviousMidpointStep_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMidpointLevel > 0)
            {
                _currentMidpointLevel--;
                UpdateMidpointStepDisplay();
            }
        }

        private void NextMidpointStep_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMidpointLevel < _midpointLevels.Count - 1)
            {
                _currentMidpointLevel++;
                UpdateMidpointStepDisplay();
            }
        }

        private void ClearMidpoint_Click(object sender, RoutedEventArgs e)
        {
            _midpointLevels.Clear();
            _currentMidpointLevel = 0;
            MidpointCanvas.Children.Clear();
            MidpointStepText.Text = "Шаг 0 / 0";
            MidpointStatusText.Text = "Задайте параметры и сгенерируйте последовательность.";
            PreviousMidpointStepButton.IsEnabled = false;
            NextMidpointStepButton.IsEnabled = false;
        }

        private void MidpointCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_midpointLevels.Count > 0)
            {
                RenderMidpointLevel();
            }
        }

        private void UpdateMidpointStepDisplay()
        {
            if (_midpointLevels.Count == 0)
            {
                MidpointCanvas.Children.Clear();
                MidpointStepText.Text = "Шаг 0 / 0";
                MidpointStatusText.Text = "Задайте параметры и сгенерируйте последовательность.";
                PreviousMidpointStepButton.IsEnabled = false;
                NextMidpointStepButton.IsEnabled = false;
                return;
            }

            int lastStep = _midpointLevels.Count - 1;
            MidpointStepText.Text = $"Шаг {_currentMidpointLevel} / {lastStep}";
            MidpointStatusText.Text = $"Точек в текущей ломаной: {_midpointLevels[_currentMidpointLevel].Count}. Seed сохраняет тот же случайный профиль при повторной генерации.";
            PreviousMidpointStepButton.IsEnabled = _currentMidpointLevel > 0;
            NextMidpointStepButton.IsEnabled = _currentMidpointLevel < lastStep;
            RenderMidpointLevel();
        }

        private void RenderMidpointLevel()
        {
            MidpointCanvas.Children.Clear();
            if (_midpointLevels.Count == 0)
            {
                return;
            }

            List<System.Windows.Point> points = _midpointLevels[_currentMidpointLevel];
            if (points.Count < 2)
            {
                return;
            }

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            foreach (System.Windows.Point point in points)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }

            double rangeX = Math.Max(1, maxX - minX);
            double rangeY = Math.Max(1, maxY - minY);
            double canvasWidth = MidpointCanvas.ActualWidth > 0 ? MidpointCanvas.ActualWidth : 800;
            double canvasHeight = MidpointCanvas.ActualHeight > 0 ? MidpointCanvas.ActualHeight : 500;
            const double padding = 30;
            double scale = Math.Min(
                Math.Max(1, canvasWidth - 2 * padding) / rangeX,
                Math.Max(1, canvasHeight - 2 * padding) / rangeY);
            double offsetX = (canvasWidth - rangeX * scale) / 2 - minX * scale;
            double offsetY = (canvasHeight - rangeY * scale) / 2 - minY * scale;

            var renderedPoints = new PointCollection();
            foreach (System.Windows.Point point in points)
            {
                renderedPoints.Add(new System.Windows.Point(
                    point.X * scale + offsetX,
                    point.Y * scale + offsetY));
            }

            MidpointCanvas.Children.Add(new Polyline
            {
                Points = renderedPoints,
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            });
        }

        private static bool TryParseDouble(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
                   double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
