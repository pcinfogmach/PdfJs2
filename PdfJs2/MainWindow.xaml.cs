using System.Text;
using System.Windows;
using System.IO;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Text.Json;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.Json.Serialization;

namespace PdfJs2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string settingsFilePath = "PdfJs2WindowStateSettings.json";
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => { openFile(); };
        }

        public MainWindow(string filePath)
        {
            InitializeComponent();
            openPdfFile(filePath);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (File.Exists(settingsFilePath))
            {
                var json = File.ReadAllText(settingsFilePath);
                var windowStateSettings = JsonSerializer.Deserialize<WindowStateSettings>(json);

                if (windowStateSettings != null)
                {
                    this.Top = Math.Max(SystemParameters.VirtualScreenTop, Math.Min(windowStateSettings.Top, SystemParameters.VirtualScreenHeight - this.Height));
                    this.Left = Math.Max(SystemParameters.VirtualScreenLeft, Math.Min(windowStateSettings.Left, SystemParameters.VirtualScreenWidth - this.Width));
                    this.Width = windowStateSettings.Width;
                    this.Height = windowStateSettings.Height;
                    this.WindowState = windowStateSettings.WindowState;
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            var windowStateSettings = new WindowStateSettings
            {
                WindowState = this.WindowState
            };

            if (this.WindowState == WindowState.Normal) // Save only if window is not maximized/minimized
            {
                windowStateSettings.Top = this.Top;
                windowStateSettings.Left = this.Left;
                windowStateSettings.Width = this.Width;
                windowStateSettings.Height = this.Height;
            }

            // Serialize settings to JSON
            var json = JsonSerializer.Serialize(windowStateSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }

        void openFile()
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    Title = "Select a PDF File"
                };

                if (openFileDialog.ShowDialog() == true)
                    openPdfFile(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            NoTabsLeftMessage();
        }

        void openPdfFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string targetFolder = Path.Combine(appPath, "pdfjs", "web");
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
            string targetPath = Path.Combine(targetFolder, fileName);

            File.Copy(filePath, targetPath, true); // Copy the selected PDF file to the pdfjs folder

            var pdfViewer = new PdfViewer(filePath);
            pdfViewer.WebMessageReceived += Viewer_WebMessageReceived;
            tabControl.Items.Add(new TabItem
            {
                Header = fileName,
                Content = pdfViewer,
                IsSelected = true,
            });
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void X_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                removeTabItem(button.Tag as TabItem);
        }

        void removeTabItem(TabItem tabItem)
        {
            if (tabItem != null)
            {
                PdfViewer pdfViewer = tabItem.Content as PdfViewer;
                if (pdfViewer != null) pdfViewer.Dispose();
                tabControl.Items.Remove(tabItem);
                NoTabsLeftMessage();
            }
        }

        void NoTabsLeftMessage()
        {
            if (tabControl.Items.Count == 0)
            {
                var result = MessageBox.Show("אין ספרים פתוחים, האם ברצונך לסגור את התוכנה?", "אין ספרים פתוחים", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                if (result == MessageBoxResult.Yes)
                    this.Close();
                else
                    openFile();
            }         
        }

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }


    public class WindowStateSettings
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public System.Windows.WindowState WindowState { get; set; }
    }
}