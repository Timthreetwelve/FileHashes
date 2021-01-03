// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using TKUtils;
using MessageBoxImage = TKUtils.MessageBoxImage;
#endregion Using directives

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
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

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
        #endregion Process command line argument

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
        #endregion Get file info

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
        #endregion Compute Hashes

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
        #endregion Compare Hashes

        #region Window Events
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CommandLineArgs();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            windowHandle = new WindowInteropHelper(this).Handle;
            DisableMinMaxButtons();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            UserSettings.Setting.WindowLeft = Left;
            UserSettings.Setting.WindowTop = Top;
            UserSettings.Setting.WindowHeight = Height;
            UserSettings.Setting.WindowWidth = Width;

            UserSettings.SaveSettings();
        }
        #endregion Window Events

        #region Menu & Button Events
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

        private void GridSmaller_Click(object sender, RoutedEventArgs e)
        {
            GridSmaller();
        }
        private void GridLarger_Click(object sender, RoutedEventArgs e)
        {
            GridLarger();
        }
        private void GridReset_Click(object sender, RoutedEventArgs e)
        {
            GridSizeReset();
        }

        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            CompareHashes();
        }

        private void MnuCalc_Click(object sender, RoutedEventArgs e)
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
        #endregion Menu & Button Events

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

        #endregion Helper Methods

        #region Read the Settings file
        private void ReadSettings()
        {
            // Window position
            Top = UserSettings.Setting.WindowTop;
            Left = UserSettings.Setting.WindowLeft;
            Width = UserSettings.Setting.WindowWidth;
            Height = UserSettings.Setting.WindowHeight;
            WindowState = WindowState.Normal;

            // Set data grid zoom level
            double curZoom = UserSettings.Setting.GridZoom;
            Grid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);

            // Put version number in title bar
            Title = WindowTitleVersion();

            // Settings change event
            UserSettings.Setting.PropertyChanged += UserSettingChanged;
        }
        #endregion Read the Settings file

        #region Setting change
        private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
            var newValue = prop?.GetValue(sender, null);
            switch (e.PropertyName)
            {
                case "KeepOnTop":
                    Topmost = (bool)newValue;
                    break;
            }
            Debug.WriteLine($"***Setting change: {e.PropertyName} New Value: {newValue}");
        }
        #endregion Setting change

        #region Grid Size
        private void GridSmaller()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom > 0.9)
            {
                curZoom -= .05;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
            Grid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
        private void GridLarger()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom < 1.3)
            {
                curZoom += .05;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
            Grid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
        private void GridSizeReset()
        {
            UserSettings.Setting.GridZoom = 1.0;
            Grid1.LayoutTransform = new ScaleTransform(1, 1);
        }
        #endregion Grid Size

        #region Mouse Events
        private void Grid1_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
            {
                GridLarger();
            }
            else if (e.Delta < 0)
            {
                GridSmaller();
            }
        }
        #endregion Mouse Events

        #region Keyboard Events
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.NumPad0 && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                GridSizeReset();
            }

            if (e.Key == Key.Add && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                GridLarger();
            }

            if (e.Key == Key.Subtract && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                GridSmaller();
            }
            if (e.Key == Key.F1)
            {
                About about = new About
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _ = about.ShowDialog();
            }
        }
        #endregion Keyboard Events
    }
}
