namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-08-06
     */
    class DialogEditVModel : BaseViewModel
    {
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string Text { get; set; }

        public DialogEditVModel(string title, string message)
        {
            Title = title;
            Message = message;
        }

        /** Commands =========================================================================================================
         */
        public RelayCommand CommandCloseOK => new RelayCommand(CloseOK);
        private void CloseOK(object parameter)
        {
            if (parameter is View.DialogEdit dialog)
            {
                dialog.EditText = Text;
                dialog.Close();
            }
        }

        public RelayCommand CommandCloseCancel => new RelayCommand(CloseCancel);
        private void CloseCancel(object parameter)
        {
            if (parameter is View.DialogEdit dialog)
            {
                dialog.EditText = null;
                dialog.Close();
            }
        }
    }
}
