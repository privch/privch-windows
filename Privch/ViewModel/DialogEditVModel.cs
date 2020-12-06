using System;
using PrivCh.ViewModel.Element;

namespace PrivCh.ViewModel
{
    class DialogEditVModel : BaseViewModel
    {
        public string Title { get; }
        public string Message { get; }

        private readonly Action actionComplete;

        public DialogEditVModel(string title, string message, Action action)
        {
            Title = title;
            Message = message;

            actionComplete = action;
        }

        /** Commands ================================================================================
         */
        public RelayCommand CommandCloseOK => new RelayCommand(CloseOK);
        private void CloseOK(object parameter)
        {
            if (parameter is View.DialogEdit dialog)
            {
                actionComplete?.Invoke();
                dialog.Close();
            }
        }

        public RelayCommand CommandCloseCancel => new RelayCommand(CloseCancel);
        private void CloseCancel(object parameter)
        {
            if (parameter is View.DialogEdit dialog)
            {
                actionComplete?.Invoke();
                dialog.Close();
            }
        }
    }
}
