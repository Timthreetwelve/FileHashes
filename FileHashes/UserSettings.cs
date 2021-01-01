// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKUtils;
#endregion Using directives

namespace FileHashes
{
    public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
    {
        #region Constructor
        public UserSettings()
        {
            // Set defaults
            CheckMD5 = false;
            CheckSHA1 = false;
            CheckSHA256 = true;
            CheckSHA512 = false;
            GridZoom = 1;
            KeepOnTop = false;
            WindowLeft = 100;
            WindowTop = 100;
            WindowWidth = 650;
            WindowHeight = 350;
        }
        #endregion Constructor

        #region Properties
        public bool CheckMD5 { get; set; }

        public bool CheckSHA1 { get; set; }

        public bool CheckSHA256 { get; set; }

        public bool CheckSHA512 { get; set; }

        public double GridZoom
        {
            get
            {
                if (gridZoom <= 0)
                {
                    gridZoom = 1;
                }
                return gridZoom;
            }
            set
            {
                gridZoom = value;
                OnPropertyChanged();
            }
        }

        public bool KeepOnTop
        {
            get => keepOnTop;
            set
            {
                keepOnTop = value;
                OnPropertyChanged();
            }
        }

        public double WindowHeight
        {
            get
            {
                if (windowHeight < 100)
                {
                    windowHeight = 100;
                }
                return windowHeight;
            }
            set => windowHeight = value;
        }

        public double WindowLeft
        {
            get
            {
                if (windowLeft < 0)
                {
                    windowLeft = 100;
                }
                return windowLeft;
            }
            set => windowLeft = value;
        }

        public double WindowTop
        {
            get
            {
                if (windowTop < 0)
                {
                    windowTop = 100;
                }
                return windowTop;
            }
            set => windowTop = value;
        }

        public double WindowWidth
        {
            get
            {
                if (windowWidth < 100)
                {
                    windowWidth = 100;
                }
                return windowWidth;
            }
            set => windowWidth = value;
        }
        #endregion Properties

        #region Private backing fields
        private bool keepOnTop;
        private double gridZoom;
        private double windowHeight;
        private double windowLeft;
        private double windowTop;
        private double windowWidth;
        #endregion Private backing fields

        #region Handle property change event
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Handle property change event
    }
}
