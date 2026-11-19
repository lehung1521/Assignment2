namespace BackOffice.WinForms
{
    partial class AgentForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtAgentName;
        private TextBox txtAddress;
        private TextBox txtPhone;
        private Button btnSave;
        private Button btnCancel;

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
            components = new System.ComponentModel.Container();

            var lblAgentName = new Label { Text = "Agent Name:", AutoSize = true };
            txtAgentName = new TextBox { Width = 200 };

            var lblAddress = new Label { Text = "Address:", AutoSize = true };
            txtAddress = new TextBox { Width = 200, Multiline = true, Height = 60 };

            var lblPhone = new Label { Text = "Phone:", AutoSize = true };
            txtPhone = new TextBox { Width = 200 };

            btnSave = new Button { Text = "Save", Width = 80, Height = 30 };
            btnCancel = new Button { Text = "Cancel", Width = 80, Height = 30 };

            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;

            this.Load += AgentForm_Load;

            // Layout
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(20),
                AutoSize = false
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 5; i++)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            panel.Controls.Add(lblAgentName, 0, 0);
            panel.Controls.Add(txtAgentName, 1, 0);

            panel.Controls.Add(lblAddress, 0, 1);
            panel.Controls.Add(txtAddress, 1, 1);

            panel.Controls.Add(lblPhone, 0, 2);
            panel.Controls.Add(txtPhone, 1, 2);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            this.Controls.Add(panel);
            this.Controls.Add(btnPanel);

            this.ClientSize = new System.Drawing.Size(350, 280);
        }
    }
}
