using System.Drawing.Drawing2D;
using System.Text;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Forms.Pages;

/// <summary>
/// Deductions & Contributions page: SSS, PhilHealth, Pag-IBIG, withholding tax, and loans breakdown.
/// </summary>
public class DeductionsControl : UserControl
{
    private const int PageSize = 8;

    private sealed record DeductionRow(
        EmployeeRecord Employee,
        decimal Sss,
        decimal PhilHealth,
        decimal PagIbig,
        decimal Tax,
        decimal Loans,
        decimal Total);

    private readonly List<DeductionRow> _rows;
    private readonly SearchInput _searchInput;
    private readonly FilterDropdown _companyFilter;
    private readonly StyledDataGridView _grid;
    private readonly EmptyState _emptyState;
    private readonly PaginationBar _pagination;

    private string _selectedCompany = "All Companies";
    private int _currentPage = 1;

    public DeductionsControl()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Dock = DockStyle.Fill;
        Padding = new Padding(0, 8, 0, 12);

        _rows = EmployeeDirectoryService.CreateDefaultEmployees()
            .Where(e => e.Status == "Active")
            .Select(e =>
            {
                decimal sss = Math.Round(e.BasicSalary * 0.045m, 2);
                decimal ph = Math.Round(e.BasicSalary * 0.025m, 2);
                decimal pi = 200m;
                decimal tax = Math.Max(0m, Math.Round((e.BasicSalary + e.Allowances - 20833m) * 0.15m, 2));
                decimal loans = e.BasicSalary > 30000m ? 1000m : 0m;
                return new DeductionRow(e, sss, ph, pi, tax, loans, sss + ph + pi + tax + loans);
            }).ToList();

        // Action Bar
        var actionBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(AppTheme.Spacing.PagePadding, 0, AppTheme.Spacing.PagePadding, 8)
        };

        var btnExport = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            CornerRadius = AppTheme.Radii.InputLarge,
            Text = "Export Remittances",
            Size = new Size(164, 36),
            Dock = DockStyle.Right,
            Font = AppTheme.Typography.CreateFont(10f, FontStyle.Bold)
        };
        btnExport.Click += (_, _) => ExportCsv();
        actionBar.Controls.Add(btnExport);

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

        kpiRow.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), Title = "Total SSS", Value = $"₱{_rows.Sum(r => r.Sss):N0}", Subtext = "Employee share", Icon = StatCardIcon.Employees, AccentColor = AppTheme.Colors.PrimarySolid }, 0, 0);
        kpiRow.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(4, 0, 6, 0), Title = "Total PhilHealth", Value = $"₱{_rows.Sum(r => r.PhilHealth):N0}", Subtext = "Monthly contribution", Icon = StatCardIcon.Present, AccentColor = AppTheme.Colors.StatusActiveFg }, 1, 0);
        kpiRow.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 4, 0), Title = "Total Pag-IBIG", Value = $"₱{_rows.Sum(r => r.PagIbig):N0}", Subtext = "Mandatory HDMF", Icon = StatCardIcon.Payroll, AccentColor = AppTheme.Colors.PrimaryDeep }, 2, 0);
        kpiRow.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), Title = "Withholding Tax", Value = $"₱{_rows.Sum(r => r.Tax):N0}", Subtext = "BIR statutory tax", Icon = StatCardIcon.Alert, AccentColor = AppTheme.Colors.ErrorBorder }, 3, 0);

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
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sss", HeaderText = "SSS", FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhilHealth", HeaderText = "PHILHEALTH", FillWeight = 13 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PagIbig", HeaderText = "PAG-IBIG", FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tax", HeaderText = "TAX", FillWeight = 13 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Loans", HeaderText = "LOANS", FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "TOTAL", FillWeight = 14 });

        _grid.CellPainting += Grid_CellPainting;

        _emptyState = new EmptyState
        {
            Title = "No deduction records found",
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

        BindRows();
    }

    private List<DeductionRow> GetFiltered()
    {
        string q = _searchInput.Text.Trim();
        return _rows.Where(r =>
            (_selectedCompany == "All Companies" || r.Employee.Company == _selectedCompany) &&
            (string.IsNullOrEmpty(q) ||
             r.Employee.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             r.Employee.EmployeeId.Contains(q, StringComparison.OrdinalIgnoreCase))).ToList();
    }

    private void BindRows()
    {
        var filtered = GetFiltered();
        int totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
        _currentPage = Math.Max(1, Math.Min(_currentPage, totalPages));
        var pageRows = filtered.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

        _grid.Rows.Clear();
        foreach (var r in pageRows)
        {
            int idx = _grid.Rows.Add(
                r.Employee.FullName,
                $"₱{r.Sss:N2}",
                $"₱{r.PhilHealth:N2}",
                $"₱{r.PagIbig:N2}",
                $"₱{r.Tax:N2}",
                $"₱{r.Loans:N2}",
                $"₱{r.Total:N2}");
            _grid.Rows[idx].Tag = r;
        }
        _grid.ClearSelection();
        _grid.Visible = pageRows.Count > 0;
        _emptyState.Visible = pageRows.Count == 0;
        _pagination.SetPageInfo(pageRows.Count, filtered.Count, _currentPage, totalPages, "deduction records");
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics is null || _grid.Rows[e.RowIndex].Tag is not DeductionRow row)
        {
            return;
        }

        using var bgBrush = new SolidBrush(AppTheme.Colors.SurfaceWhite);
        e.Graphics.FillRectangle(bgBrush, e.CellBounds);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };

        string col = _grid.Columns[e.ColumnIndex].Name;
        if (col == "Employee")
        {
            using var nameFont = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
            using var idFont = AppTheme.Typography.CreateFont(9f, FontStyle.Regular);
            using var primaryBrush = new SolidBrush(AppTheme.Colors.TextPrimary);
            using var mutedBrush = new SolidBrush(AppTheme.Colors.TextMuted);
            e.Graphics.DrawString(row.Employee.FullName, nameFont, primaryBrush, new RectangleF(e.CellBounds.X + 12, e.CellBounds.Y + 12, e.CellBounds.Width - 20, 22), sfLeft);
            e.Graphics.DrawString($"{row.Employee.EmployeeId} · {row.Employee.Company}", idFont, mutedBrush, new RectangleF(e.CellBounds.X + 12, e.CellBounds.Y + 34, e.CellBounds.Width - 20, 18), sfLeft);
            e.Handled = true;
        }
        else
        {
            string val = e.FormattedValue?.ToString() ?? string.Empty;
            using var font = AppTheme.Typography.CreateFont(10.5f, col == "Total" ? FontStyle.Bold : FontStyle.Regular);
            using var brush = new SolidBrush(col == "Total" ? AppTheme.Colors.ErrorText : AppTheme.Colors.TextPrimary);
            e.Graphics.DrawString(val, font, brush, new RectangleF(e.CellBounds.X + 10, e.CellBounds.Y, Math.Max(20, e.CellBounds.Width - 20), e.CellBounds.Height), sfLeft);
            e.Handled = true;
        }
    }

    private void ExportCsv()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"MCT_Deductions_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (sfd.ShowDialog(FindForm()) != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("EmployeeId,FullName,Company,SSS,PhilHealth,PagIBIG,Tax,Loans,Total");
        foreach (var r in GetFiltered())
        {
            sb.AppendLine($"\"{r.Employee.EmployeeId}\",\"{r.Employee.FullName}\",\"{r.Employee.Company}\",{r.Sss},{r.PhilHealth},{r.PagIbig},{r.Tax},{r.Loans},{r.Total}");
        }
        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show(FindForm(), "Deductions remittance exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
