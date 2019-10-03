using System;
using System.Collections.ObjectModel;
using XTransmit.Model.Curl;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-08-04
     */
    public class CurlPlayVModel : BaseViewModel
    {
        public SiteProfile Profile { get; private set; }
        public ProgressInfo Progress { get; private set; }

        public string WindowTitle { get { return $"{Profile.Website} {Profile.Title}"; } }
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
            Progress = new ProgressInfo(0, false);
            WindowProgress = 0;

            IsNotRunning = true;
            IsRandomDelay = Profile.DelayMax > Profile.DelayMin;

            DelaySetting = IsRandomDelay ? $"{Profile.DelayMin} - {Profile.DelayMax}" : Profile.DelayMin.ToString();
            ResponseList = new ObservableCollection<CurlResponse>();

            siteWorker = new SiteWorker(OnStateUpdated, OnSiteResponse);
            this.actionSaveProfile = actionSaveProfile;
        }

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
                    delay_minimum = int.Parse(delay);
                }
                catch { return false; }
            }
            else
            {
                try
                {
                    // random delay
                    delay_minimum = int.Parse(delay_splited[0]);
                    delay_maximum = int.Parse(delay_splited[1]);
                }
                catch { return false; }
            }

            Profile.DelayMin = delay_minimum;
            Profile.DelayMax = delay_maximum;
            return true;
        }

        /** Actions ===================================================================================
         */
        private void OnStateUpdated(bool isRunning)
        {
            IsNotRunning = !isRunning;
            if (IsNotRunning)
            {
                Progress.Set(0, false);
                WindowProgress = 0;
            }
            else
            {
                Progress.Set(70, true);
            }

            OnPropertyChanged("IsNotRunning");
            OnPropertyChanged("Progress");
            OnPropertyChanged("WindowProgress");
        }

        private void OnSiteResponse(CurlResponse curlResponse)
        {
            WindowProgress = (double)curlResponse.Index / Profile.PlayTimes;
            ResponseList.Insert(0, curlResponse);

            OnPropertyChanged("WindowProgress");
        }

        /** Commands ==================================================================================
         */
        public RelayCommand CommandSetDalay => new RelayCommand(UpdateDelay);
        private void UpdateDelay(object parameter)
        {
            ParseDelay(DelaySetting);

            IsRandomDelay = Profile.DelayMax > Profile.DelayMin;
            DelaySetting = IsRandomDelay ? $"{Profile.DelayMin} - {Profile.DelayMax}" : Profile.DelayMin.ToString();

            OnPropertyChanged("DelaySetting");
            OnPropertyChanged("IsRandomDelay");
        }

        public RelayCommand CommandSaveProfile => new RelayCommand(SaveProfile, CanSaveProfile);
        private bool CanSaveProfile(object parameter) => actionSaveProfile != null;
        private void SaveProfile(object parameter)
        {
            actionSaveProfile(Profile);
        }

        public RelayCommand CommandTogglePlay => new RelayCommand(TogglePlayAsync);
        private void TogglePlayAsync(object parameter)
        {
            IsNotRunning = !IsNotRunning;
            if (IsNotRunning)
            {
                Progress.Set(0, false);
                WindowProgress = 0;
                siteWorker.StopBgWork();
            }
            else
            {
                Progress.Set(70, true);
                siteWorker.StartBgWork(Profile);
            }

            OnPropertyChanged("IsNotRunning");
            OnPropertyChanged("Progress");
            OnPropertyChanged("WindowProgress");
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
