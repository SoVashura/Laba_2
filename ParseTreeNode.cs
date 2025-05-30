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
        public List<ParseTreeNode> Children { get; } = new List<ParseTreeNode>();

        public ParseTreeNode(string nodeType, string value = null, int position = -1)
        {
            NodeType = nodeType;
            Value = value;
            Position = position;
        }

        public void AddChild(ParseTreeNode node)
        {
            if (node != null)
                Children.Add(node);
        }
    }
}
