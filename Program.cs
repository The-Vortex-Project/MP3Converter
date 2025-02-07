using System.Security.Cryptography.Xml;
using System.IO;
using System.Diagnostics;
using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MP3ConverterTray
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
            {
                string inputFile = args[0];

                // Ensure the file exists
                if (!File.Exists(inputFile))
                {
                    MessageBox.Show("File not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Start conversion
                ConvertWavToMp3(inputFile);
                return;
            }

            // No file passed, start as tray app
            Application.Run(new TrayForm());
        }
        private static string GetBitrateSetting()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\MP3Converter"))
            {
                return key?.GetValue("Bitrate") as string ?? "128k";  // Default to 128k
            }
        }

        private static void ConvertWavToMp3(string inputFile)
        {
            string outputFile = Path.ChangeExtension(inputFile, ".mp3");
            string bitrate = GetBitrateSetting();  // Load saved bitrate

            string appDirectory = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
            string ffmpegPath = Path.Combine(appDirectory, "ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                MessageBox.Show("FFmpeg is missing! Please place 'ffmpeg.exe' in the same folder as this app.",
                    "Missing FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputFile}\" -b:a {bitrate} \"{outputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process? ffmpeg = Process.Start(psi))
                {
                    if (ffmpeg == null)
                    {
                        MessageBox.Show("Failed to start FFmpeg.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ffmpeg.WaitForExit();
                }

                ShowNotification("MP3 Conversion Complete", $"Saved: {outputFile}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting file: {ex.Message}", "Conversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private static void ShowNotification(string title, string message)
        {
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Information
            };

            notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }
    }
}