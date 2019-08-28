using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using XTransmit.Model.Curl;
using XTransmit.Model.Network;
using XTransmit.View;

namespace XTransmit.ViewModel
{
    /**TODO - Optimize the save action
     * Updated: 2019-08-02
     */
    public class ContentCurlVModel : BaseViewModel
    {
        public ObservableCollection<SiteProfile> ObSiteList { get; private set; }

        public ContentCurlVModel()
        {
            // load IPAddress and UserAgent data
            IPAddressManager.Load(App.FileIPAddressXml);
            UserAgentManager.Load(App.FileUserAgentXml);

            // load curl data
            List<SiteProfile> siteList = SiteManager.LoadFileOrDefault(App.FileCurlXml);
            ObSiteList = new ObservableCollection<SiteProfile>(siteList);

            // set list display grouping
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(ObSiteList);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Website");
            collectionView.GroupDescriptions.Add(groupDescription);
        }

        /** Actions ===============================================================================
         */
        private void OnSaveProfile(SiteProfile profile)
        {
            SiteProfile profileNew = profile.Copy();
            SiteProfile profileOld = ObSiteList.FirstOrDefault(item => item.Title == profileNew.Title && item.Website == profileNew.Website);

            if (profileOld != null)
            {
                int index = ObSiteList.IndexOf(profileOld);
                ObSiteList[index] = profileNew;
            }
            else
            {
                ObSiteList.Add(profileNew);
            }

            // convert to list and save
            List<SiteProfile> siteProfiles = new List<SiteProfile>(ObSiteList);
            SiteManager.WriteFile(App.FileCurlXml, siteProfiles);
        }

        /** Commands ==============================================================================
         */
        private bool isSelectProfile(object selected) => (selected is SiteProfile);

        // new profile
        public RelayCommand CommandNewProfile => new RelayCommand(newProfile);
        private void newProfile(object parameter)
        {
            new WindowCurlPlay(SiteProfile.Default(), OnSaveProfile).Show();
        }

        // delete profile
        public RelayCommand CommandDeleteProfile => new RelayCommand(deleteProfile, isSelectProfile);
        private void deleteProfile(object selected)
        {
            if (selected is SiteProfile profileDelete)
            {
                SiteProfile profile = ObSiteList.FirstOrDefault(item => item.Title == profileDelete.Title && item.Website == profileDelete.Website);
                if (profile != null)
                {
                    ObSiteList.Remove(profile);

                    // convert to list and save
                    List<SiteProfile> siteProfiles = new List<SiteProfile>(ObSiteList);
                    SiteManager.WriteFile(App.FileCurlXml, siteProfiles);
                }
            }
        }

        // launch
        public RelayCommand CommandLaunchProfile => new RelayCommand(launchProfile, isSelectProfile);
        private void launchProfile(object selected)
        {
            if (selected is SiteProfile profile)
            {
                new WindowCurlPlay(profile.Copy(), OnSaveProfile).Show();
            }
        }

        public RelayCommand CommandViewIPAddress => new RelayCommand(viewIPAddress);
        private void viewIPAddress(object obj)
        {
            new WindowIPAddress().Show();
        }

        public RelayCommand CommandViewUserAgent => new RelayCommand(viewUserAgent);
        private void viewUserAgent(object obj)
        {
            new WindowUserAgent().Show();
        }
    }
}
