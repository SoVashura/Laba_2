using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    public class MatchResult
    {
        public string Value { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }

        public MatchResult(string value, int position, int length)
        {
            Value = value;
            Position = position;
            Length = length;
        }
    }
}
