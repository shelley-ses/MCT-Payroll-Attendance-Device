using System.Drawing.Drawing2D;
using System.Text;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Forms.Pages;

/// <summary>
/// Payroll Processing page: workflow actions, summary KPIs, company/search filters, and payroll calculation table.
/// </summary>
public class PayrollProcessingControl : UserControl
{
    private const int PageSize = 8;

    private sealed class PayrollItem
    {
        public required EmployeeRecord Employee { get; init; }
        public decimal GrossPay { get; init; }
        public decimal Deductions { get; init; }
        public decimal NetPay { get; init; }
        public string Status { get; set; } = "Calculated";
    }

    private readonly List<PayrollItem> _items;
    private readonly SearchInput _searchInput;
    private readonly FilterDropdown _companyFilter;
    private readonly StyledDataGridView _grid;
    private readonly EmptyState _emptyState;
    private readonly PaginationBar _pagination;
    private readonly StatCard _cardGross;
    private readonly StatCard _cardDeductions;
    private readonly StatCard _cardNet;
    private readonly StatCard _cardStatus;

    private string _selectedCompany = "All Companies";
    private int _currentPage = 1;

    public PayrollProcessingControl()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Dock = DockStyle.Fill;
        Padding = new Padding(0, 8, 0, 12);

        _items = EmployeeDirectoryService.CreateDefaultEmployees()
            .Where(e => e.Status == "Active")
            .Select(e =>
            {
                decimal gross = e.BasicSalary + e.Allowances;
                decimal sss = Math.Round(e.BasicSalary * 0.045m, 2);
                decimal ph = Math.Round(e.BasicSalary * 0.025m, 2);
                decimal pi = 200m;
                decimal tax = Math.Max(0m, Math.Round((gross - 20833m) * 0.15m, 2));
                decimal ded = sss + ph + pi + tax;
                return new PayrollItem
                {
                    Employee = e,
                    GrossPay = gross,
                    Deductions = ded,
                    NetPay = gross - ded,
                    Status = "Calculated"
                };
            }).ToList();

        // Action bar
        var actionBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(AppTheme.Spacing.PagePadding, 0, AppTheme.Spacing.PagePadding, 8)
        };

        var actionFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        var btnExport = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Export Payroll",
            Size = new Size(136, 36),
            Font = AppTheme.Typography.CreateFont(10f, FontStyle.Bold),
            Margin = new Padding(0, 0, 10, 0)
        };
        btnExport.Click += (_, _) => ExportCsv();

        var btnCalculate = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Recalculate",
            Size = new Size(124, 36),
            Font = AppTheme.Typography.CreateFont(10f, FontStyle.Bold),
            Margin = new Padding(0, 0, 10, 0)
        };
        btnCalculate.Click += (_, _) =>
        {
            foreach (var item in _items) item.Status = "Calculated";
            RefreshSummaryCards();
            BindRows();
        };

        var btnApprove = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            CornerRadius = AppTheme.Radii.InputLarge,
            Text = "Approve & Process",
            Size = new Size(164, 36),
            Font = AppTheme.Typography.CreateFont(10f, FontStyle.Bold)
        };
        btnApprove.Click += (_, _) =>
        {
            foreach (var item in _items) item.Status = "Approved";
            RefreshSummaryCards();
            BindRows();
            MessageBox.Show(FindForm(), "Payroll batch has been approved and queued for payslip generation.", "Payroll Approved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        actionFlow.Controls.Add(btnExport);
        actionFlow.Controls.Add(btnCalculate);
        actionFlow.Controls.Add(btnApprove);
        actionBar.Controls.Add(actionFlow);

        // Summary KPI Row
        var kpiRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 132,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(AppTheme.Spacing.PagePadding, 0, AppTheme.Spacing.PagePadding, 12)
        };
        for (int i = 0; i < 4; i++) kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        kpiRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _cardGross = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), Title = "Total Gross Pay", Icon = StatCardIcon.Payroll, AccentColor = AppTheme.Colors.PrimarySolid };
        _cardDeductions = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(4, 0, 6, 0), Title = "Total Deductions", Icon = StatCardIcon.Alert, AccentColor = AppTheme.Colors.ErrorBorder };
        _cardNet = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 4, 0), Title = "Total Net Pay", Icon = StatCardIcon.Present, AccentColor = AppTheme.Colors.StatusActiveFg };
        _cardStatus = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), Title = "Batch Status", Icon = StatCardIcon.Employees, AccentColor = AppTheme.Colors.PrimaryDeep };

        kpiRow.Controls.Add(_cardGross, 0, 0);
        kpiRow.Controls.Add(_cardDeductions, 1, 0);
        kpiRow.Controls.Add(_cardNet, 2, 0);
        kpiRow.Controls.Add(_cardStatus, 3, 0);

        // Toolbar
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 56,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(AppTheme.Spacing.PagePadding, 4, AppTheme.Spacing.PagePadding, 10)
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 164f + AppTheme.Spacing.PagePadding));

        _searchInput = new SearchInput
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0),
            PlaceholderText = "Search employee name or ID..."
        };
        _searchInput.SearchTextChanged += (_, _) => { _currentPage = 1; BindRows(); };

        _companyFilter = new FilterDropdown { Dock = DockStyle.Fill };
        _companyFilter.SetItems(["All Companies", "MCT Hotel", "MCT Corp", "MCT Tech", "MCT Logistics"], "All Companies");
        _companyFilter.SelectedItemChanged += (_, val) => { _selectedCompany = val; _currentPage = 1; BindRows(); };

        toolbar.Controls.Add(_searchInput, 0, 0);
        toolbar.Controls.Add(_companyFilter, 1, 0);

        // Grid
        var gridHost = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Colors.SurfaceWhite };
        _grid = new StyledDataGridView
        {
            Dock = DockStyle.Fill,
            HeaderBackColor = AppTheme.Colors.PrimarySolid,
            HeaderForeColor = Color.White,
            ColumnHeadersHeight = AppTheme.Spacing.TableHeaderCompactHeight,
            ReadOnly = true
        };
        _grid.HandleCreated += (_, _) => _grid.ClearSelection();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Employee", HeaderText = "EMPLOYEE", FillWeight = 24 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Company", HeaderText = "COMPANY", FillWeight = 14 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Basic", HeaderText = "BASIC + ALLOW.", FillWeight = 16 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Gross", HeaderText = "GROSS PAY", FillWeight = 14 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Deductions", HeaderText = "DEDUCTIONS", FillWeight = 14 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Net", HeaderText = "NET PAY", FillWeight = 14 });
        var colStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "STATUS", FillWeight = 12 };
        colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grid.Columns.Add(colStatus);

        _grid.CellPainting += Grid_CellPainting;

        _emptyState = new EmptyState
        {
            Title = "No payroll records found",
            Description = "Try clearing your search or company filter.",
            ActionText = "Reset filters",
            Visible = false
        };
        _emptyState.ActionClicked += (_, _) =>
        {
            _searchInput.Clear();
            _companyFilter.SelectedItem = "All Companies";
            _selectedCompany = "All Companies";
            _currentPage = 1;
            BindRows();
        };

        gridHost.Controls.Add(_grid);
        gridHost.Controls.Add(_emptyState);

        _pagination = new PaginationBar
        {
            Dock = DockStyle.Bottom,
            Padding = new Padding(AppTheme.Spacing.PagePadding, 8, AppTheme.Spacing.PagePadding, 0)
        };
        _pagination.PageChanged += (_, page) => { _currentPage = page; BindRows(); };

        Controls.Add(gridHost);
        Controls.Add(toolbar);
        Controls.Add(kpiRow);
        Controls.Add(actionBar);
        Controls.Add(_pagination);

        RefreshSummaryCards();
        BindRows();
    }

    private void RefreshSummaryCards()
    {
        _cardGross.Value = $"₱{_items.Sum(i => i.GrossPay):N0}";
        _cardGross.Subtext = $"{_items.Count} active employees";
        _cardDeductions.Value = $"₱{_items.Sum(i => i.Deductions):N0}";
        _cardDeductions.Subtext = "SSS, PhilHealth, Pag-IBIG, Tax";
        _cardNet.Value = $"₱{_items.Sum(i => i.NetPay):N0}";
        _cardNet.Subtext = "May 1 – May 15, 2025";
        bool allApproved = _items.All(i => i.Status == "Approved");
        _cardStatus.Value = allApproved ? "Approved" : "Review";
        _cardStatus.Subtext = allApproved ? "Ready for payslips" : "Awaiting final approval";
    }

    private List<PayrollItem> GetFiltered()
    {
        string q = _searchInput.Text.Trim();
        return _items.Where(i =>
            (_selectedCompany == "All Companies" || i.Employee.Company == _selectedCompany) &&
            (string.IsNullOrEmpty(q) ||
             i.Employee.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             i.Employee.EmployeeId.Contains(q, StringComparison.OrdinalIgnoreCase))).ToList();
    }

    private void BindRows()
    {
        var filtered = GetFiltered();
        int totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
        _currentPage = Math.Max(1, Math.Min(_currentPage, totalPages));
        var pageRows = filtered.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

        _grid.Rows.Clear();
        foreach (var item in pageRows)
        {
            int idx = _grid.Rows.Add(
                item.Employee.FullName,
                item.Employee.Company,
                $"₱{item.Employee.BasicSalary:N0} + ₱{item.Employee.Allowances:N0}",
                $"₱{item.GrossPay:N2}",
                $"₱{item.Deductions:N2}",
                $"₱{item.NetPay:N2}",
                item.Status);
            _grid.Rows[idx].Tag = item;
        }
        _grid.ClearSelection();
        _grid.Visible = pageRows.Count > 0;
        _emptyState.Visible = pageRows.Count == 0;
        _pagination.SetPageInfo(pageRows.Count, filtered.Count, _currentPage, totalPages, "payroll records");
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics is null || _grid.Rows[e.RowIndex].Tag is not PayrollItem item)
        {
            return;
        }

        using var bgBrush = new SolidBrush(AppTheme.Colors.SurfaceWhite);
        e.Graphics.FillRectangle(bgBrush, e.CellBounds);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };

        string col = _grid.Columns[e.ColumnIndex].Name;
        if (col == "Employee")
        {
            using var nameFont = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
            using var idFont = AppTheme.Typography.CreateFont(9f, FontStyle.Regular);
            using var primaryBrush = new SolidBrush(AppTheme.Colors.TextPrimary);
            using var mutedBrush = new SolidBrush(AppTheme.Colors.TextMuted);
            e.Graphics.DrawString(item.Employee.FullName, nameFont, primaryBrush, new RectangleF(e.CellBounds.X + 12, e.CellBounds.Y + 12, e.CellBounds.Width - 20, 22), sfLeft);
            e.Graphics.DrawString($"{item.Employee.EmployeeId} · {item.Employee.Position}", idFont, mutedBrush, new RectangleF(e.CellBounds.X + 12, e.CellBounds.Y + 34, e.CellBounds.Width - 20, 18), sfLeft);
            e.Handled = true;
        }
        else if (col == "Status")
        {
            bool approved = item.Status == "Approved";
            Color bg = approved ? AppTheme.Colors.StatusActiveBg : AppTheme.Colors.SurfaceInput;
            Color fg = approved ? AppTheme.Colors.StatusActiveFg : AppTheme.Colors.PrimaryDeep;
            int pillW = Math.Min(96, e.CellBounds.Width - 12);
            var pillRect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - pillW) / 2, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, pillW, 24);
            using var path = AppTheme.CreateRoundedRectPath(pillRect, 12);
            using var pBrush = new SolidBrush(bg);
            using var fBrush = new SolidBrush(fg);
            using var font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold);
            e.Graphics.FillPath(pBrush, path);
            e.Graphics.DrawString(item.Status, font, fBrush, pillRect, sfCenter);
            e.Handled = true;
        }
        else
        {
            string val = e.FormattedValue?.ToString() ?? string.Empty;
            using var font = AppTheme.Typography.CreateFont(10.5f, col == "Net" ? FontStyle.Bold : FontStyle.Regular);
            using var brush = new SolidBrush(col == "Net" ? AppTheme.Colors.TextDark : AppTheme.Colors.TextPrimary);
            e.Graphics.DrawString(val, font, brush, new RectangleF(e.CellBounds.X + 10, e.CellBounds.Y, Math.Max(20, e.CellBounds.Width - 20), e.CellBounds.Height), sfLeft);
            e.Handled = true;
        }
    }

    private void ExportCsv()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"MCT_Payroll_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (sfd.ShowDialog(FindForm()) != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("EmployeeId,FullName,Company,BasicSalary,Allowances,GrossPay,Deductions,NetPay,Status");
        foreach (var item in GetFiltered())
        {
            sb.AppendLine($"\"{item.Employee.EmployeeId}\",\"{item.Employee.FullName}\",\"{item.Employee.Company}\",{item.Employee.BasicSalary},{item.Employee.Allowances},{item.GrossPay},{item.Deductions},{item.NetPay},\"{item.Status}\"");
        }
        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show(FindForm(), "Payroll summary exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
