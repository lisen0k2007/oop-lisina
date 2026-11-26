namespace MyTasks
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            txtTitle = new TextBox();
            dtpDueDate = new DateTimePicker();
            btnAdd = new Button();
            txtSearch = new TextBox();
            cmbFilter = new ComboBox();
            lstTasks = new ListBox();
            btnComplete = new Button();
            btnRename = new Button();
            btnDelete = new Button();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(34, 34);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(349, 39);
            txtTitle.TabIndex = 0;
            // 
            // dtpDueDate
            // 
            dtpDueDate.Location = new Point(460, 34);
            dtpDueDate.Name = "dtpDueDate";
            dtpDueDate.Size = new Size(400, 39);
            dtpDueDate.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(973, 34);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(150, 46);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "добавить";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(34, 107);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(349, 39);
            txtSearch.TabIndex = 3;
            // 
            // cmbFilter
            // 
            cmbFilter.FormattingEnabled = true;
            cmbFilter.Location = new Point(460, 107);
            cmbFilter.Name = "cmbFilter";
            cmbFilter.Size = new Size(285, 40);
            cmbFilter.TabIndex = 4;
            // 
            // lstTasks
            // 
            lstTasks.DrawMode = DrawMode.OwnerDrawFixed;
            lstTasks.FormattingEnabled = true;
            lstTasks.Location = new Point(34, 216);
            lstTasks.Name = "lstTasks";
            lstTasks.Size = new Size(808, 324);
            lstTasks.TabIndex = 5;
            lstTasks.DrawItem += lstTasks_DrawItem;
            // 
            // btnComplete
            // 
            btnComplete.Location = new Point(34, 562);
            btnComplete.Name = "btnComplete";
            btnComplete.Size = new Size(150, 46);
            btnComplete.TabIndex = 6;
            btnComplete.Text = "Выполнено";
            btnComplete.UseVisualStyleBackColor = true;
            btnComplete.Click += btnComplete_Click;
            // 
            // btnRename
            // 
            btnRename.Location = new Point(423, 562);
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(229, 46);
            btnRename.TabIndex = 7;
            btnRename.Text = "Переименовать";
            btnRename.UseVisualStyleBackColor = true;
            btnRename.Click += btnRename_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(222, 562);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(150, 46);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "Удалить";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(34, 640);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(78, 32);
            lblStatus.TabIndex = 9;
            lblStatus.Text = "label1";
            lblStatus.Click += lblStatus_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1236, 716);
            Controls.Add(lblStatus);
            Controls.Add(btnDelete);
            Controls.Add(btnRename);
            Controls.Add(btnComplete);
            Controls.Add(lstTasks);
            Controls.Add(cmbFilter);
            Controls.Add(txtSearch);
            Controls.Add(btnAdd);
            Controls.Add(dtpDueDate);
            Controls.Add(txtTitle);
            Name = "MainForm";
            Text = "Мои задачи";
            FormClosing += Form1_FormClosing;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtTitle;
        private DateTimePicker dtpDueDate;
        private Button btnAdd;
        private TextBox txtSearch;
        private ComboBox cmbFilter;
        private ListBox lstTasks;
        private Button btnComplete;
        private Button btnRename;
        private Button btnDelete;
        private Label lblStatus;
    }
}
