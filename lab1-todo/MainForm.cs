using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyTasks
{
    public partial class MainForm : Form
    {
        private TodoList _todoList = new TodoList();

        public MainForm()
        {
            InitializeComponent();

            cmbFilter.Items.AddRange(new object[] { "Все", "Активные", "Выполненные" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => RefreshList();

            txtSearch.TextChanged += (s, e) => RefreshList();

            _todoList.Load();
            RefreshList();
        }

        private void RefreshList()
        {
            lstTasks.Items.Clear();

            bool showActive = cmbFilter.SelectedIndex == 0 || cmbFilter.SelectedIndex == 1;
            bool showCompleted = cmbFilter.SelectedIndex == 0 || cmbFilter.SelectedIndex == 2;

            var items = _todoList.GetFiltered(showCompleted, showActive, txtSearch.Text);

            foreach (var item in items)
            {
                lstTasks.Items.Add(item);
            }

            int total = _todoList.GetAll().Count;
            int done = 0;
            foreach (var i in _todoList.GetAll()) if (i.IsDone) done++;
            lblStatus.Text = $"{total} задач, выполнено {done}";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                _todoList.Add(txtTitle.Text, dtpDueDate.Value);
                txtTitle.Clear();
                RefreshList();
                _todoList.Save();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnComplete_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem is TodoItem item)
            {
                item.Complete();
                RefreshList();
                _todoList.Save();
            }
            else
            {
                MessageBox.Show("Выберите задачу.", "Внимание");
            }
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem is TodoItem item)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Новое название:", "Переименовать", item.Title);
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    try
                    {
                        item.Rename(newName);
                        RefreshList();
                        _todoList.Save();
                    }
                    catch (ArgumentException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem is TodoItem item)
            {
                _todoList.Remove(item);
                RefreshList();
                _todoList.Save();
            }
            else
            {
                MessageBox.Show("Выберите задачу.");
            }
        }

        private void lstTasks_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            TodoItem item = lstTasks.Items[e.Index] as TodoItem;
            if (item == null) return;

            e.DrawBackground();

            Brush textBrush = Brushes.Black;
            if (!item.IsDone && item.DueDate.HasValue && item.DueDate.Value.Date < DateTime.Now.Date)
            {
                textBrush = Brushes.Red;
            }
            else if (item.IsDone)
            {
                textBrush = Brushes.Gray;
            }

            string displayText = item.Title;
            if (item.DueDate.HasValue)
                displayText += $" (до {item.DueDate.Value:dd.MM.yyyy})";

            e.Graphics.DrawString(displayText, e.Font, textBrush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _todoList.Save();
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
