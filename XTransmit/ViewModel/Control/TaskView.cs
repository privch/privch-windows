using System;

namespace XTransmit.ViewModel.Control
{
    public class TaskView : BaseViewModel
    {
        public string Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
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

        private string id;
        private int progress100 = 0;
        private bool cancellationPending = false;

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object newTask)
        {
            if (newTask is TaskView task)
            {
                return Id == task.Id;
            }

            return false;
        }
    }
}
