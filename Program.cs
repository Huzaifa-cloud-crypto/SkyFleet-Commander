using System;
using System.Windows.Forms;

namespace AGDRONE
{
    static class Program
    {
  
        ///  The main entry point for the application.
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Ensure Database is initialized
            DatabaseManager.InitializeDatabase();
            
            Application.Run(new MainForm());
        }
    }
}
