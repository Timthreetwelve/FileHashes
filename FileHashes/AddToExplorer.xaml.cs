// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace FileHashes
{
    /// <summary>
    /// Interaction logic for AddToExplorer.xaml
    /// </summary>
    public partial class AddToExplorer : Window
    {
        private readonly string appPath = Assembly.GetExecutingAssembly().Location;

        public AddToExplorer()
        {
            InitializeComponent();
        }

        private void BtnAddToExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey shell = Registry.CurrentUser.OpenSubKey(@"Software\Classes\*", true))
                {
                    if (shell == null)
                    {
                        lblStatus.Foreground = Brushes.Red;
                        lblStatus.Text = "Required registry key Not Found";
                        return;
                    }

                    shell.CreateSubKey(@"shell\FileHashes", true);

                    using (RegistryKey hash = shell.OpenSubKey(@"shell\FileHashes", true))
                    {
                        hash.SetValue("", "File Hashes");
                        hash.SetValue("Icon", $"{appPath},0");
                        hash.CreateSubKey("command");

                        using (RegistryKey cmd = hash.OpenSubKey("command", true))
                        {
                            cmd.SetValue("", $"\"{appPath}\" \"%1\"");
                            cmd.Close();
                        }
                        hash.Close();
                    }
                    shell.Close();
                }
                lblStatus.Foreground = Brushes.Black;
                lblStatus.Text = "Registry key added";
            }
            catch (Exception ex)
            {
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = ex.Message;
            }
        }

        private void BtnRemoveExplorer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey shell = Registry.CurrentUser.OpenSubKey(@"Software\Classes\*\shell", true))
                {
                    using (RegistryKey hash = shell.OpenSubKey("FileHashes", false))
                    {
                        if(hash == null)
                        {
                            lblStatus.Foreground = Brushes.Black;
                            lblStatus.Text = "Registry key not found - nothing to remove";
                        }
                        else
                        {
                            hash.Close();
                            shell.DeleteSubKeyTree("FileHashes");
                            shell.Close();
                            lblStatus.Foreground = Brushes.Black;
                            lblStatus.Text = "Registry key removed";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Text = ex.Message;
            }
        }
    }
}
