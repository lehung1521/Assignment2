using B2BWholesale.Data.Services;

namespace BackOffice.WinForms
{
    public partial class ItemForm : Form
    {
        private readonly IInventoryService _inventoryService;
        private readonly ItemRecord? _existingItem;

        public ItemForm(IInventoryService inventoryService, ItemRecord? existingItem = null)
        {
            InitializeComponent();
            _inventoryService = inventoryService;
            _existingItem = existingItem;
            this.Text = existingItem == null ? "Add New Item" : "Edit Item";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void ItemForm_Load(object sender, EventArgs e)
        {
            if (_existingItem != null)
            {
                txtItemName.Text = _existingItem.ItemName;
                txtSize.Text = _existingItem.Size;
                txtPrice.Text = _existingItem.Price.ToString("F2");
                txtStock.Text = _existingItem.StockQuantity.ToString();
            }
            txtItemName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var input = new ItemInput(txtItemName.Text, txtSize.Text, txtPrice.Text, txtStock.Text);
                if (_existingItem is null)
                    _inventoryService.CreateItem(input);
                else
                    _inventoryService.UpdateItem(_existingItem.ItemId, input);

                MessageBox.Show(_existingItem is null ? "Item added successfully." : "Item updated successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving item: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

    }
}
