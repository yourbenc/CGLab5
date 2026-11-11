using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CGLab5;

public partial class BezierEditor : UserControl
{
    private const double PointRadius = 6;
    private readonly List<Point> points = new();
    private int draggedIndex = -1;
    private Vector dragOffset;

    public BezierEditor()
    {
        InitializeComponent();
    }

    private int FindPoint(Point position)
    {
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if ((points[i] - position).LengthSquared <= PointRadius * PointRadius)
                return i;
        }

        return -1;
    }

    private void StartEdit(object sender, MouseButtonEventArgs e)
    {
        Point position = e.GetPosition(Plot);
        int index = FindPoint(position);
        if (index < 0)
        {
            points.Add(position);
            Redraw();
            return;
        }

        if (Plot.CaptureMouse())
        {
            draggedIndex = index;
            dragOffset = points[index] - position;
        }
    }

    private void MovePoint(object sender, MouseEventArgs e)
    {
        if (draggedIndex < 0)
            return;

        Point position = e.GetPosition(Plot) + dragOffset;
        points[draggedIndex] = new Point(
            Math.Clamp(position.X, 0, Plot.ActualWidth),
            Math.Clamp(position.Y, 0, Plot.ActualHeight));
        Redraw();
    }

    private void EndEdit(object sender, MouseButtonEventArgs e)
    {
        if (draggedIndex >= 0)
            Plot.ReleaseMouseCapture();
    }

    private void CancelDrag(object sender, MouseEventArgs e)
    {
        draggedIndex = -1;
    }

    private void DeletePoint(object sender, MouseButtonEventArgs e)
    {
        if (draggedIndex >= 0)
            return;

        int index = FindPoint(e.GetPosition(Plot));
        if (index >= 0)
        {
            points.RemoveAt(index);
            Redraw();
        }
    }

    private Vector TangentAt(int index)
    {
        if (index == 0)
            return points[1] - points[0];
        if (index == points.Count - 1)
            return points[index] - points[index - 1];

        return (points[index + 1] - points[index - 1]) * 0.5;
    }

    private void Redraw()
    {
        var geometry = new StreamGeometry();
        if (points.Count >= 2)
        {
            using StreamGeometryContext context = geometry.Open();
            context.BeginFigure(points[0], isFilled: false, isClosed: false);
            for (int i = 0; i < points.Count - 1; i++)
            {
                // Convert the endpoint derivatives to cubic Bézier control points.
                Point firstControl = points[i] + TangentAt(i) / 3;
                Point secondControl = points[i + 1] - TangentAt(i + 1) / 3;
                context.BezierTo(firstControl, secondControl, points[i + 1], true, false);
            }
        }

        geometry.Freeze();
        Curve.Data = geometry;

        Plot.Children.RemoveRange(1, Plot.Children.Count - 1);
        foreach (Point point in points)
        {
            var marker = new Ellipse
            {
                Width = PointRadius * 2,
                Height = PointRadius * 2,
                Fill = Brushes.White,
                Stroke = Brushes.Navy,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(marker, point.X - PointRadius);
            Canvas.SetTop(marker, point.Y - PointRadius);
            Plot.Children.Add(marker);
        }
    }
}
