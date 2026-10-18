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
        private double _angleIncrement;
        private List<LineSegment> _segments;
        private double _thickness;
        private Color _color;
        private Random _random;
        private bool _useRandomness;
        private bool _isTreeMode;

        public TurtleInterpreter(double angleIncrement, bool useRandomness = false, bool isTreeMode = false)
        {
            _angleIncrement = angleIncrement;
            _useRandomness = useRandomness;
            _isTreeMode = isTreeMode;
            _stateStack = new Stack<TurtleState>();
            _segments = new List<LineSegment>();
            _random = new Random();
        }

        public void Reset()
        {
            _currentPosition = new Point(0, 0);
            _currentAngle = 0;
            _thickness = _isTreeMode ? 5.0 : 1.0;
            _color = _isTreeMode ? Colors.SaddleBrown : Colors.Black;
            _segments.Clear();
            _stateStack.Clear();
        }

        public List<LineSegment> Interpret(string lSystemString, double initialDirection = 0)
        {
            Reset();
            _currentAngle = initialDirection;

            foreach (char c in lSystemString)
            {
                ProcessCommand(c);
            }

            return _segments;
        }

        private void ProcessCommand(char command)
        {
            switch (command)
            {
                case 'F':
                case 'G':
                    MoveForward(true);
                    break;
                case 'f':
                    MoveForward(false);
                    break;
                case '+':
                    Rotate(GetRandomAngle(_angleIncrement));
                    break;
                case '-':
                    Rotate(GetRandomAngle(-_angleIncrement));
                    break;
                case '[':
                    SaveState();
                    break;
                case ']':
                    RestoreState();
                    break;
                case 'X':
                case 'Y':
                case 'A':
                case 'B':
                    break;
            }
        }

        private void MoveForward(bool draw)
        {
            // В WPF: 0° = вправо, 90° = вниз, -90° = вверх
            double angleRad = _currentAngle * Math.PI / 180.0;
            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);

            Point newPosition = new Point(
                _currentPosition.X + dx,
                _currentPosition.Y + dy
            );

            if (draw)
            {
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
                    _color = InterpolateColor(Colors.SaddleBrown, Colors.ForestGreen, 0.15);
                }
            }

            _currentPosition = newPosition;
        }

        private void Rotate(double angle)
        {
            _currentAngle += angle;
        }

        private void SaveState()
        {
            _stateStack.Push(new TurtleState
            {
                Position = _currentPosition,
                Angle = _currentAngle,
                Thickness = _thickness,
                Color = _color
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
            }
        }

        private double GetRandomAngle(double baseAngle)
        {
            if (_useRandomness)
            {
                return baseAngle + _random.NextDouble() * 10 - 5;
            }
            return baseAngle;
        }

        private Color InterpolateColor(Color from, Color to, double factor)
        {
            return Color.FromRgb(
                (byte)Math.Max(0, Math.Min(255, from.R + (to.R - from.R) * factor)),
                (byte)Math.Max(0, Math.Min(255, from.G + (to.G - from.G) * factor)),
                (byte)Math.Max(0, Math.Min(255, from.B + (to.B - from.B) * factor))
            );
        }

        public List<LineSegment> GetSegments() => _segments;
    }
}