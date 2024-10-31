using System.IO;
using System.IO.Compression;
using System.Windows;

namespace PdfJs2
{
    public static class ZipHelper
    {
        public static string GetViewerDirectory()
        {
            string extractPath = string.Empty;
            
            try
            {
                string appPath =  AppDomain.CurrentDomain.BaseDirectory;
                if (!Directory.Exists(appPath)) { Directory.CreateDirectory(appPath); }
                string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdfjs.zip");
                extractPath = Path.Combine(appPath, "pdfjs");
                string viewerPath = Path.Combine(extractPath, "web", "viewer.html");

                if (!File.Exists(viewerPath))
                {
                    ZipFile.ExtractToDirectory(zipPath, extractPath, true);
                    Console.WriteLine("Extraction complete.");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"An error occurred: {ex.Message}");
            }

            return extractPath;
        }
    }
}
