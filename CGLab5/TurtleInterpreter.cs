using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CGLab5
{
    public class TurtleInterpreter
    {
        private Stack<TurtleState> _stateStack;
        private Point _currentPosition;
        private double _currentAngle;
        private List<LineSegment> _segments;
        private double _thickness;
        private Color _color;
        private int _branchDepth;
        private const int MaxBranchDepth = 4;
        private Random _random;
        private bool _isTreeMode;
        private LSystem _lSystem;

        public TurtleInterpreter(LSystem lSystem, bool isTreeMode = false)
        {
            _lSystem = lSystem;
            _isTreeMode = isTreeMode;
            _stateStack = new Stack<TurtleState>();
            _segments = new List<LineSegment>();
            _random = new Random();
        }

        public void Reset()
        {
            _currentPosition = new Point(0, 0);
            _currentAngle = _lSystem.InitialDirection;
            _branchDepth = 0;
            _thickness = _isTreeMode ? 5.0 : 1.0;
            _color = _isTreeMode ? Colors.SaddleBrown : Colors.Black;
            _segments.Clear();
            _stateStack.Clear();
        }

        public List<LineSegment> Interpret(string lSystemString)
        {
            Reset();
            foreach (char c in lSystemString)
            {
                ProcessCommand(c);
            }
            return _segments;
        }

        private void ProcessCommand(char command)
        {
            if (_lSystem.Interpretations.ContainsKey(command))
            {
                ExecuteInterpretation(_lSystem.Interpretations[command]);
            }
            else
            {
                // Fallback для совместимости
                switch (command)
                {
                    case 'F': case 'G': MoveForward(true, 1.0); break;
                    case 'f': MoveForward(false, 1.0); break;
                    case '+': Rotate(_lSystem.Angle); break;
                    case '-': Rotate(-_lSystem.Angle); break;
                    case '[': SaveState(); _branchDepth++; break;
                    case ']': RestoreState(); break;
                }
            }
        }

        private void ExecuteInterpretation(CommandInterpretation interp)
        {
            switch (interp.ActionType)
            {
                case "FORWARD":
                    MoveForward(true, interp.Value);
                    break;

                case "ROTATE":
                    double angle = interp.Value; // Базовый угол

                    if (interp.IsRandom)
                    {
                        // ИСПРАВЛЕНО: добавляем случайное отклонение к базовому углу
                        // Диапазон: от -RandomRange до +RandomRange
                        double randomDeviation = (_random.NextDouble() - 0.5) * 2 * interp.RandomRange;
                        angle = angle + randomDeviation;
                    }

                    Rotate(angle);
                    break;

                case "SAVE":
                    SaveState();
                    _branchDepth++;
                    break;

                case "RESTORE":
                    RestoreState();
                    break;
            }
        }

        private void MoveForward(bool draw, double length = 1.0)
        {
            double angleRad = _currentAngle * Math.PI / 180.0;
            double dx = Math.Cos(angleRad) * length;
            double dy = Math.Sin(angleRad) * length;

            Point newPosition = new Point(_currentPosition.X + dx, _currentPosition.Y + dy);

            if (draw)
            {
                if (_isTreeMode)
                {
                    double colorFactor = Math.Min(1.0, (double)_branchDepth / MaxBranchDepth);
                    _color = InterpolateColor(Colors.SaddleBrown, Colors.ForestGreen, colorFactor);
                }

                _segments.Add(new LineSegment
                {
                    Start = _currentPosition,
                    End = newPosition,
                    Thickness = _thickness,
                    Color = _color
                });

                if (_isTreeMode)
                {
                    _thickness = Math.Max(0.5, _thickness * 0.7);
                }
            }
            _currentPosition = newPosition;
        }

        private void Rotate(double angle) => _currentAngle += angle;

        private void SaveState()
        {
            _stateStack.Push(new TurtleState
            {
                Position = _currentPosition,
                Angle = _currentAngle,
                Thickness = _thickness,
                Color = _color,
                BranchDepth = _branchDepth
            });
        }

        private void RestoreState()
        {
            if (_stateStack.Count > 0)
            {
                var state = _stateStack.Pop();
                _currentPosition = state.Position;
                _currentAngle = state.Angle;
                _thickness = state.Thickness;
                _color = state.Color;
                _branchDepth = state.BranchDepth;
            }
        }

        private Color InterpolateColor(Color from, Color to, double factor)
        {
            factor = Math.Max(0, Math.Min(1, factor));
            return Color.FromRgb(
                (byte)(from.R + (to.R - from.R) * factor),
                (byte)(from.G + (to.G - from.G) * factor),
                (byte)(from.B + (to.B - from.B) * factor)
            );
        }

        public List<LineSegment> GetSegments() => _segments;
    }
}