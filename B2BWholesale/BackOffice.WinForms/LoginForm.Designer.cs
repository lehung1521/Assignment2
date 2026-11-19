namespace BackOffice.WinForms
{
    partial class LoginForm
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

        private void InitializeComponent()
        {
            var tableLayout = new TableLayoutPanel();
            var lblUsername = new Label();
            var lblPassword = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            btnExit = new Button();

            this.SuspendLayout();

            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 2;
            tableLayout.RowCount = 5;
            tableLayout.Padding = new Padding(20);
            tableLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            // Username label
            lblUsername.Text = "Username/Email:";
            lblUsername.TextAlign = ContentAlignment.MiddleRight;
            lblUsername.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(lblUsername, 0, 1);

            // Username textbox
            txtUsername.Dock = DockStyle.Fill;
            txtUsername.Margin = new Padding(5);
            tableLayout.Controls.Add(txtUsername, 1, 1);

            // Password label
            lblPassword.Text = "Password:";
            lblPassword.TextAlign = ContentAlignment.MiddleRight;
            lblPassword.Dock = DockStyle.Fill;
            tableLayout.Controls.Add(lblPassword, 0, 2);

            // Password textbox
            txtPassword.Dock = DockStyle.Fill;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Margin = new Padding(5);
            txtPassword.KeyDown += txtPassword_KeyDown;
            tableLayout.Controls.Add(txtPassword, 1, 2);

            // Login button
            btnLogin.Text = "Login";
            btnLogin.Height = 30;
            btnLogin.Click += btnLogin_Click;
            tableLayout.Controls.Add(btnLogin, 0, 3);

            // Exit button
            btnExit.Text = "Exit";
            btnExit.Height = 30;
            btnExit.Click += btnExit_Click;
            tableLayout.Controls.Add(btnExit, 1, 3);

            this.Controls.Add(tableLayout);
            this.ClientSize = new Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Load += LoginForm_Load;

            this.ResumeLayout(false);
        }

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
    }
}
