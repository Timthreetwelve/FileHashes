﻿// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TKUtils;
using MessageBoxImage = TKUtils.MessageBoxImage;

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
        #endregion

        #region Window button voodoo
        // https://stackoverflow.com/questions/18707782/disable-maximize-button-of-wpf-window-keeping-resizing-feature-intact
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;                        // maximize button
        private const int WS_MINIMIZEBOX = 0x20000;                        // minimize button
        private const int WS_BOTHBOXES = WS_MINIMIZEBOX + WS_MAXIMIZEBOX;  // Both
        private IntPtr windowHandle;
        #endregion

        public MainWindow()
        {
            UserSettings.Init(UserSettings.AppFolder,UserSettings.DefaultFilename,true);

            InitializeComponent();

            ReadSettings();
        }

        #region Process command line argument
        private void CommandLineArgs()
        {
            string[] args = App.Args;

            if (args != null)
            {
                tbxFileName.Text = args[0].Trim();
                CalculateHashes();
            }
        }
        #endregion

        #region Get file info
        private bool GetFileInfo()
        {
            string fn = tbxFileName.Text.Trim();
            fileName = fn;
            const long MB = (1024 * 1024);
            if (File.Exists(fn))
            {
                FileInfo info = new FileInfo(fn);
                Debug.WriteLine($"*** {fn} is {info.Length} bytes");
                long size = info.Length / MB;
                switch (size)
                {
                    case long n when (n > 500 && n <= 5000):
                        Debug.WriteLine($"*** {size} is above 100MB - less than 5000MB");
                        MessageBoxResult x = TKMessageBox.Show($"The file is {info.Length / MB:N0} megabytes," +
                                                             "\nThis could take a while.\nDo you want to continue? ",
                                                             "Large File Warning",
                                                             MessageBoxButton.YesNo,
                                                             MessageBoxImage.Question);
                        if (x == MessageBoxResult.No)
                        {
                            ClearTextBoxes();
                            return false;
                        }
                        return true;
                    case long n when (n > 5000):
                        Debug.WriteLine($"||| {size}MB is above 5000 MB");
                        TKMessageBox.Show($"The file is {info.Length / MB:N0} megabytes\nFile is too large.",
                                     "Large File Warning",
                                     MessageBoxButton.OK,
                                     MessageBoxImage.Warning);
                        ClearTextBoxes();
                        return false;
                    default:
                        Debug.WriteLine($"*** {size}MB is less than 500MB");
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
                ClearTextBoxes();
                Mouse.OverrideCursor = Cursors.Wait;

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

                Mouse.OverrideCursor = null;
            }
        }

        private string MD5Checksum(string file)
        {
            using (BufferedStream stream = new BufferedStream(File.OpenRead(file), bufferSize))
            {
                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    byte[] checksum = md5.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
                }
            }
        }
        private string SHA1Checksum(string file)
        {
            using (BufferedStream stream = new BufferedStream(File.OpenRead(file), bufferSize))
            {
                using (SHA1 sha1 = new SHA1CryptoServiceProvider())
                {
                    byte[] checksum = sha1.ComputeHash(stream);
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

            if (string.Equals(result, "true", StringComparison.OrdinalIgnoreCase))
            {
                lblVerify.Text = "File hashes match";
            }
            else if (string.Equals(result, "false", StringComparison.OrdinalIgnoreCase))
            {
                lblVerify.Text = "File hashes do not match";
            }
            else if (string.Equals(result, "wrong length", StringComparison.OrdinalIgnoreCase))
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
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CommandLineArgs();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            windowHandle = new WindowInteropHelper(this).Handle;
            DisableMinMaxButtons();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.Setting.WindowLeft = Left;
            UserSettings.Setting.WindowTop = Top;
            UserSettings.Setting.WindowHeight = Height;
            UserSettings.Setting.WindowWidth = Width;

            UserSettings.SaveSettings();
        }

        private void BtnFileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog
            {
                Title = "Enter File Name",
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
        }

        private void TbxFileName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                CalculateHashes();
            }
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            AddToExplorer add = new AddToExplorer
            {
                Owner = this
            };
            add.ShowDialog();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            _ = about.ShowDialog();
        }

        private void MnuReadMe_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(@".\ReadMe.txt");
        }
        #endregion

        #region Helper Methods
        protected void DisableMinMaxButtons()
        {
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            SetWindowLong(windowHandle, GWL_STYLE, GetWindowLong(windowHandle, GWL_STYLE) & ~WS_BOTHBOXES);
        }

        private void ClearTextBoxes()
        {
            tbxMD5.Text = string.Empty;
            tbxSHA1.Text = string.Empty;
            tbxSHA256.Text = string.Empty;
            tbxSHA512.Text = string.Empty;
            tbxVerify.Text = string.Empty;
            lblVerify.Text = string.Empty;
        }

        public string WindowTitleVersion()
        {
            // Get the assembly version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string myExe = Assembly.GetExecutingAssembly().GetName().Name;

            // Remove the release (last) node
            string titleVer = version.ToString().Remove(version.ToString().LastIndexOf("."));

            return string.Format($"{myExe} - {titleVer}");
        }

        #endregion

        #region Read the Settings file
        private void ReadSettings()
        {
            // Window position
            Top = UserSettings.Setting.WindowTop;
            Left = UserSettings.Setting.WindowLeft;
            Width = UserSettings.Setting.WindowWidth;
            Height = UserSettings.Setting.WindowHeight;
            WindowState = WindowState.Normal;

            // Put version number in title bar
            Title = WindowTitleVersion();
        }
        #endregion
    }
}
