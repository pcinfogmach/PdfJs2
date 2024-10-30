using System.Configuration;
using System.Data;
using System.IO.Pipes;
using System.IO;
using System.Windows;
using PdfJs2;

namespace MyPdf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string PipeName = "MyPdfPipe";
        private Mutex appMutex;
        private MainWindow window;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                bool isFirstInstance;
                appMutex = new Mutex(true, "MyPdfAppMutex", out isFirstInstance);

                if (isFirstInstance)
                {
                    StartPipeServer(); // Start the named pipe server
                    InitializeWindow(e.Args); // Initialize the main window
                }
                else
                {
                    SendFilePathToRunningInstance(e.Args); // Send path to the existing instance
                    Shutdown(); // Close this instance
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.OnStartup(e);
        }

        private void InitializeWindow(string[] args)
        {
            window = new MainWindow();
            window.Show();

            if (args.Length > 0)
            {
                string filePath = args[0];
                window.openPdfFile(filePath);
            }
        }

        private void StartPipeServer()
        {
            Task.Run(async () =>
            {
                while (true) // Keep the server running to handle multiple connections
                {
                    try
                    {
                        using (var pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                        {
                            await pipeServer.WaitForConnectionAsync(); // Wait for a client connection

                            using (var reader = new StreamReader(pipeServer))
                            {
                                string filePath = await reader.ReadLineAsync();
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    // Use the dispatcher to interact with the UI thread
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        window.openPdfFile(filePath);
                                        window.Activate();

                                        if (window.WindowState == WindowState.Minimized)
                                            window.WindowState = WindowState.Normal;
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Pipe server error: {ex.Message}");
                    }
                }
            });
        }

        private void SendFilePathToRunningInstance(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous))
                    {
                        pipeClient.Connect(1000); // Timeout after 1 second

                        using (var writer = new StreamWriter(pipeClient) { AutoFlush = true })
                        {
                            writer.WriteLine(args[0]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to communicate with the running instance: {ex.Message}");
                }
            }
        }
    }

}
