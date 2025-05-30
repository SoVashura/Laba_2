using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_compile
{
    public class EditorTab : TabPage
    {
        public Stack<string> UndoStack { get; } = new Stack<string>();
        public Stack<string> RedoStack { get; } = new Stack<string>();
        public string FilePath { get; set; }

        public EditorTab()
        {
            UndoStack.Push(""); // Инициализация пустым состоянием
        }
    }
}
