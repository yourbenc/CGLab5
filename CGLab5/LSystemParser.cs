using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CGLab5
{
    public static class LSystemParser
    {
        public static LSystem ParseFromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var lSystem = new LSystem();

            // Первая строка: аксиома, угол, начальное направление
            var firstLine = lines[0].Trim();
            var parts = firstLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                throw new FormatException("Первая строка должна содержать: аксиома угол [начальное_направление]");

            lSystem.Axiom = parts[0];
            lSystem.Angle = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            lSystem.InitialDirection = parts.Length > 2 ? double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture) : 0;

            // Остальные строки: правила
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                    continue;

                // Поддерживаем разные форматы разделителей
                string[] ruleParts;
                if (line.Contains("->"))
                    ruleParts = line.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                else if (line.Contains("→"))
                    ruleParts = line.Split(new[] { "→" }, StringSplitOptions.RemoveEmptyEntries);
                else
                    continue;

                if (ruleParts.Length == 2)
                {
                    char symbol = ruleParts[0].Trim()[0];
                    string replacement = ruleParts[1].Trim();
                    lSystem.Rules[symbol] = replacement;
                }
            }

            return lSystem;
        }

        public static LSystem CreateFromString(string axiom, double angle, double initialDir, params (char, string)[] rules)
        {
            var lSystem = new LSystem
            {
                Axiom = axiom,
                Angle = angle,
                InitialDirection = initialDir
            };

            foreach (var (symbol, replacement) in rules)
            {
                lSystem.Rules[symbol] = replacement;
            }

            return lSystem;
        }
    }
}