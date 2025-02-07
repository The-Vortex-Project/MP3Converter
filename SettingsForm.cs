using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP3ConverterTray
{
    public partial class SettingsForm : Form
    {
        private const string RegistryPath = @"Software\MP3Converter";

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                string savedBitrate = key?.GetValue("Bitrate") as string ?? "192k";  // Default to 192k if not set
                comboBox1.SelectedItem = savedBitrate;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string selectedBitrate = comboBox1.SelectedItem?.ToString() ?? "128k";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("Bitrate", selectedBitrate);
            }

            MessageBox.Show("Settings saved!", "MP3 Converter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
