using System.Data;
using System.Windows;
using XTransmit.Model;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    /**
     * Updated: 2019-08-02
     */
    public partial class WindowIPAddress : Window
    {
        public WindowIPAddress()
        {
            InitializeComponent();

            Preference preference = App.GlobalPreference;
            Left = preference.WindowIPAddress.X;
            Top = preference.WindowIPAddress.Y;
            Width = preference.WindowIPAddress.W;
            Height = preference.WindowIPAddress.H;

            DataContext = new IPAddressVModel();
            Closing += WindowIPSet_Closing;
        }

        private void WindowIPSet_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((IPAddressVModel)DataContext).OnWindowClosing();

            // Save window placement
            Preference preference = App.GlobalPreference;
            preference.WindowIPAddress.X = Left;
            preference.WindowIPAddress.Y = Top;
            preference.WindowIPAddress.W = Width;
            preference.WindowIPAddress.H = Height;
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is DataTable table)
            {
                ((IPAddressVModel)DataContext).OnTableSelectionChanged(table.TableName);
            }
        }
    }
}
