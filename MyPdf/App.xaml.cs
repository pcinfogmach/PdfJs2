using System.Configuration;
using System.Data;
using System.Windows;

namespace MyPdf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            new PdfJs2.MainWindow().Show();
        }
    }

}
