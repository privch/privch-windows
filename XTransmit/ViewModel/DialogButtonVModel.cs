namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-08-06
     */
    class DialogButtonVModel : BaseViewModel
    {
        public string Title { get; private set; }
        public string Message { get; private set; }

        public DialogButtonVModel(string title, string message)
        {
            Title = title;
            Message = message;
        }

        /** Commands =========================================================================================================
         */
        public RelayCommand CommandCloseYes => new RelayCommand(CloseYes);
        private void CloseYes(object parameter)
        {
            if (parameter is View.DialogButton dialog)
            {
                dialog.CancelableResult = true;
                dialog.Close();
            }
        }

        public RelayCommand CommandCloseNo => new RelayCommand(CloseNo);
        private void CloseNo(object parameter)
        {
            if (parameter is View.DialogButton dialog)
            {
                dialog.CancelableResult = false;
                dialog.Close();
            }
        }
    }
}
