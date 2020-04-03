﻿using System;
using System.Collections.Generic;
using Privch.ViewModel.Element;

namespace Privch.ViewModel
{
    class DialogActionVModel : BaseViewModel
    {
        public string Title { get; }
        public string Message { get; }
        public Dictionary<string, Action> ActionList { get; }

        private readonly View.DialogAction Dialog;

        public DialogActionVModel(View.DialogAction dialog, string title, string message, Dictionary<string, Action> actions)
        {
            Dialog = dialog;

            Title = title;
            Message = message;
            ActionList = actions;
        }

        /** Commands =========================================================================================================
         */
        public RelayCommand CommandAction => new RelayCommand(ExecuteAction);
        private void ExecuteAction(object parameter)
        {
            if (parameter is string key)
            {
                ActionList[key]?.Invoke();
            }

            Dialog.Close();
        }
    }
}
