using System.Windows;
using Privch.Model;
using Privch.Model.Setting;
using Privch.ViewModel;

namespace Privch.View
{
    public partial class DialogAbout : Window
    {
        public DialogAbout()
        {
            InitializeComponent();

            Preference preference = SettingManager.Appearance;
            Left = preference.WindowAbout.X;
            Top = preference.WindowAbout.Y;

            DataContext = new DialogAboutVModel();
            Closing += DialogAbout_Closing;
        }

        private void DialogAbout_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window placement
            Preference preference = SettingManager.Appearance;
            preference.WindowAbout.X = Left;
            preference.WindowAbout.Y = Top;
        }
    }
}
