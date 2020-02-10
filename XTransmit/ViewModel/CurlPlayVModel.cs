using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using XTransmit.Model.Curl;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    internal class CurlPlayVModel : BaseViewModel, IDisposable
    {
        public SiteProfile Profile { get; private set; }

        public string WindowTitle => $"{Profile.Website} {Profile.Title}";
        public double WindowProgress { get; private set; }

        public bool IsNotRunning { get; private set; }
        public bool IsRandomDelay { get; private set; }

        public string DelaySetting { get; set; } // textbox delay 
        public ObservableCollection<CurlResponse> ResponseList { get; private set; }

        private readonly SiteWorker siteWorker;
        private readonly Action<SiteProfile> actionSaveProfile; // callback action 

        public CurlPlayVModel(SiteProfile siteProfile, Action<SiteProfile> actionSaveProfile)
        {
            Profile = siteProfile;
            WindowProgress = 0;

            IsNotRunning = true;
            IsRandomDelay = Profile.DelayMax > Profile.DelayMin;

            DelaySetting = IsRandomDelay ? $"{Profile.DelayMin} - {Profile.DelayMax}"
                : Profile.DelayMin.ToString(CultureInfo.InvariantCulture);
            ResponseList = new ObservableCollection<CurlResponse>();

            siteWorker = new SiteWorker(ActionStateUpdated, ActionSiteResponse);
            this.actionSaveProfile = actionSaveProfile;
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
                siteWorker.Dispose();
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private bool ParseDelay(string delay)
        {
            int delay_minimum;
            int delay_maximum = 0;

            // parse loop setting
            string[] delay_splited = delay.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (delay_splited.Length < 2)
            {
                try
                {
                    // no random delay
                    delay_minimum = int.Parse(delay, CultureInfo.InvariantCulture);
                }
                catch { return false; }
            }
            else
            {
                try
                {
                    // random delay
                    delay_minimum = int.Parse(delay_splited[0], CultureInfo.InvariantCulture);
                    delay_maximum = int.Parse(delay_splited[1], CultureInfo.InvariantCulture);
                }
                catch { return false; }
            }

            Profile.DelayMin = delay_minimum;
            Profile.DelayMax = delay_maximum;
            return true;
        }

        /** Actions ===================================================================================
         */
        private void ActionStateUpdated(bool isRunning)
        {
            IsNotRunning = !isRunning;
            if (IsNotRunning)
            {
                WindowProgress = 0;
            }

            OnPropertyChanged(nameof(IsNotRunning));
            OnPropertyChanged(nameof(WindowProgress));
        }

        private void ActionSiteResponse(CurlResponse curlResponse)
        {
            WindowProgress = (double)curlResponse.Index / Profile.PlayTimes;
            ResponseList.Insert(0, curlResponse);

            OnPropertyChanged(nameof(WindowProgress));
        }

        /** Commands ==================================================================================
         */
        public RelayCommand CommandSetDalay => new RelayCommand(UpdateDelay);

        private void UpdateDelay(object parameter)
        {
            ParseDelay(DelaySetting);

            IsRandomDelay = Profile.DelayMax > Profile.DelayMin;
            DelaySetting = IsRandomDelay ? $"{Profile.DelayMin} - {Profile.DelayMax}"
                : Profile.DelayMin.ToString(CultureInfo.InvariantCulture);

            OnPropertyChanged(nameof(DelaySetting));
            OnPropertyChanged(nameof(IsRandomDelay));
        }

        public RelayCommand CommandSaveProfile => new RelayCommand(SaveProfile, CanSaveProfile);
        private bool CanSaveProfile(object parameter) => actionSaveProfile != null;
        private void SaveProfile(object parameter)
        {
            actionSaveProfile.Invoke(Profile);
        }

        public RelayCommand CommandTogglePlay => new RelayCommand(TogglePlayAsync);
        private void TogglePlayAsync(object parameter)
        {
            IsNotRunning = !IsNotRunning;
            if (IsNotRunning)
            {
                WindowProgress = 0;
                siteWorker.StopBgWork();
            }
            else
            {
                siteWorker.StartBgWork(Profile);
            }

            OnPropertyChanged(nameof(IsNotRunning));
            OnPropertyChanged(nameof(WindowProgress));
        }

        // response ------------------------------------------------------------
        private bool IsValidResponse(object parameter) => ResponseList.Count > 0;

        public RelayCommand CommandClearResponse => new RelayCommand(ClearResponse, IsValidResponse);
        private void ClearResponse(object parameter)
        {
            ResponseList.Clear();
        }
    }
}
