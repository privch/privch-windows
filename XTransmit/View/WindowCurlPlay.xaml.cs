using System;
using System.Windows;
using XTransmit.Model;
using XTransmit.Model.Curl;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class WindowCurlPlay : Window
    {
        public WindowCurlPlay(SiteProfile siteProfile, Action<SiteProfile> actionSaveProfile)
        {
            InitializeComponent();

            Preference preference = App.GlobalPreference;
            Left = preference.WindowCurlRunner.X;
            Top = preference.WindowCurlRunner.Y;
            Width = preference.WindowCurlRunner.W;
            Height = preference.WindowCurlRunner.H;

            DataContext = new CurlPlayVModel(siteProfile, actionSaveProfile);
            Closing += WindowCurl_Closing;
        }

        private void WindowCurl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window placement
            Preference preference = App.GlobalPreference;
            preference.WindowCurlRunner.X = Left;
            preference.WindowCurlRunner.Y = Top;
            preference.WindowCurlRunner.W = Width;
            preference.WindowCurlRunner.H = Height;
        }
    }
}
