using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    internal class Token
    {
        public int Code { get; }
        public string Type { get; }
        public string Value { get; }
        public int Start { get; }
        public int End { get; }

        public Token(int code, string type, string value, int start, int end)
        {
            Code = code;
            Type = type;
            Value = value;
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"{Code} - {Type} - {Value} ({Start}-{End})";
        }
    }
}