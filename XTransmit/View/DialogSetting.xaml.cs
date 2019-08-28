using System.Windows;
using XTransmit.Model;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-06
     */
    public partial class DialogSetting : Window
    {
        public DialogSetting()
        {
            InitializeComponent();

            Preference preference = App.GlobalPreference;
            Left = preference.WindowSetting.X;
            Top = preference.WindowSetting.Y;

            DataContext = new SettingVModel();
            Closing += DialogSetting_Closing;
        }

        private void DialogSetting_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window placement
            Preference preference = App.GlobalPreference;
            preference.WindowSetting.X = Left;
            preference.WindowSetting.Y = Top;
        }
    }
}
