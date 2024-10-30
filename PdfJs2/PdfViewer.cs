using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;

namespace PdfJs2
{
    public class PdfViewer : WebView2
    {
        string filePath = string.Empty;
        public PdfViewer()
        {
            this.CoreWebView2InitializationCompleted += Browser_CoreWebView2InitializationCompleted;
            this.EnsureCoreWebView2Async();
        }

        public PdfViewer(string path)
        {
            filePath = path;
            this.CoreWebView2InitializationCompleted += Browser_CoreWebView2InitializationCompleted;
            this.EnsureCoreWebView2Async();
        }

        private void Browser_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            string pdfPath = ZipHelper.GetViewerDiectory();
            this.CoreWebView2.SetVirtualHostNameToFolderMapping("pdfjs", pdfPath, CoreWebView2HostResourceAccessKind.DenyCors);
            this.Source = new Uri($"https://pdfjs/web/viewer.html?file={Path.GetFileName(filePath)}");
        }    
    }
}
