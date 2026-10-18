using System;
using System.IO;

namespace CGLab5
{
    public static class LSystemParser
    {
        public static LSystem ParseFromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var lSystem = new LSystem();

            var firstLine = lines[0].Trim();
            var parts = firstLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                throw new FormatException("Первая строка должна содержать: аксиома угол [начальное_направление]");

            lSystem.Axiom = parts[0];
            lSystem.Angle = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            lSystem.InitialDirection = parts.Length > 2 ? double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture) : 0;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                    continue;

                // Чтение интерпретации (например: INTERPRET: + ROTATE RANDOM 10)
                if (line.StartsWith("INTERPRET:", StringComparison.OrdinalIgnoreCase))
                {
                    ParseInterpretation(line, lSystem);
                }
                // Чтение правил замены
                else if (line.Contains("->") || line.Contains("→"))
                {
                    string[] ruleParts = line.Contains("->")
                        ? line.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries)
                        : line.Split(new[] { "→" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ruleParts.Length == 2)
                    {
                        char symbol = ruleParts[0].Trim()[0];
                        string replacement = ruleParts[1].Trim();
                        lSystem.Rules[symbol] = replacement;
                    }
                }
            }

            // Если в файле нет интерпретаций, задаём стандартные (для обратной совместимости)
            if (lSystem.Interpretations.Count == 0)
            {
                SetDefaultInterpretations(lSystem);
            }

            return lSystem;
        }

        private static void ParseInterpretation(string line, LSystem lSystem)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) return;

            char symbol = parts[1][0];
            string actionType = parts[2].ToUpper();

            var interp = new CommandInterpretation { ActionType = actionType };

            if (actionType == "ROTATE")
            {
                if (parts[3].ToUpper().Contains("RANDOM"))
                {
                    interp.IsRandom = true;

                    // Базовый угол берём из первой строки файла (lSystem.Angle)
                    // Если есть знак минус перед RANDOM, делаем угол отрицательным
                    bool isNegative = parts[3].StartsWith("-");
                    interp.Value = isNegative ? -lSystem.Angle : lSystem.Angle;

                    // Диапазон случайности
                    if (parts.Length > 4)
                    {
                        interp.RandomRange = double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    // Фиксированный угол
                    interp.Value = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (actionType == "FORWARD")
            {
                interp.Value = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
            }

            lSystem.Interpretations[symbol] = interp;
        }

        private static void SetDefaultInterpretations(LSystem lSystem)
        {
            lSystem.Interpretations['F'] = new CommandInterpretation { ActionType = "FORWARD", Value = 1 };
            lSystem.Interpretations['G'] = new CommandInterpretation { ActionType = "FORWARD", Value = 1 };
            lSystem.Interpretations['f'] = new CommandInterpretation { ActionType = "FORWARD", Value = 1 };
            lSystem.Interpretations['+'] = new CommandInterpretation { ActionType = "ROTATE", Value = lSystem.Angle };
            lSystem.Interpretations['-'] = new CommandInterpretation { ActionType = "ROTATE", Value = -lSystem.Angle };
            lSystem.Interpretations['['] = new CommandInterpretation { ActionType = "SAVE" };
            lSystem.Interpretations[']'] = new CommandInterpretation { ActionType = "RESTORE" };
        }

        public static LSystem CreateFromString(string axiom, double angle, double initialDir, params (char, string)[] rules)
        {
            var lSystem = new LSystem { Axiom = axiom, Angle = angle, InitialDirection = initialDir };
            foreach (var (symbol, replacement) in rules)
            {
                lSystem.Rules[symbol] = replacement;
            }
            SetDefaultInterpretations(lSystem);
            return lSystem;
        }
    }
}