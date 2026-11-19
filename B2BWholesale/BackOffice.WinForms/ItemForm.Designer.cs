namespace BackOffice.WinForms
{
    partial class ItemForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtItemName;
        private TextBox txtSize;
        private TextBox txtPrice;
        private TextBox txtStock;
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

            var lblItemName = new Label { Text = "Item Name:", AutoSize = true };
            txtItemName = new TextBox { Width = 200 };

            var lblSize = new Label { Text = "Size:", AutoSize = true };
            txtSize = new TextBox { Width = 200 };

            var lblPrice = new Label { Text = "Price:", AutoSize = true };
            txtPrice = new TextBox { Width = 200 };

            var lblStock = new Label { Text = "Stock Quantity:", AutoSize = true };
            txtStock = new TextBox { Width = 200 };

            btnSave = new Button { Text = "Save", Width = 80, Height = 30 };
            btnCancel = new Button { Text = "Cancel", Width = 80, Height = 30 };

            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;

            this.Load += ItemForm_Load;

            // Layout
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(20),
                AutoSize = false
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 6; i++)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            panel.Controls.Add(lblItemName, 0, 0);
            panel.Controls.Add(txtItemName, 1, 0);

            panel.Controls.Add(lblSize, 0, 1);
            panel.Controls.Add(txtSize, 1, 1);

            panel.Controls.Add(lblPrice, 0, 2);
            panel.Controls.Add(txtPrice, 1, 2);

            panel.Controls.Add(lblStock, 0, 3);
            panel.Controls.Add(txtStock, 1, 3);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            this.Controls.Add(panel);
            this.Controls.Add(btnPanel);

            this.ClientSize = new System.Drawing.Size(350, 250);
        }
    }
}
