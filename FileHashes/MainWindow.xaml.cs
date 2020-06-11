﻿// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace FileHashes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region private variables
        private const int bufferSize = 16 * 1024 * 1024;
        private string fileName;
        private string result;
        private readonly Properties.Settings PSettings = Properties.Settings.Default;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CommandLineArgs();
        }

        #region Get filename passed as command line argument
        private void CommandLineArgs()
        {
            string[] args = App.Args;

            if (args != null)
            {
                tbxFileName.Text = args[0].Trim();
                CalculateHashes();
                Debug.WriteLine("||| Processing command line");
            }
        }
        #endregion

        #region Get file info
        private bool GetFileInfo()
        {
            string fn = tbxFileName.Text.Trim();
            fileName = fn;
            long MB = (1024 * 1024);
            if (File.Exists(fn))
            {
                FileInfo info = new FileInfo(fn);
                Debug.WriteLine($"*** {fn} is {info.Length} bytes");
                long size = info.Length / MB;
                switch (size)
                {
                    case long n when (n > 500 && n <= 3500):
                        Debug.WriteLine($"{size} is above 100MB - less than 3500MB");
                        MessageBoxResult x = MessageBox.Show($"The file is {info.Length / MB} megabytes," +
                                                             $"\nThis could take a while." +
                                                             $"\nDo you want to continue? ",
                                                             "Large File Warning",
                                                             MessageBoxButton.YesNo,
                                                             MessageBoxImage.Question);
                        if (x == MessageBoxResult.No) return false;
                        return true;
                    case long n when (n > 3500):
                        Debug.WriteLine($"{size}MB is above 3500 MB");
                        MessageBox.Show($"The file is {info.Length / MB} megabytes," +
                                     $"\nFile is too large.",
                                     "Large File Warning",
                                     MessageBoxButton.OK,
                                     MessageBoxImage.Warning);
                        return false;
                    default:
                        Debug.WriteLine($"{size}MB is less than 500MB");
                        return true;
                }
            }
            else
            {
                ClearTextBoxes();
                lblVerify.Text = "File not found";
            }
            return false;
        }
        #endregion

        #region Compute Hashes

        private void CalculateHashes()
        {
            if (GetFileInfo())
            {
                Debug.WriteLine("||| Calc hashes");
                ClearTextBoxes();

                if ((bool)cbxMD5.IsChecked)
                {
                    tbxMD5.Text = MD5Checksum(fileName);
                }
                if ((bool)cbxSHA1.IsChecked)
                {
                    tbxSHA1.Text = SHA1Checksum(fileName);
                }
                if ((bool)cbxSHA256.IsChecked)
                {
                    tbxSHA256.Text = SHA256Checksum(fileName);
                }
                if ((bool)cbxSHA512.IsChecked)
                {
                    tbxSHA512.Text = SHA512Checksum(fileName);
                }
            }
        }

        private string MD5Checksum(string file)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (BufferedStream stream = new BufferedStream(File.OpenRead(file), bufferSize))
            {
                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    byte[] checksum = md5.ComputeHash(stream);
                    Debug.WriteLine($"+++ Finished MD5 {sw.Elapsed} elapsed");
                    return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
                }
            }
        }
        private string SHA1Checksum(string file)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (BufferedStream stream = new BufferedStream(File.OpenRead(file), bufferSize))
            {
                using (SHA1 sha1 = new SHA1CryptoServiceProvider())
                {
                    byte[] checksum = sha1.ComputeHash(stream);
                    sw.Stop();
                    Debug.WriteLine($"+++ Finished SHA1 {sw.Elapsed} elapsed");
                    return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
                }
            }
        }

        private string SHA256Checksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                using (SHA256 sha256 = new SHA256CryptoServiceProvider())
                {
                    byte[] checksum = sha256.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
                }
            }
        }

        private string SHA512Checksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                using (SHA512 sha512 = new SHA512CryptoServiceProvider())
                {
                    byte[] checksum = sha512.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
                }
            }
        }
        #endregion

        #region Compare Hashes
        private void CompareHashes()
        {
            string verifyText = tbxVerify.Text.Trim();

            switch (verifyText.Length)
            {
                case 32:
                    result = string.Equals(verifyText, tbxMD5.Text, StringComparison.OrdinalIgnoreCase).ToString();
                    break;
                case 40:
                    result = string.Equals(verifyText, tbxSHA1.Text, StringComparison.OrdinalIgnoreCase).ToString();
                    break;
                case 64:
                    result = string.Equals(verifyText, tbxSHA256.Text, StringComparison.OrdinalIgnoreCase).ToString();
                    break;
                case 128:
                    result = string.Equals(verifyText, tbxSHA512.Text, StringComparison.OrdinalIgnoreCase).ToString();
                    break;
                default:
                    result = "wrong length";
                    break;
            }

            if (result.ToLower() == "true")
            {
                lblVerify.Text = "File hashes match";
            }
            else if (result.ToLower() == "false")
            {
                lblVerify.Text = "File hashes do not match";
            }
            else if (result.ToLower() == "wrong length")
            {
                lblVerify.Text = "Verification hash is wrong length for MD5, SHA1, SHA256 or SHA512";
            }
            else
            {
                lblVerify.Text = "?";
            }
        }
        #endregion

        #region Events
        private void BtnFileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog
            {
                Title = "Enter Log File Name",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "All files (*.*)|*.*"
            };
            if (dlgOpen.ShowDialog() == true)
            {
                tbxFileName.Text = dlgOpen.FileName;
                ClearTextBoxes();
                CalculateHashes();
            }
        }

        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            CompareHashes();
        }

        private void BtnCalc_Click(object sender, RoutedEventArgs e)
        {
            CalculateHashes();
            Debug.WriteLine("||| button clicked");
        }

        private void TbxFileName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                CalculateHashes();
            }
        }
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Helper Methods
        private void ClearTextBoxes()
        {
            tbxMD5.Text = string.Empty;
            tbxSHA1.Text = string.Empty;
            tbxSHA256.Text = string.Empty;
            tbxSHA512.Text = string.Empty;
            tbxVerify.Text = string.Empty;
            lblVerify.Text = string.Empty;
        }
        #region Title version
        public string WindowTitleVersion()
        {
            // Get the assembly version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string myExe = Assembly.GetExecutingAssembly().GetName().Name;

            // Remove the release (last) node
            string titleVer = version.ToString().Remove(version.ToString().LastIndexOf("."));

            return string.Format($"{myExe} - {titleVer}");
        }
        #endregion Title version

        #endregion

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show("Yo");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PSettings.WindowLeft = Left;
            PSettings.WindowTop = Top;
            PSettings.WindowHeight = Height;
            PSettings.WindowWidth = Width;
            PSettings.Save();
        }

        #region Read the Settings file
        private void ReadSettings()
        {
            // Settings upgrade if needed
            if (PSettings.SettingsUpgradeRequired)
            {
                PSettings.Upgrade();
                PSettings.SettingsUpgradeRequired = false;
                PSettings.Save();
                Debug.WriteLine("*** SettingsUpgradeRequired");
            }

            // Window position
            Top = PSettings.WindowTop;
            Left = PSettings.WindowLeft;
            Width = PSettings.WindowWidth;
            Height = PSettings.WindowHeight;

            // Put version number in title bar
            Title = WindowTitleVersion();
        }

        #endregion

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            AddToExplorer add = new AddToExplorer();
            add.Owner = this;
            add.ShowDialog();
        }
    }
}
