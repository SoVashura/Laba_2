using System;
using System.Windows.Forms;

namespace Lab1_compile
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Главная форма вашего приложения
        }
    }
}