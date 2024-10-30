using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PdfJs2
{
    public static class ZipHelper
    {
        public static string GetViewerDiectory()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string zipPath = Path.Combine(appPath, "pdfjs.zip");
            string extractPath = Path.Combine(appPath, "pdfjs");
            string viewerDirectory = Path.Combine(extractPath, "web");
            string viewerPath = Path.Combine(viewerDirectory, "viewerDirectory.html");

            if (!Directory.Exists(viewerDirectory) ||
                !File.Exists(viewerPath))
            {
                try
                {
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                    Console.WriteLine("Extraction complete.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }

            return extractPath;
        }
    }
}
