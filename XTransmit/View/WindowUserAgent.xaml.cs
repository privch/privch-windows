using System.Windows;
using System.Windows.Controls;
using XTransmit.Model;
using XTransmit.Model.Setting;
using XTransmit.ViewModel;

namespace XTransmit.View
{
    public partial class WindowUserAgent : Window
    {
        public WindowUserAgent()
        {
            InitializeComponent();

            Preference preference = SettingManager.Appearance;
            Left = preference.WindowUserAgent.X;
            Top = preference.WindowUserAgent.Y;
            Width = preference.WindowUserAgent.W;
            Height = preference.WindowUserAgent.H;

            DataContext = new UserAgentVModel();
            Closing += WindowUserAgent_Closing;
        }

        private void WindowUserAgent_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            xDataGrid.CancelEdit(DataGridEditingUnit.Cell);
            xDataGrid.CancelEdit(DataGridEditingUnit.Row);
            ((UserAgentVModel)DataContext).OnWindowClosing(); // better way?

            // Save window placement
            Preference preference = SettingManager.Appearance;
            preference.WindowUserAgent.X = Left;
            preference.WindowUserAgent.Y = Top;
            preference.WindowUserAgent.W = Width;
            preference.WindowUserAgent.H = Height;
        }
    }
}
