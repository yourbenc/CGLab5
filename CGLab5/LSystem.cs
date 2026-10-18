using System;
using System.Collections.Generic;
using System.Text;

namespace CGLab5
{
    public class LSystem
    {
        public string Axiom { get; set; }
        public Dictionary<char, string> Rules { get; set; }
        public double Angle { get; set; }
        public double InitialDirection { get; set; }
        public bool UseRandomness { get; set; }

        public LSystem()
        {
            Rules = new Dictionary<char, string>();
            UseRandomness = false;
        }

        public string Generate(int iterations)
        {
            string current = Axiom;
            for (int i = 0; i < iterations; i++)
            {
                StringBuilder next = new StringBuilder();
                foreach (char c in current)
                {
                    if (Rules.ContainsKey(c))
                        next.Append(Rules[c]);
                    else
                        next.Append(c);
                }
                current = next.ToString();
            }
            return current;
        }
    }
}