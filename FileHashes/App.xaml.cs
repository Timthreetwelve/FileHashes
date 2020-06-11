// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.Windows;

namespace FileHashes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0) return;

            if (e.Args.Length > 0)
            {
                Args = e.Args;
            }
        }
    }
}
