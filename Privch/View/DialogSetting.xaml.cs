using System.Windows;
using Privch.Model;
using Privch.Model.Setting;
using Privch.ViewModel;

namespace Privch.View
{
    public partial class DialogSetting : Window
    {
        public DialogSetting()
        {
            InitializeComponent();

            Preference preference = SettingManager.Appearance;
            Left = preference.WindowSetting.X;
            Top = preference.WindowSetting.Y;

            DataContext = new SettingVModel();
            Closing += DialogSetting_Closing;
        }

        private void DialogSetting_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window placement
            Preference preference = SettingManager.Appearance;
            preference.WindowSetting.X = Left;
            preference.WindowSetting.Y = Top;
        }
    }
}
