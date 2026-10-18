using System.Windows;
using System.Windows.Media;

namespace CGLab5
{
    public class LineSegment
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public double Thickness { get; set; }
        public Color Color { get; set; }
    }
}