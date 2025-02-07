using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Principal;


namespace MP3ConverterTray
{
    public partial class TrayForm : Form
    {
        private ContextMenuStrip trayMenu = new ContextMenuStrip();  // Initialize here

        public TrayForm()
        {
            InitializeComponent();
        }

        private void TrayForm_Load(object sender, EventArgs e)
        {
            // Ensure the form stays minimized and hidden
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Setup the system tray menu
            SetupTrayMenu();
            RegisterContextMenu();
            EnsureFfmpegExists();
        }

        private void SetupTrayMenu()
        {
            // Add menu items
            trayMenu.Items.Add("Settings", null, OpenSettings);
            trayMenu.Items.Add("Quit", null, ExitApplication);

            // Attach the menu to notifyIcon1 (from the Toolbox)
            notifyIcon1.ContextMenuStrip = trayMenu;
        }

        private void OpenSettings(object? sender, EventArgs e)
        {
            new SettingsForm().ShowDialog();
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void RegisterContextMenu()
        {
            try
            {
                if (!IsRunningAsAdministrator())
                {
                    // Relaunch as Admin
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    try
                    {
                        Process.Start(psi);
                        Application.Exit();
                    }
                    catch
                    {
                        MessageBox.Show("Administrator permission is required to add the right-click menu.",
                            "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }

                string appPath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                if (string.IsNullOrEmpty(appPath))
                {
                    MessageBox.Show("Error: Unable to determine application path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Change the Registry Key Name to "Convert to MP3"
                string registryKeyPath = @"HKEY_CLASSES_ROOT\SystemFileAssociations\.wav\shell\Convert to MP3";
                string commandKeyPath = registryKeyPath + @"\command";
                string commandValue = $"\"{appPath}\" \"%1\"";

                // Update registry for right-click menu
                Registry.SetValue(registryKeyPath, "", "Convert to MP3");  // This sets the visible text in the menu
                Registry.SetValue(commandKeyPath, "", commandValue);  // This sets the command

                //MessageBox.Show("Right-click 'Convert to MP3' has been updated!", "Registry Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating context menu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureFfmpegExists()
        {
            string? appDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            if (string.IsNullOrEmpty(appDirectory))  // Check if it's null or empty
            {
                MessageBox.Show("Error: Unable to determine application directory.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string ffmpegPath = Path.Combine(appDirectory, "ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                MessageBox.Show("FFmpeg is missing! Please place 'ffmpeg.exe' in the same folder as this app.",
                    "Missing FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }


    }
}