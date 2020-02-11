using System.Windows;
using System.Windows.Controls;
using XTransmit.Model;
using XTransmit.Model.Setting;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class WindowIPAddress : Window
    {
        public WindowIPAddress()
        {
            InitializeComponent();

            Preference preference = SettingManager.Appearance;
            Left = preference.WindowIPAddress.X;
            Top = preference.WindowIPAddress.Y;
            Width = preference.WindowIPAddress.W;
            Height = preference.WindowIPAddress.H;

            DataContext = new IPAddressVModel();
            Closing += WindowIPAddress_Closing;
        }

        private void WindowIPAddress_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            xDataGrid.CancelEdit(DataGridEditingUnit.Cell);
            xDataGrid.CancelEdit(DataGridEditingUnit.Row);
            ((IPAddressVModel)DataContext).OnWindowClosing();

            // Save window placement
            Preference preference = SettingManager.Appearance;
            preference.WindowIPAddress.X = Left;
            preference.WindowIPAddress.Y = Top;
            preference.WindowIPAddress.W = Width;
            preference.WindowIPAddress.H = Height;
        }

        private void ProgressBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((IPAddressVModel)DataContext).StopPing();
        }
    }
}
