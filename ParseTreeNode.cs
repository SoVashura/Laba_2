using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    public class ParseTreeNode
    {
        public string NodeType { get; }
        public string Value { get; }
        public int Position { get; }
        public List<ParseTreeNode> Children { get; }

        public ParseTreeNode(string nodeType, string value = null, int position = -1)
        {
            NodeType = nodeType;
            Value = value;
            Position = position;
            Children = new List<ParseTreeNode>();
        }
    }
}
