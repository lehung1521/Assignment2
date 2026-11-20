using System.Globalization;
using B2BWholesale.Data.Services;

namespace BackOffice.WinForms;

/// <summary>
/// Task 1.4 - Dashboard / advanced data filter.
/// Shows best-selling items, a low-stock warning and total revenue by agent.
/// Cancelled orders are not counted as sales.
/// </summary>
public sealed class DashboardControl : UserControl
{
    private static readonly CultureInfo Vietnam = CultureInfo.GetCultureInfo("vi-VN");

    private readonly IDashboardService _dashboard;
    private readonly IAgentManagementService _agents;

    private readonly DateTimePicker _from = CreateDatePicker();
    private readonly DateTimePicker _to = CreateDatePicker();
    private readonly NumericUpDown _top = new() { Minimum = 1, Maximum = 100, Value = 10, Width = 60 };
    private readonly NumericUpDown _threshold = new() { Minimum = 0, Maximum = 100000, Width = 70 };
    private readonly ComboBox _agentCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };

    private readonly Label _kpiRevenue = new();
    private readonly Label _kpiOrders = new();
    private readonly Label _kpiLowStock = new();
    private readonly Label _lowStockBanner = new();

    private readonly DataGridView _gridBest = CreateGrid();
    private readonly DataGridView _gridLow = CreateGrid();
    private readonly DataGridView _gridRevenue = CreateGrid();

    private readonly Font _boldFont;
    private bool _suspendEvents;

    private sealed record AgentChoice(int? AgentId, string Name);

    public DashboardControl(IDashboardService dashboard, IAgentManagementService agents, int defaultLowStockThreshold)
    {
        _dashboard = dashboard;
        _agents = agents;
        _threshold.Value = Math.Clamp(defaultLowStockThreshold, (int)_threshold.Minimum, (int)_threshold.Maximum);
        _boldFont = new Font(Font, FontStyle.Bold);

        Dock = DockStyle.Fill;
        BuildLayout();
        ConfigureGrids();

        _gridLow.DataBindingComplete += (_, _) => HighlightLowStockRows();
    }

    /// <summary>Reloads the agent list and every query (called when the tab is opened).</summary>
    public void Reload()
    {
        LoadAgents();
        RefreshData();
    }

    // ------------------------------------------------------------------ layout

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildFilterBar(), 0, 0);
        root.Controls.Add(BuildKpiRow(), 0, 1);
        root.Controls.Add(BuildGridArea(), 0, 2);

        Controls.Add(root);
    }

    private Control BuildFilterBar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        bar.Controls.Add(MakeLabel("From:"));
        bar.Controls.Add(_from);
        bar.Controls.Add(MakeLabel("To:"));
        bar.Controls.Add(_to);
        bar.Controls.Add(MakeLabel("Top N items:"));
        bar.Controls.Add(_top);
        bar.Controls.Add(MakeLabel("Agent:"));
        bar.Controls.Add(_agentCombo);
        bar.Controls.Add(MakeLabel("Low stock below:"));
        bar.Controls.Add(_threshold);

        var refresh = new Button { Text = "Refresh", Width = 90, Height = 28, Margin = new Padding(12, 2, 0, 0) };
        refresh.Click += (_, _) => RefreshData();
        var clear = new Button { Text = "Clear filters", Width = 100, Height = 28, Margin = new Padding(6, 2, 0, 0) };
        clear.Click += (_, _) => ClearFilters();
        bar.Controls.Add(refresh);
        bar.Controls.Add(clear);

        _agentCombo.SelectedIndexChanged += (_, _) =>
        {
            if (!_suspendEvents) RefreshData();
        };

        return bar;
    }

    private Control BuildKpiRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 8)
        };
        for (var i = 0; i < 3; i++)
        {
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
        }

        row.Controls.Add(MakeKpiCard("Total revenue (selected filters)", _kpiRevenue, Color.FromArgb(0, 102, 204)), 0, 0);
        row.Controls.Add(MakeKpiCard("Orders counted", _kpiOrders, Color.FromArgb(46, 125, 50)), 1, 0);
        row.Controls.Add(MakeKpiCard("Low-stock items", _kpiLowStock, Color.FromArgb(198, 40, 40)), 2, 0);
        return row;
    }

    private Control BuildGridArea()
    {
        var area = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        area.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        area.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        area.Controls.Add(MakeGroup("Best-selling items (units sold)", _gridBest, null), 0, 0);

        _lowStockBanner.Dock = DockStyle.Top;
        _lowStockBanner.Height = 24;
        _lowStockBanner.TextAlign = ContentAlignment.MiddleLeft;
        _lowStockBanner.Font = _boldFont;
        area.Controls.Add(MakeGroup("Low-stock warning", _gridLow, _lowStockBanner), 1, 0);

        var revenueGroup = MakeGroup("Total revenue by agent", _gridRevenue, null);
        area.Controls.Add(revenueGroup, 0, 1);
        area.SetColumnSpan(revenueGroup, 2);

        return area;
    }

    private static GroupBox MakeGroup(string title, DataGridView grid, Control? banner)
    {
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8) };
        if (banner is not null)
        {
            group.Controls.Add(banner);
        }

        group.Controls.Add(grid);
        grid.BringToFront();   // the Fill control must be docked last
        return group;
    }

    private static Control MakeKpiCard(string title, Label valueLabel, Color accent)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 70,
            Margin = new Padding(0, 0, 8, 0),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 24,
            ForeColor = Color.DimGray,
            Padding = new Padding(8, 6, 0, 0)
        };

        valueLabel.Dock = DockStyle.Fill;
        valueLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        valueLabel.ForeColor = accent;
        valueLabel.Padding = new Padding(8, 0, 0, 0);
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        valueLabel.Text = "-";

        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        valueLabel.BringToFront();
        return card;
    }

    private static Label MakeLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Margin = new Padding(10, 7, 2, 0)
    };

    private static DateTimePicker CreateDatePicker() => new()
    {
        Format = DateTimePickerFormat.Short,
        ShowCheckBox = true,
        Checked = false,
        Width = 110
    };

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = Color.White
    };

    // ------------------------------------------------------------------ grids

    private void ConfigureGrids()
    {
        AddText(_gridBest, nameof(BestSellerRow.Rank), "#", 8);
        AddText(_gridBest, nameof(BestSellerRow.ItemName), "Item", 40);
        AddText(_gridBest, nameof(BestSellerRow.Size), "Pack size", 28);
        AddNumber(_gridBest, nameof(BestSellerRow.UnitsSold), "Units sold", 16);
        AddNumber(_gridBest, nameof(BestSellerRow.Revenue), "Revenue (VND)", 24);

        AddText(_gridLow, nameof(LowStockRow.ItemName), "Item", 45);
        AddText(_gridLow, nameof(LowStockRow.Size), "Pack size", 30);
        AddNumber(_gridLow, nameof(LowStockRow.StockQuantity), "In stock", 15);

        AddText(_gridRevenue, nameof(AgentRevenueRow.AgentName), "Agent", 50);
        AddNumber(_gridRevenue, nameof(AgentRevenueRow.OrderCount), "Orders", 15);
        AddNumber(_gridRevenue, nameof(AgentRevenueRow.Revenue), "Total revenue (VND)", 35);
    }

    private static void AddText(DataGridView grid, string property, string header, float weight)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = header,
            FillWeight = weight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private static void AddNumber(DataGridView grid, string property, string header, float weight)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = header,
            FillWeight = weight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        column.DefaultCellStyle.Format = "N0";
        column.DefaultCellStyle.FormatProvider = Vietnam;
        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.Columns.Add(column);
    }

    private void HighlightLowStockRows()
    {
        foreach (DataGridViewRow row in _gridLow.Rows)
        {
            if (row.DataBoundItem is not LowStockRow item)
            {
                continue;
            }

            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 226, 226);
            row.DefaultCellStyle.ForeColor = Color.DarkRed;
            if (item.StockQuantity == 0)
            {
                row.DefaultCellStyle.Font = _boldFont;
            }
        }
    }

    // ------------------------------------------------------------------ data

    private void LoadAgents()
    {
        var previous = (_agentCombo.SelectedItem as AgentChoice)?.AgentId;

        _suspendEvents = true;
        try
        {
            var choices = new List<AgentChoice> { new(null, "All agents") };
            choices.AddRange(_agents.GetAgents().Select(a => new AgentChoice(a.AgentId, a.AgentName)));

            _agentCombo.DataSource = choices;
            _agentCombo.DisplayMember = nameof(AgentChoice.Name);
            _agentCombo.SelectedIndex = Math.Max(0, choices.FindIndex(c => c.AgentId == previous));
        }
        finally
        {
            _suspendEvents = false;
        }
    }

    private void ClearFilters()
    {
        _suspendEvents = true;
        try
        {
            _from.Checked = false;
            _to.Checked = false;
            _top.Value = 10;
            if (_agentCombo.Items.Count > 0) _agentCombo.SelectedIndex = 0;
        }
        finally
        {
            _suspendEvents = false;
        }

        RefreshData();
    }

    private void RefreshData()
    {
        try
        {
            DateTime? from = _from.Checked ? _from.Value.Date : null;
            DateTime? to = _to.Checked ? _to.Value.Date : null;
            int? agentId = (_agentCombo.SelectedItem as AgentChoice)?.AgentId;
            var threshold = (int)_threshold.Value;

            var best = _dashboard.GetBestSellingItems(from, to, (int)_top.Value);
            var low = _dashboard.GetLowStockItems(threshold);
            var revenue = _dashboard.GetRevenueByAgent(agentId, from, to);

            _gridBest.DataSource = best.ToList();
            _gridLow.DataSource = low.ToList();
            _gridRevenue.DataSource = revenue.ToList();

            _kpiRevenue.Text = FormatMoney(revenue.Sum(r => r.Revenue));
            _kpiOrders.Text = revenue.Sum(r => r.OrderCount).ToString("N0", Vietnam);
            _kpiLowStock.Text = low.Count.ToString(Vietnam);

            if (low.Count == 0)
            {
                _lowStockBanner.ForeColor = Color.ForestGreen;
                _lowStockBanner.Text = $"All items have at least {threshold} units in stock.";
            }
            else
            {
                _lowStockBanner.ForeColor = Color.Firebrick;
                _lowStockBanner.Text = $"WARNING: {low.Count} item(s) have fewer than {threshold} units - restock soon.";
            }
        }
        catch (ServiceValidationException ex)
        {
            MessageBox.Show(ex.Message, "Invalid filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not load the dashboard: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string FormatMoney(decimal amount) => amount.ToString("N0", Vietnam) + " VND";

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _boldFont.Dispose();
        }

        base.Dispose(disposing);
    }
}
