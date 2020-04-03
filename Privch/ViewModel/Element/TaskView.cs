using System;

namespace Privch.ViewModel.Element
{
    public class TaskView : BaseViewModel
    {
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int Progress100
        {
            get => progress100;
            set
            {
                progress100 = value;
                OnPropertyChanged(nameof(Progress100));
            }
        }

        public Action StopAction { get; set; } = null;

        private string name = null;
        private int progress100 = 0;

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object newTask)
        {
            if (newTask is TaskView task)
            {
                return Name == task.Name;
            }

            return false;
        }
    }
}
