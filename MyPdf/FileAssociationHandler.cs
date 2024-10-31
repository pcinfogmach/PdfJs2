using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;

namespace MyPdf
{
    public class FileAssociationHandler
    {
        private const string PdfExtension = ".pdf";
        private const string ProgId = "MyPdf.PDF";
        private const string AppName = "MyPdfApp"; // Your app name here
        private const string SettingsFileName = "preferences.dat";
        private readonly string SettingsFilePath;

        public FileAssociationHandler()
        {
            SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName, SettingsFileName);
            EnsureSettingsFileExists();
        }

        public void HandlePdfAssociation()
        {
            if (!IsFileAssociationExists() && ShouldPromptForAssociation())
            {
                if (MessageBox.Show("Do you want to associate .pdf files with this app?", AppName, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (IsAdministrator())
                    {
                        CreateFileAssociation();
                    }
                    else
                    {
                        RestartAsAdministrator();
                    }
                }
            }
        }

        private bool IsFileAssociationExists()
        {
            using var key = Registry.ClassesRoot.OpenSubKey(PdfExtension);
            return key != null && key.GetValue(null) as string == ProgId;
        }

        private bool ShouldPromptForAssociation()
        {
            // Read the setting from the .dat file
            var skipPrompt = ReadSkipPromptSetting();
            if (skipPrompt)
            {
                return false;
            }

            // Prompt the user and update the setting file
            var result = MessageBox.Show("Do you want to see this prompt again?", AppName, MessageBoxButton.YesNo);
            WriteSkipPromptSetting(result == MessageBoxResult.No);

            return true;
        }

        private void CreateFileAssociation()
        {
            try
            {
                using var key = Registry.ClassesRoot.CreateSubKey(PdfExtension);
                key?.SetValue(null, ProgId);

                using var progIdKey = Registry.ClassesRoot.CreateSubKey(ProgId);
                progIdKey?.SetValue(null, "PDF File");

                using var shellKey = progIdKey?.CreateSubKey(@"shell\open\command");
                shellKey?.SetValue(null, $"\"{Process.GetCurrentProcess().MainModule.FileName}\" \"%1\"");

                MessageBox.Show("File association for .pdf created successfully!", AppName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating file association: {ex.Message}", AppName);
            }
        }

        private bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdministrator()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            var startInfo = new ProcessStartInfo(exeName)
            {
                UseShellExecute = true,
                Verb = "runas" // Requests admin privileges
            };

            try
            {
                Process.Start(startInfo);
                Application.Current.Shutdown(); // Close the current instance
            }
            catch
            {
                MessageBox.Show("Administrator privileges are required to create the file association.", AppName);
            }
        }

        private void EnsureSettingsFileExists()
        {
            var appDataFolder = Path.GetDirectoryName(SettingsFilePath);
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            if (!File.Exists(SettingsFilePath))
            {
                File.WriteAllText(SettingsFilePath, "SkipAssociationPrompt=False");
            }
        }

        private bool ReadSkipPromptSetting()
        {
            try
            {
                var settings = File.ReadAllText(SettingsFilePath);
                return settings.Contains("SkipAssociationPrompt=True");
            }
            catch
            {
                return false; // Default to false if there's an issue
            }
        }

        private void WriteSkipPromptSetting(bool skipPrompt)
        {
            try
            {
                File.WriteAllText(SettingsFilePath, $"SkipAssociationPrompt={(skipPrompt ? "True" : "False")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving preferences: {ex.Message}", AppName);
            }
        }
    }
}
