using System;
using System.Collections.Generic;
using System.Windows;

namespace CGLab5
{
    /// <summary>Builds each refinement level of a one-dimensional midpoint-displacement profile.</summary>
    public static class MidpointDisplacement
    {
        public static List<List<Point>> GenerateLevels(
            int initialSegments,
            int iterations,
            double initialAmplitude,
            double roughness,
            int seed,
            double width = 1000)
        {
            if (initialSegments < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(initialSegments), "Initial segment count must be positive.");
            }
            if (iterations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations), "Iteration count cannot be negative.");
            }
            if (initialAmplitude < 0 || double.IsNaN(initialAmplitude) || double.IsInfinity(initialAmplitude))
            {
                throw new ArgumentOutOfRangeException(nameof(initialAmplitude), "Amplitude must be a finite, non-negative value.");
            }
            if (roughness < 0 || roughness > 1 || double.IsNaN(roughness))
            {
                throw new ArgumentOutOfRangeException(nameof(roughness), "Roughness must be between 0 and 1.");
            }
            if (width <= 0 || double.IsNaN(width) || double.IsInfinity(width))
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be a finite, positive value.");
            }

            var levels = new List<List<Point>>(iterations + 1);
            var points = new List<Point>(initialSegments + 1);
            for (int i = 0; i <= initialSegments; i++)
            {
                points.Add(new Point(width * i / initialSegments, 0));
            }
            levels.Add(points);

            var random = new Random(seed);
            double amplitude = initialAmplitude;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                var refined = new List<Point>((points.Count - 1) * 2 + 1)
                {
                    points[0]
                };

                for (int i = 0; i < points.Count - 1; i++)
                {
                    Point start = points[i];
                    Point end = points[i + 1];
                    double midpointX = (start.X + end.X) / 2;
                    double midpointY = (start.Y + end.Y) / 2;
                    double displacement = (random.NextDouble() * 2 - 1) * amplitude;

                    refined.Add(new Point(midpointX, midpointY + displacement));
                    refined.Add(end);
                }

                points = refined;
                levels.Add(points);
                amplitude *= roughness;
            }

            return levels;
        }
    }
}
