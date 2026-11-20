using B2BWholesale.Data.Models;
using B2BWholesale.Data.Services;

namespace BackOffice.WinForms
{
    public partial class MainForm : Form
    {
        private readonly IInventoryService _inventoryService;
        private readonly IAgentManagementService _agentService;
        private readonly IOrderManagementService _orderService;
        private readonly IShippingManifestService _manifestService;
        private readonly IShippingManifestPdfGenerator _manifestPdfGenerator;
        private readonly IDashboardService _dashboardService;
        private DashboardControl? _dashboardControl;
        private readonly User _currentUser;
        private TabControl tabControl = null!;

        public MainForm(
            IInventoryService inventoryService,
            IAgentManagementService agentService,
            IOrderManagementService orderService,
            IShippingManifestService manifestService,
            IShippingManifestPdfGenerator manifestPdfGenerator,
            IDashboardService dashboardService,
            User currentUser)
        {
            InitializeComponent();
            _inventoryService = inventoryService;
            _agentService = agentService;
            _orderService = orderService;
            _manifestService = manifestService;
            _manifestPdfGenerator = manifestPdfGenerator;
            _dashboardService = dashboardService;
            _currentUser = currentUser;
            this.Text = $"B2B Wholesale - Admin Dashboard ({currentUser.UserName})";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeTabControl();
            LoadItemsTab();
            LoadAgentsTab();
            LoadOrdersTab();
            LoadDashboardTab();
        }

        private void InitializeTabControl()
        {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            var tabItems = new TabPage("Items");
            var tabAgents = new TabPage("Agents");
            var tabOrders = new TabPage("Orders");
            var tabDashboard = new TabPage("Dashboard");

            tabControl.TabPages.Add(tabItems);
            tabControl.TabPages.Add(tabAgents);
            tabControl.TabPages.Add(tabOrders);
            tabControl.TabPages.Add(tabDashboard);

            // Task 1.4: refresh the dashboard every time its tab is opened (new orders / agents / stock)
            tabControl.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl.SelectedTab == tabDashboard)
                {
                    _dashboardControl?.Reload();
                }
            };

            this.Controls.Add(tabControl);
        }

        // TASK 1.4 - Dashboard tab
        private void LoadDashboardTab()
        {
            var tabDashboard = tabControl.TabPages[3];
            tabDashboard.Controls.Clear();

            _dashboardControl = new DashboardControl(
                _dashboardService, _agentService, AppConfig.LowStockThreshold);
            tabDashboard.Controls.Add(_dashboardControl);
            _dashboardControl.Reload();
        }

        private void LoadItemsTab()
        {
            var tabItems = tabControl.TabPages[0];
            tabItems.Controls.Clear();

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, Margin = new Padding(0, 0, 0, 10) };
            var btnAddItem = new Button { Text = "Add Item", Width = 100, Height = 30 };
            btnAddItem.Click += (s, e) => ShowAddItemForm();
            toolbar.Controls.Add(btnAddItem);
            panel.Controls.Add(toolbar);

            // DataGridView for items
            var dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                ColumnHeadersHeight = 32,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            dgvItems.Columns.Add("ItemID", "ID");
            dgvItems.Columns.Add("ItemName", "Item Name");
            dgvItems.Columns.Add("Size", "Packaging / Unit");
            dgvItems.Columns.Add("Price", "Unit Price");
            dgvItems.Columns.Add("Stock", "Stock Quantity");

            // Context menu for grid
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Edit", null, (s, e) => ShowEditItemForm(dgvItems));
            contextMenu.Items.Add("Delete", null, (s, e) => DeleteItem(dgvItems));
            dgvItems.ContextMenuStrip = contextMenu;

            RefreshItemsGrid(dgvItems);
            panel.Controls.Add(dgvItems);
            dgvItems.BringToFront();
            tabItems.Controls.Add(panel);
        }

        private void RefreshItemsGrid(DataGridView dgv)
        {
            dgv.Rows.Clear();
            var items = _inventoryService.GetItems();
            foreach (var item in items)
            {
                dgv.Rows.Add(item.ItemId, item.ItemName, item.Size, item.Price, item.StockQuantity);
            }
        }

        private void ShowAddItemForm()
        {
            var form = new ItemForm(_inventoryService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadItemsTab();
            }
        }

        private void ShowEditItemForm(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to edit.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int itemId = Convert.ToInt32(dgv.SelectedRows[0].Cells[0].Value);
            var item = _inventoryService.GetItem(itemId);
            if (item != null)
            {
                var form = new ItemForm(_inventoryService, item);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadItemsTab();
                }
            }
        }

        private void DeleteItem(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to delete.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                int itemId = Convert.ToInt32(dgv.SelectedRows[0].Cells[0].Value);
                _inventoryService.DeleteItem(itemId);
                LoadItemsTab();
                MessageBox.Show("Item deleted successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting item: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAgentsTab()
        {
            var tabAgents = tabControl.TabPages[1];
            tabAgents.Controls.Clear();

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, Margin = new Padding(0, 0, 0, 10) };
            var btnAddAgent = new Button { Text = "Add Agent", Width = 100, Height = 30 };
            btnAddAgent.Click += (s, e) => ShowAddAgentForm();
            toolbar.Controls.Add(btnAddAgent);
            panel.Controls.Add(toolbar);

            // DataGridView for agents
            var dgvAgents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                ColumnHeadersHeight = 32,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            dgvAgents.Columns.Add("AgentID", "ID");
            dgvAgents.Columns.Add("AgentName", "Agent Name");
            dgvAgents.Columns.Add("Address", "Address");
            dgvAgents.Columns.Add("Phone", "Phone");

            // Context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Edit", null, (s, e) => ShowEditAgentForm(dgvAgents));
            contextMenu.Items.Add("Delete", null, (s, e) => DeleteAgent(dgvAgents));
            dgvAgents.ContextMenuStrip = contextMenu;

            RefreshAgentsGrid(dgvAgents);
            panel.Controls.Add(dgvAgents);
            dgvAgents.BringToFront();
            tabAgents.Controls.Add(panel);
        }

        private void RefreshAgentsGrid(DataGridView dgv)
        {
            dgv.Rows.Clear();
            var agents = _agentService.GetAgents();
            foreach (var agent in agents)
            {
                dgv.Rows.Add(agent.AgentId, agent.AgentName, agent.Address, agent.Phone);
            }
        }

        private void ShowAddAgentForm()
        {
            var form = new AgentForm(_agentService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadAgentsTab();
            }
        }

        private void ShowEditAgentForm(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an agent to edit.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int agentId = Convert.ToInt32(dgv.SelectedRows[0].Cells[0].Value);
            var agent = _agentService.GetAgent(agentId);
            if (agent != null)
            {
                var form = new AgentForm(_agentService, agent);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadAgentsTab();
                }
            }
        }

        private void DeleteAgent(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an agent to delete.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this agent?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                int agentId = Convert.ToInt32(dgv.SelectedRows[0].Cells[0].Value);
                _agentService.DeleteAgent(agentId);
                LoadAgentsTab();
                MessageBox.Show("Agent deleted successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting agent: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrdersTab()
        {
            var tabOrders = tabControl.TabPages[2];
            tabOrders.Controls.Clear();

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            // Top panel: orders grid
            var dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                ColumnHeadersHeight = 32,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                Margin = new Padding(10)
            };

            dgvOrders.Columns.Add("OrderID", "ID");
            dgvOrders.Columns.Add("OrderDate", "Order Date");
            dgvOrders.Columns.Add("AgentName", "Agent Name");
            dgvOrders.Columns.Add("OrderStatus", "Status");
            dgvOrders.Columns.Add("Total", "Order Total");

            // Status combo button and manifest print button
            var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10) };
            var toolbar1 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };

            var lblStatus = new Label { Text = "Change Status:", Width = 100, TextAlign = ContentAlignment.MiddleLeft };
            var cmbStatus = new ComboBox { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(_orderService.GetAllowedStatuses().ToArray());
            var btnChangeStatus = new Button { Text = "Update", Width = 80, Height = 30 };
            btnChangeStatus.Click += (s, e) => UpdateOrderStatus(dgvOrders, cmbStatus);

            var btnPrintManifest = new Button { Text = "Print Manifest", Width = 120, Height = 30, Margin = new Padding(10, 0, 0, 0) };
            btnPrintManifest.Click += (s, e) => PrintShippingManifest(dgvOrders);

            toolbar1.Controls.Add(lblStatus);
            toolbar1.Controls.Add(cmbStatus);
            toolbar1.Controls.Add(btnChangeStatus);
            toolbar1.Controls.Add(btnPrintManifest);

            statusPanel.Controls.Add(toolbar1);

            RefreshOrdersGrid(dgvOrders);
            splitContainer.Panel1.Controls.Add(dgvOrders);
            splitContainer.Panel1.Controls.Add(statusPanel);
            dgvOrders.BringToFront();

            // Bottom panel: order details
            var dgvOrderDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ColumnHeadersVisible = true,
                ColumnHeadersHeight = 32,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                Margin = new Padding(10)
            };

            dgvOrderDetails.Columns.Add("ItemName", "Item Name");
            dgvOrderDetails.Columns.Add("Size", "Packaging / Unit");
            dgvOrderDetails.Columns.Add("Quantity", "Quantity");
            dgvOrderDetails.Columns.Add("UnitAmount", "Unit Price");
            dgvOrderDetails.Columns.Add("LineTotal", "Line Total");

            splitContainer.Panel2.Controls.Add(dgvOrderDetails);

            // Wire up grid selection change
            dgvOrders.SelectionChanged += (s, e) =>
            {
                if (dgvOrders.SelectedRows.Count > 0)
                {
                    int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells[0].Value);
                    RefreshOrderDetailsGrid(dgvOrderDetails, orderId);
                }
            };

            tabOrders.Controls.Add(splitContainer);
        }

        private void RefreshOrdersGrid(DataGridView dgv)
        {
            dgv.Rows.Clear();
            foreach (var order in _orderService.GetOrders())
            {
                dgv.Rows.Add(order.OrderId, order.OrderDate.ToString("yyyy-MM-dd"),
                    order.AgentName, order.OrderStatus, order.Total);
            }
        }

        private void RefreshOrderDetailsGrid(DataGridView dgv, int orderId)
        {
            dgv.Rows.Clear();
            try
            {
                foreach (var detail in _orderService.GetOrderDetails(orderId))
                {
                    dgv.Rows.Add(detail.ItemName, detail.Size,
                        detail.Quantity, detail.UnitPrice, detail.LineTotal);
                }
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Order Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateOrderStatus(DataGridView dgv, ComboBox cmbStatus)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbStatus.SelectedItem is not string status)
            {
                MessageBox.Show("Please select a status.", "No Status", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int orderId = Convert.ToInt32(dgv.SelectedRows[0].Cells[0].Value);
                _orderService.UpdateOrderStatus(orderId, status);
                RefreshOrdersGrid(dgv);
                MessageBox.Show("Order status updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating order: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintShippingManifest(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order to generate the manifest.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int orderId = Convert.ToInt32(dgv.SelectedRows[0].Cells[0].Value);
                var manifest = _manifestService.GetManifest(orderId);
                var pdf = _manifestPdfGenerator.GeneratePdf(manifest);
                using var dialog = new SaveFileDialog
                {
                    Title = "Save Shipping Manifest",
                    Filter = "PDF document (*.pdf)|*.pdf",
                    FileName = $"ShippingManifest-{orderId}.pdf",
                    DefaultExt = "pdf",
                    AddExtension = true
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllBytes(dialog.FileName, pdf);
                    MessageBox.Show("Shipping Manifest PDF saved successfully.", "Report Generated",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating manifest: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
