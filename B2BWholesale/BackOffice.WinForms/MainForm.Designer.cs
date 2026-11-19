namespace BackOffice.WinForms
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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Load += MainForm_Load;
        }
    }
}
