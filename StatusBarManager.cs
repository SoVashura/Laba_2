using System;
using System.Linq;
using System.Windows.Forms;

namespace Lab1_compile
{
    public class StatusBarManager
    {
        private readonly ToolStripStatusLabel _positionLabel;
        private readonly Form _form;

        public StatusBarManager(Form form)
        {
            _form = form;

            // Создаем StatusStrip, если его нет
            if (_form.Controls.OfType<StatusStrip>().FirstOrDefault() == null)
            {
                var statusStrip = new StatusStrip();
                _positionLabel = new ToolStripStatusLabel();
                statusStrip.Items.Add(_positionLabel);
                _form.Controls.Add(statusStrip);
            }
            else
            {
                _positionLabel = _form.Controls.OfType<StatusStrip>().First().Items.OfType<ToolStripStatusLabel>().FirstOrDefault();
                if (_positionLabel == null)
                {
                    _positionLabel = new ToolStripStatusLabel();
                    _form.Controls.OfType<StatusStrip>().First().Items.Add(_positionLabel);
                }
            }
        }

        public void UpdateCursorPosition(RichTextBox editor)
        {
            if (editor == null)
            {
                HideCursorPosition();
                return;
            }

            int line = editor.GetLineFromCharIndex(editor.SelectionStart) + 1;
            int column = editor.SelectionStart - editor.GetFirstCharIndexFromLine(line - 1) + 1;

            _positionLabel.Text = $"Строка: {line}, Колонка: {column}";
            _positionLabel.Visible = true;
        }

        public void HideCursorPosition()
        {
            _positionLabel.Visible = false;
        }
    }
}
