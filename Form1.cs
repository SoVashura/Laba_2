using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_compile
{

    public partial class Form1 : Form
    {
        private string currentFilePath = null;
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private StatusBarManager statusBar;
        private Font currentFont = new Font("Consolas", 14); // Шрифт по умолчанию
        private bool shouldClearHighlight = false;


        public Form1()
        {
            InitializeComponent();
            statusBar = new StatusBarManager(this);
            statusBar.HideCursorPosition(); // Скрываем метку при запуске
        }
        private void Editor_SelectionChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab)
            {
                if (sender is RichTextBox editor)
                {
                    statusBar.UpdateCursorPosition(editor);
                }
            }
            else
            {
                statusBar.HideCursorPosition();
            }
        }

        // Создать файл
        private void button1_Click(object sender, EventArgs e)
        {
            создатьToolStripMenuItem_Click(sender, e);
        }
        // Открыть файл
        private void button2_Click(object sender, EventArgs e)
        {
            открытьToolStripMenuItem_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            сохранитьToolStripMenuItem_Click(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            отменитьToolStripMenuItem_Click(sender, e);
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorTab newTab = new EditorTab { Text = "Новый документ" };
            SplitContainer panel = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal, // горизонтальное разделение
                SplitterWidth = 6, // толщина границы
                BorderStyle = BorderStyle.FixedSingle
            };


            // Создаем панель для нумерации строк
            Panel panelLineNumbers = new Panel { Dock = DockStyle.Left, Width = 40, BackColor = Color.LightGray };
            RichTextBox editor = new RichTextBox { Dock = DockStyle.Fill, Visible = true };

            //dont forget 
            //RichTextBox output = new RichTextBox { Dock = DockStyle.Fill, Height = 200, Visible = true, ReadOnly = true};
            DataGridView tokenTable = new DataGridView { Dock = DockStyle.Fill, Height = 200, Visible = true };
            tokenTable.AllowUserToAddRows = false;
            tokenTable.AllowUserToDeleteRows = false;
            tokenTable.ReadOnly = true;
            tokenTable.RowHeadersVisible = false;
            tokenTable.ColumnHeadersVisible = true;

            // Добавляем столбцы
            tokenTable.Columns.Add("Code", "Код");
            tokenTable.Columns.Add("Type", "Тип");
            tokenTable.Columns.Add("Value", "Лексема");
            tokenTable.Columns.Add("Start", "Начало");
            tokenTable.Columns.Add("End", "Конец");
            panelLineNumbers.Tag = currentFont; // Сохраняем шрифт в Tag

            editor.SelectionChanged += Editor_SelectionChanged;
            editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);

            editor.TextChanged += (s, ev) =>
            {
                undoTimer.Stop();
                undoTimer.Start();
                if (!newTab.Text.EndsWith("*")) newTab.Text += "*";
                panelLineNumbers.Invalidate(); // Перерисовка номеров строк
            };
            editor.KeyPress += (s, ev) =>
            {
                if (shouldClearHighlight)
                {
                    ClearErrorHighlights(editor);
                    shouldClearHighlight = false;
                }
            };

            editor.VScroll += (s, ev) => panelLineNumbers.Invalidate();
            panelLineNumbers.Paint += (s, ev) => DrawLineNumbers(ev.Graphics, editor, panelLineNumbers);

            panel.Panel1.Controls.Add(editor);
            panel.Panel1.Controls.Add(panelLineNumbers); // Добавляем панель нумерации
            //panel.Panel2.Controls.Add(output);
            panel.Panel2.Controls.Add(tokenTable);
            newTab.Controls.Add(panel);

            tabControl1.TabPages.Add(newTab);
            tabControl1.SelectedTab = newTab;

            newTab.UndoStack.Push(""); // Запоминаем пустое состояние
            editor.Font = currentFont; // Применяем текущий шрифт
            
            //output.Font = currentFont;

            tabControl1.SelectedTab.Text = "Новый документ"; // Устанавливаем имя вкладки
        }

        private void DrawLineNumbers(Graphics g, RichTextBox editor, Panel panelLineNumbers)
        {
            g.Clear(Color.LightGray);

            Font font = panelLineNumbers.Tag as Font ?? editor.Font;

            int lineHeight = TextRenderer.MeasureText("0", font).Height;
            string[] lines = editor.Text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                int y = i * lineHeight + 2;
                g.DrawString((i + 1).ToString(), font, Brushes.Black, panelLineNumbers.Width - 30, y);
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;

                EditorTab newTab = new EditorTab { Text = Path.GetFileName(filePath), FilePath = filePath };
                SplitContainer panel = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Horizontal, // горизонтальное разделение
                    SplitterWidth = 6, // толщина границы
                    BorderStyle = BorderStyle.FixedSingle
                };
                // Создаем панель для нумерации строк
                Panel panelLineNumbers = new Panel { Dock = DockStyle.Left, Width = 40, BackColor = Color.LightGray };
                panelLineNumbers.Tag = currentFont; // Сохраняем шрифт в Tag

                RichTextBox editor = new RichTextBox { Dock = DockStyle.Fill, Visible = true };
                RichTextBox output = new RichTextBox { Dock = DockStyle.Bottom, Height = 100, Visible = true, ReadOnly = true };
                editor.SelectionChanged += Editor_SelectionChanged;
                editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);
                editor.TextChanged += (s, ev) =>
                {
                    undoTimer.Stop();
                    undoTimer.Start();
                    if (!newTab.Text.EndsWith("*")) newTab.Text += "*"; // Добавляем звездочку при изменении
                    panelLineNumbers.Invalidate(); // Перерисовка номеров строк
                };
                editor.VScroll += (s, ev) => panelLineNumbers.Invalidate();
                panelLineNumbers.Paint += (s, ev) => DrawLineNumbers(ev.Graphics, editor, panelLineNumbers);
                editor.Text = File.ReadAllText(filePath);
                editor.Tag = filePath;

              
                panel.Panel1.Controls.Add(editor);
                panel.Panel1.Controls.Add(panelLineNumbers); // Добавляем панель нумерации
                panel.Panel2.Controls.Add(output);
                newTab.Controls.Add(panel);

                tabControl1.TabPages.Add(newTab);
                tabControl1.SelectedTab = newTab;
                editor.Font = currentFont; // Применяем текущий шрифт
                output.Font = currentFont;

                newTab.UndoStack.Push(editor.Text); // Запоминаем изначальное состояние файла
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    создатьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.O:
                    открытьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.S:
                    сохранитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.Shift | Keys.S:
                    сохранитьКакToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.W:
                    if (tabControl1.SelectedTab != null)
                        ЗакрытьВкладку(tabControl1.SelectedTab);
                    return true;

                case Keys.Control | Keys.Z:
                    отменитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.Y:
                    повторитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.X:
                    вырезатьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.C:
                    копироватьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.V:
                    вставитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.A:
                    выделитьВсеToolStripMenuItem_Click(null, null);
                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                saveFileDialog1.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog1.FileName, editor.Text);
                    tab.FilePath = saveFileDialog1.FileName;       // Сохраняем путь в свойство вкладки
                    editor.Tag = tab.FilePath;                      // (опционально) сохраняем путь в Tag редактора
                    tab.Text = Path.GetFileName(tab.FilePath);       // Обновляем название вкладки
                }
            }
            else
            {
                MessageBox.Show("Нет открытого документа", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (!string.IsNullOrEmpty(tab.FilePath)) // Если для вкладки уже задан путь
                {
                    File.WriteAllText(tab.FilePath, editor.Text);
                    tab.Text = Path.GetFileName(tab.FilePath);   // Обновляем название вкладки
                    editor.Tag = tab.FilePath;                     // Обновляем Tag, если он используется
                }
                else // Если файл новый, вызываем "Сохранить как"
                {
                    сохранитьКакToolStripMenuItem_Click(sender, e);
                }
            }
            else
            {
                MessageBox.Show("Нет открытого документа", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab selectedTab)
            {
                SplitContainer panel = selectedTab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                // Обновляем позицию курсора при смене вкладки
                statusBar.UpdateCursorPosition(editor);
            }
            else
            {
                // Если вкладок нет, скрываем метку позиции
                statusBar.HideCursorPosition();
            }
            if (tabControl1.SelectedTab != null)
            {
                SplitContainer panel = tabControl1.SelectedTab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor != null && !string.IsNullOrEmpty(editor.Text))
                {
                    currentFilePath = editor.Tag as string; // Используем Tag для хранения пути
                }
            }
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {

            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                if (tabPage is EditorTab tab)
                {
                    ЗакрытьВкладку(tab);
                    if (tabControl1.TabPages.Contains(tab)) // Если вкладка не закрылась (отмена)
                    {
                        return;
                    }
                }
            }

            // Если все вкладки обработаны — выходим из приложения
            Application.Exit();
        }
        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (tab.UndoStack.Count > 1) // Первый элемент — это начальное состояние
                {
                    tab.RedoStack.Push(tab.UndoStack.Pop());
                    editor.Text = tab.UndoStack.Peek();
                    editor.SelectionStart = editor.Text.Length;
                }
            }
        }
        private bool IsFileAlreadyOpen(string filePath)
        {
            foreach (TabPage tab in tabControl1.TabPages)
            {
                if (tab is EditorTab editorTab && editorTab.FilePath == filePath)
                {
                    tabControl1.SelectedTab = tab; // Просто активируем вкладку
                    return true;
                }
            }
            return false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            tabControl1.AllowDrop = true;  // Включаем поддержку на уровне вкладок

            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            tabControl1.DragEnter += new DragEventHandler(Form1_DragEnter);
            tabControl1.DragDrop += new DragEventHandler(Form1_DragDrop);
            foreach (TabPage tab in tabControl1.TabPages)
            {
                if (tab is EditorTab editorTab)
                {
                    SplitContainer panel = editorTab.Controls[0] as SplitContainer;
                    RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;
                    editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);
                }
            }

        }
        private void ApplySyntaxHighlighting(RichTextBox editor)
        {
            int selectionStart = editor.SelectionStart;
            int selectionLength = editor.SelectionLength;

            string[] keywords = { "int", "float", "double", "string", "if", "else", "while", "for", "return", "void", "char", "struct" };
            string[] operators = { "+", "-", "*", "/", "=", "==", "!=", "{", "}", "<=", ">=" };

            editor.SuspendLayout();
            int cursorPosition = editor.SelectionStart;

            // Очищаем всю подсветку
            editor.SelectAll();
            editor.SelectionColor = Color.Black;

            // Подсвечиваем ключевые слова (синий)
            foreach (string keyword in keywords)
            {
                HighlightWord(editor, keyword, Color.Blue);
            }

            // Подсвечиваем числа (фиолетовый)
            HighlightRegex(editor, @"\b\d+\b", Color.Purple);

            // Подсвечиваем строки в кавычках (зеленый)
            HighlightRegex(editor, "\".*?\"", Color.Green);

            // Подсвечиваем операторы (красный)
            foreach (string op in operators)
            {
                HighlightWord(editor, op, Color.Red);
            }

            editor.SelectionStart = cursorPosition;
            editor.SelectionLength = 0;
            editor.SelectionColor = Color.Black;
            editor.ResumeLayout();

        }
        private void HighlightWord(RichTextBox editor, string word, Color color)
        {
            int index = 0;
            while ((index = editor.Text.IndexOf(word, index)) != -1)
            {
                editor.Select(index, word.Length);
                editor.SelectionColor = color;
                index += word.Length;
            }
        }
        private void HighlightRegex(RichTextBox editor, string pattern, Color color)
        {
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(editor.Text, pattern))
            {
                editor.Select(match.Index, match.Length);
                editor.SelectionColor = color;
            }
        }
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // Разрешаем копирование
            }
            else
            {
                e.Effect = DragDropEffects.None; // Отклоняем остальные типы
            }
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string filePath in files) // Если тянут несколько файлов, открываем все
            {
                if (Path.GetExtension(filePath).ToLower() != ".txt") // Только txt-файлы
                {
                    MessageBox.Show($"Файл {Path.GetFileName(filePath)} не поддерживается!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                if (IsFileAlreadyOpen(filePath))
                {
                    continue; // Файл уже открыт, пропускаем его
                }

                EditorTab newTab = new EditorTab { Text = Path.GetFileName(filePath), FilePath = filePath };
                SplitContainer panel = new SplitContainer { Dock = DockStyle.Fill };
                Panel panelLineNumbers = new Panel { Dock = DockStyle.Left, Width = 40, BackColor = Color.LightGray };
                RichTextBox editor = new RichTextBox { Dock = DockStyle.Fill, Visible = true };
                RichTextBox output = new RichTextBox { Dock = DockStyle.Bottom, Height = 100, Visible = true, ReadOnly = true };
                panelLineNumbers.Tag = currentFont; // Сохраняем шрифт в Tag
                editor.SelectionChanged += Editor_SelectionChanged;
                editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);
                editor.TextChanged += (s, ev) =>
                {
                    undoTimer.Stop();
                    undoTimer.Start();
                    if (!newTab.Text.EndsWith("*")) newTab.Text += "*"; // Добавляем звездочку при изменении
                    panelLineNumbers.Invalidate(); // Перерисовка номеров строк
                };
                editor.VScroll += (s, ev) => panelLineNumbers.Invalidate();
                panelLineNumbers.Paint += (s, ev) => DrawLineNumbers(ev.Graphics, editor, panelLineNumbers);
                editor.Text = File.ReadAllText(filePath);
                editor.Tag = filePath;

                panel.Panel1.Controls.Add(editor);
                panel.Panel1.Controls.Add(panelLineNumbers); // Добавляем панель нумерации
                panel.Panel2.Controls.Add(output);
                newTab.Controls.Add(panel);

                tabControl1.TabPages.Add(newTab);
                tabControl1.SelectedTab = newTab;
                editor.Font = currentFont; // Применяем текущий шрифт
                output.Font = currentFont;

                newTab.UndoStack.Push(editor.Text); // Запоминаем изначальное состояние файла
            }
        }
        private void ClearErrorHighlights(RichTextBox editor)
        {
            int cursorPos = editor.SelectionStart;

            editor.SelectAll();
            editor.SelectionBackColor = editor.BackColor; // Сброс фона
            editor.SelectionStart = cursorPos;
            editor.SelectionLength = 0;
        }
        private void undoTimer_Tick(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (tab.UndoStack.Count == 0 || tab.UndoStack.Peek() != editor.Text)
                {
                    tab.UndoStack.Push(editor.Text); // Запоминаем состояние текста
                    tab.RedoStack.Clear();          // Очищаем redo, если пользователь что-то ввел
                }

                if (tab.RedoStack.Count > 0 && tab.RedoStack.Peek() == editor.Text)
                {
                    tab.RedoStack.Pop(); // Если повтор случайно закинул текущее состояние — убираем его
                }
            }
            undoTimer.Stop();
        }
        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (tab.RedoStack.Count > 0)
                {
                    string text = tab.RedoStack.Pop();  // Забираем текст из Redo
                    tab.UndoStack.Push(text);           // Пихаем обратно в Undo
                    editor.Text = text;                // Показываем в редакторе
                    editor.SelectionStart = editor.Text.Length;

                    undoTimer.Stop();
                    undoTimer.Start(); // Снова запускаем таймер
                }
            }
        }
        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor.SelectedText.Length > 0)
                {
                    Clipboard.SetText(editor.SelectedText);
                }
            }
        }
        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (Clipboard.ContainsText())
                {
                    editor.SelectedText = Clipboard.GetText(); // Вставляем в место выделения
                }
            }
        }
        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor.SelectedText.Length > 0)
                {
                    Clipboard.SetText(editor.SelectedText);
                    editor.SelectedText = ""; // Заменяем выделенный текст пустотой
                }
            }
        }
        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                editor.SelectAll();
            }
        }
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor.SelectedText.Length > 0)
                {
                    editor.SelectedText = ""; // Удаляет выделенный текст
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            повторитьToolStripMenuItem_Click(sender, e);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            копироватьToolStripMenuItem_Click(sender, e);
        }
        private void button7_Click(object sender, EventArgs e)
        {
            вырезатьToolStripMenuItem_Click(sender, e);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            вставитьToolStripMenuItem_Click(sender, e);
        }
        private void ЗакрытьВкладку(TabPage tab)
        {
            if (tab is EditorTab editorTab)
            {
                SplitContainer panel = editorTab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                // Проверяем, есть ли изменения
                if (editorTab.Text.EndsWith("*"))
                {
                    DialogResult result = MessageBox.Show(
                        $"Сохранить изменения в \"{editorTab.Text.TrimEnd('*')}\"?",
                        "Сохранение файла",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes) // Если выбрано "Сохранить"
                    {
                        if (string.IsNullOrEmpty(editorTab.FilePath)) // Новый файл, сохраняем как...
                        {
                            сохранитьКакToolStripMenuItem_Click(null, null);
                        }
                        else
                        {
                            File.WriteAllText(editorTab.FilePath, editor.Text);
                        }
                    }
                    else if (result == DialogResult.Cancel) // Если выбрано "Отмена"
                    {
                        return; // Не закрываем вкладку
                    }
                }

                tabControl1.TabPages.Remove(tab); // Удаляем вкладку
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                if (tabPage is EditorTab tab)
                {
                    ЗакрытьВкладку(tab);
                    if (tabControl1.TabPages.Contains(tab)) // Если вкладка не закрылась (отмена)
                    {
                        e.Cancel = true; // Прерываем закрытие программы
                        return;
                    }
                }
            }
        }
        private void шрифтToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.ShowColor = false;
                fontDialog.Font = currentFont; // Используем текущий шрифт

                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFont = fontDialog.Font; // Сохраняем выбранный шрифт

                    foreach (TabPage tab in tabControl1.TabPages)
                    {
                        if (tab is EditorTab editorTab)
                        {
                            SplitContainer panel = editorTab.Controls[0] as SplitContainer;
                            RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;
                            RichTextBox output = panel.Panel2.Controls.Count >= 1 ? panel.Panel2.Controls[0] as RichTextBox : null;

                            editor.Font = currentFont;
                            if (output != null) output.Font = currentFont;
                            Panel panelLineNumbers = panel.Panel1.Controls
                            .OfType<Panel>()
                            .FirstOrDefault(p => p.Dock == DockStyle.Left);

                            if (panelLineNumbers != null)
                            {
                                panelLineNumbers.Tag = currentFont;
                                panelLineNumbers.Invalidate(); // Перерисовать номера строк
                            }
                        }
                    }
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
            "Название программы: Лексический анализатор структуры на языке C\nАвтор: Вашурина С.И.\nГруппа: АП-226\nДисциплина: Теория формальных языков и компиляторов \n",
            "О программе",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
            );
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
            "Название программы: Лексический анализатор структуры на языке C\nАвтор: Вашурина С.И.\nГруппа: АП-226\nДисциплина: Теория формальных языков и компиляторов \n",
            "О программе",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
            );
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "About.html");

            try
            {
                // Сохраняем встроенный HTML в файл
                File.WriteAllText(tempPath, Properties.Resources.about);
                // Открываем в браузере
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;
                //RichTextBox outputBox = panel.Controls[2] as RichTextBox; // Вывод ошибок в нижний RichTextBox
                DataGridView table = panel.Panel2.Controls[0] as DataGridView;
                Lexer lexer = new Lexer(editor.Text);
                List<Token> tokens = lexer.Tokenize();
                List<ParseError> errors = lexer.GetErrors();


                DisplayTokens(table, tokens);
                DisplayParseErrors(errors);
            }
        }

        private void DisplayTokens(DataGridView tokenTable, List<Token> tokens)
        {
            tokenTable.Rows.Clear();

            foreach (var token in tokens)
            {
                int rowIndex = tokenTable.Rows.Add(token.Code, token.Type, token.Value, token.Start, token.End);

                // Если токен — ошибка, выделяем строку красным
                if (token.Code == -1)
                {
                    tokenTable.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Red;
                    tokenTable.Rows[rowIndex].DefaultCellStyle.Font = new Font(tokenTable.Font, FontStyle.Bold);
                }
            }
            
        }

        private void DisplayParseErrors(List<ParseError> errors)
        {

            if (errors.Count == 0)
            {
                MessageBox.Show(
                    "Ошибок нет.",
            "Результат",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
            );
            }
            else
            {
                string temp = "";
                foreach (var error in errors)
                {
                    temp += $"{error.Message} (позиция: {error.Position})\n";
                }
                {
                    MessageBox.Show(
                        temp,
                "Результат",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );
                }
            }
        }
    }
}