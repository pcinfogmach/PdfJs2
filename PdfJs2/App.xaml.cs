using System.Configuration;
using System.Data;
using System.Windows;

namespace PdfJs2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check if there's a file path in the command-line arguments
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                new MainWindow(filePath).Show();
            }
            else
            {
                new MainWindow().Show();
            }
        }
    }
}
