using System.Windows;
using XTransmit.Model;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class DialogSetting : Window
    {
        public DialogSetting()
        {
            InitializeComponent();

            Preference preference = PreferenceManager.Global;
            Left = preference.WindowSetting.X;
            Top = preference.WindowSetting.Y;

            DataContext = new SettingVModel();
            Closing += DialogSetting_Closing;
        }

        private void DialogSetting_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window placement
            Preference preference = PreferenceManager.Global;
            preference.WindowSetting.X = Left;
            preference.WindowSetting.Y = Top;
        }
    }
}
