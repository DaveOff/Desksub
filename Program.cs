using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;

namespace Desksub
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (IsNullOrEmpty(args))
            {
                MessageBox.Show("NULL");
                System.Environment.Exit(1);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new mainForm(args));
        }

        static bool IsNullOrEmpty(string[] myStringArray)
        {
            return myStringArray == null || myStringArray.Length < 1;
        }
    }
}
