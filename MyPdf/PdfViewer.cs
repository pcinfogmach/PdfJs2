using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PdfJs2
{
    public class PdfViewer : WebView2
    {
        public string pdfPath = string.Empty;
        string pdfViewerPath = string.Empty;
        TabItem tabItem;
        private string allowedUrl;
        private DispatcherTimer dotAnimationTimer;
        private int dotCount = 0; // To keep track of the number of dots to display

        public PdfViewer()
        {
            try
            {
                this.CoreWebView2InitializationCompleted += Browser_CoreWebView2InitializationCompleted;
                this.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public PdfViewer(string filePath, TabItem tab)
        {
            try
            {
                tab.Content = this;
                tabItem = tab;
                tabItem.MinWidth = 100;
                tabItem.Header = Path.GetFileName(filePath);
                pdfPath = filePath;
                InitializeViewer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async void InitializeViewer()
        {
            try
            {
                pdfViewerPath = ZipHelper.GetViewerDirectory();
                string fileName = Path.GetFileName(pdfPath);

                allowedUrl = $"https://pdfjs/web/viewer.html?file={Uri.EscapeDataString(fileName)}";

                string targetPath = Path.Combine(pdfViewerPath, "web", fileName);
                File.Copy(pdfPath, targetPath, true); // Copy the selected PDF file to the pdfjs web folder

                this.CoreWebView2InitializationCompleted += Browser_CoreWebView2InitializationCompleted;
                await this.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Browser_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            try
            {
                if (e.IsSuccess)
                {
                    //this.CoreWebView2.ContentLoading += (s, e) => 
                    //{
                    //    // Initialize the timer for dot animation
                    //    dotAnimationTimer = new DispatcherTimer
                    //    {
                    //        Interval = TimeSpan.FromMilliseconds(10) // Update every 500 ms
                    //    };
                    //    dotAnimationTimer.Tick += DotAnimationTimer_Tick;
                    //    dotAnimationTimer.Start(); // Start the animation timer
                    //};
                    //this.CoreWebView2.DOMContentLoaded += (s, e) => { if (dotAnimationTimer != null) dotAnimationTimer.Stop(); tabItem.Header = Path.GetFileName(pdfPath); };

                    this.CoreWebView2.SetVirtualHostNameToFolderMapping("pdfjs", pdfViewerPath, CoreWebView2HostResourceAccessKind.DenyCors);

                    this.Source = new Uri(allowedUrl);

                    this.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting; // Add a handler to restrict navigation
                    this.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
                    this.CoreWebView2.WebMessageReceived += Viewer_WebMessageReceived;
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //private void DotAnimationTimer_Tick(object sender, EventArgs e)
        //{
        //    int MaxDots = 5; // Maximum number of dots to display
        //    dotCount = (dotCount + 1) % (MaxDots + 1); // Loop from 0 to MaxDots

        //    string dots = new string('❍', dotCount); // Generate a string with the current number of dots
        //    int spacesCount = MaxDots - dotCount; // Calculate the number of spaces needed
        //    string spaces = new string(' ', spacesCount); // Generate spaces to fill the header

        //    tabItem.Header = Path.GetFileName(pdfPath) + dots + spaces; // Update the header with dots followed by spaces
        //}


        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri != allowedUrl) // Cancel navigation if it's not to the allowed URL
                e.Cancel = true;
        }

        private void Viewer_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = JsonSerializer.Deserialize<Dictionary<string, string>>(e.WebMessageAsJson);

                if (message != null && message.TryGetValue("action", out var actionName))
                {
                    // Use reflection to find and invoke the method by name
                    var method = this.GetType().GetMethod(actionName, BindingFlags.NonPublic | BindingFlags.Instance);
                    method?.Invoke(this, null); // Calls the method if it exists, passing no parameters
                }
                else
                {
                    MessageBox.Show("No action defined!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        bool isSaveAs = false;
        void SaveAs()
        {
            try
            {
                isSaveAs = true;
                this.ExecuteScriptAsync("PDFViewerApplication.download();");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CoreWebView2_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            try
            {
                string downloadPath;

                if (isSaveAs)
                {
                    var saveFileDialog = new SaveFileDialog();

                    saveFileDialog.Title = "Save PDF As";
                    saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    saveFileDialog.FileName = Path.GetFileName(pdfPath); // Suggest the current file name

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        downloadPath = saveFileDialog.FileName;
                        if (!downloadPath.EndsWith(".pdf")) downloadPath += ".pdf";
                        if (File.Exists(downloadPath))
                        {
                            var result = MessageBox.Show(
                            "File already exists. Do you want to overwrite it?", "Confirm Overwrite",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.No) { e.Cancel = true; }
                        }
                        e.ResultFilePath = downloadPath; // Set the download location
                    }
                    else
                    {
                        e.Cancel = true;// Cancel download if user cancels the Save As dialog
                    }
                    isSaveAs = false;
                }
                else
                {
                    string customDownloadFolder = Path.GetDirectoryName(pdfPath);
                    if (!Directory.Exists(customDownloadFolder)) Directory.CreateDirectory(customDownloadFolder);

                    downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        Path.GetFileName(pdfPath));

                    e.ResultFilePath = downloadPath;
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
