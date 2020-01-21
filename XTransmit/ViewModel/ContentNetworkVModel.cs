using LiveCharts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media;
using XTransmit.Model.Network;
using XTransmit.Utility;

namespace XTransmit.ViewModel
{
    class ContentNetworkVModel : BaseViewModel, IDisposable
    {
        private bool isActivated = false;
        public bool IsActivated 
        { 
            get => isActivated;
            private set
            {
                isActivated = value;
                if (isActivated)
                {
                    adapterSpeedMeter.Start();
                }
                else
                {
                    adapterSpeedMeter.Stop();
                }
            }
        }

        // interface descriptions
        public List<string> NetworkInterfaceAll { get; }

        [SuppressMessage("Globalization", "CA1822", Justification = "<Pending>")]
        public string NetworkInterfaceSelected
        {
            get => App.GlobalConfig.NetworkAdapter;
            set
            {
                App.GlobalConfig.NetworkAdapter = value;
            }
        }

        public SeriesCollection ChartSeries { get; }
        public Func<double, string> ChartXFormatter { get; }
        public Func<double, string> ChartYFormatter { get; }

        private readonly List<NetworkInterface> adapterList; // network interfaces
        private readonly BandwidthMeter adapterSpeedMeter;

        // languages
        private static readonly string sr_download = (string)Application.Current.FindResource("_download");
        private static readonly string sr_upload = (string)Application.Current.FindResource("_upload");

        [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        public ContentNetworkVModel()
        {
            // init live chart
            var dayConfig = LiveCharts.Configurations.Mappers.Xy<BandwidthInfo>()
                .X(dayModel => (double)dayModel.Time.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(dayModel => dayModel.Value);

            ChartSeries = new SeriesCollection(dayConfig)
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Title = sr_download,
                    LineSmoothness = 1,
                    PointGeometry = null,
                    Stroke = Application.Current.Resources[key:"PrimaryHueMidBrush"] as Brush,
                    Values = new ChartValues<BandwidthInfo>(),
                },
                new LiveCharts.Wpf.LineSeries
                {
                    Title = sr_upload,
                    LineSmoothness = 1,
                    PointGeometry = null,
                    Stroke = Application.Current.Resources[key:"SecondaryAccentBrush"] as Brush,
                    Values = new ChartValues<BandwidthInfo>(),
                }
            };

            ChartXFormatter = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString("HH:mm:ss");
            ChartYFormatter = value => $"{TextUtil.GetBytesReadable((long)value)}/s";

            // get network interfaces
            adapterList = NetworkUtil.GetValidNetworkInterface();
            adapterList.Sort(comparison: NetworkUtil.CompareNetworkInterfaceBySpeed);
            adapterList.Reverse();

            NetworkInterfaceAll = new List<string>();
            foreach (NetworkInterface adapter in adapterList)
            {
                NetworkInterfaceAll.Add(adapter.Description);
            }

            // init speed meter
            adapterSpeedMeter = new BandwidthMeter(AdapterSpeedMeter_UpdateSpeed);

            if (NetworkInterfaceAll.Count > 0)
            {
                if (NetworkInterfaceSelected == null ||
                    NetworkInterfaceAll.FirstOrDefault(item => item == NetworkInterfaceSelected) == null)
                {
                    NetworkInterfaceSelected = adapterList[0].Description;
                }

                // also start the meter
                IsActivated = true;
            }
        }

        ~ContentNetworkVModel()
        {
            adapterSpeedMeter.Stop();
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                adapterSpeedMeter.Dispose();
            }
        }

        private void AdapterSpeedMeter_UpdateSpeed(long[] values)
        {
            DateTime now = DateTime.Now;
            ChartSeries[0].Values.Add(new BandwidthInfo(now, values[0]));
            ChartSeries[1].Values.Add(new BandwidthInfo(now, values[1]));

            // Remove data older than 30 seconds
            if (ChartSeries[0].Values.Count > 30)
            {
                ChartSeries[0].Values.RemoveAt(0);
            }

            if (ChartSeries[1].Values.Count > 30)
            {
                ChartSeries[1].Values.RemoveAt(0);
            }
        }


        // Change network interfaces. 
        // NOTE - Move it into NetworkInterfaceSelected property set method?
        public void UpdateNetworkInterface()
        {
            if (NetworkInterfaceSelected != null)
            {
                NetworkInterface adapterSelected = adapterList.FirstOrDefault(x => x.Description == NetworkInterfaceSelected);
                adapterSpeedMeter.SetAdapter(adapterSelected);
            }
        }


        /** Commands ==================================================================
         */
        public RelayCommand CommandToggleActivate => new RelayCommand(ToggleActivate);
        private void ToggleActivate(object parameter)
        {
            IsActivated = !IsActivated;
            OnPropertyChanged(nameof(IsActivated));
        }
    }
}
