using System;

namespace XTransmit.ViewModel.Control
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

        public bool CancellationPending
        {
            get => cancellationPending;
            set
            {
                cancellationPending = value;
                OnPropertyChanged(nameof(CancellationPending));
            }
        }

        public Action StopAction { get; set; } = null;

        private string name;
        private int progress100 = 0;
        private bool cancellationPending = false;

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
