using System.Windows;
using XTransmit.Model;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-02
     */
    public partial class DialogAbout : Window
    {
        public DialogAbout()
        {
            InitializeComponent();

            Preference preference = App.GlobalPreference;
            Left = preference.WindowAbout.X;
            Top = preference.WindowAbout.Y;

            DataContext = new AboutVModel();
            Closing += DialogAbout_Closing;
        }

        private void DialogAbout_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window placement
            Preference preference = App.GlobalPreference;
            preference.WindowAbout.X = Left;
            preference.WindowAbout.Y = Top;
        }
    }
}
