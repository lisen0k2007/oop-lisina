using System;

namespace MyTasks
{
    public class TodoItem
    {
        private string _title = "";
        private bool _done;
        private DateTime? _dueDate;

        public TodoItem(string title, DateTime? dueDate)
        {
            Rename(title);
            _done = false;
            _dueDate = dueDate;
        }

        public string Title => _title;
        public bool IsDone => _done;
        public DateTime? DueDate => _dueDate;

        public void Complete() => _done = true;
        public void Uncomplete() => _done = false;

        public void Rename(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название не может быть пустым");
            _title = title.Trim();
        }

        public void SetDueDate(DateTime? date)
        {
            _dueDate = date;
        }
    }
}
