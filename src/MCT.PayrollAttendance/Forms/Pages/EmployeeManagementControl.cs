using System.Drawing.Drawing2D;
using System.Text;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Forms.Modals;
using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Forms.Pages;

/// <summary>
/// Employee Management directory with KPI summary cards, multi-criteria &amp; advanced filters,
/// #4DA2FF StyledDataGridView, CSV export, Add Employee modal, and Employee Details Hub navigation.
/// </summary>
public class EmployeeManagementControl : UserControl
{
    private const int PageSize = 8;

    private readonly List<EmployeeRecord> _allEmployees = EmployeeDirectoryService.CreateDefaultEmployees();

    private readonly StatCard _cardTotal;
    private readonly StatCard _cardActive;
    private readonly StatCard _cardInactive;
    private readonly StatCard _cardRecent;

    private readonly SearchInput _searchInput;
    private readonly FilterDropdown _companyFilter;
    private readonly FilterDropdown _statusFilter;

    private readonly StyledDataGridView _grid;
    private readonly EmptyState _emptyState;
    private readonly PaginationBar _pagination;

    private IReadOnlyList<EmployeeRecord> _lastFiltered = [];
    private int _currentPage = 1;

    public event EventHandler<(EmployeeRecord Employee, string DestinationKey)>? EmployeePageRequested;

    public EmployeeManagementControl()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Dock = DockStyle.Fill;
        Padding = new Padding(AppTheme.Spacing.PagePadding, 4, AppTheme.Spacing.PagePadding, 12);

        // 1. Top Action Bar (+ Add Employee & Export Employees)
        var actionBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = AppTheme.Colors.SurfaceWhite
        };

        var btnAdd = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            Text = "+ Add Employee",
            Size = new Size(168, 36),
            CornerRadius = AppTheme.Radii.Button,
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnAdd.Click += (_, _) => OpenAddEmployeeModal();

        var btnExport = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Export Employees",
            Size = new Size(180, 36),
            Font = AppTheme.Typography.CreateFont(10f, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnExport.Click += (_, _) => ExportEmployeesCsv();

        actionBar.Resize += (_, _) =>
        {
            btnExport.Location = new Point(actionBar.Width - btnExport.Width, 4);
            btnAdd.Location = new Point(btnExport.Left - 10 - btnAdd.Width, 4);
        };
        actionBar.Controls.Add(btnAdd);
        actionBar.Controls.Add(btnExport);

        // 2. 4 Summary StatCards
        var summaryRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 152,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0, 6, 0, 10),
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        summaryRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        for (int i = 0; i < 4; i++)
        {
            summaryRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }

        _cardTotal = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 9, 0), Title = "Total Employees", Subtext = "All registered staff", Icon = StatCardIcon.Employees, AccentColor = AppTheme.Colors.PrimarySolid };
        _cardActive = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(3, 0, 6, 0), Title = "Active Employees", Subtext = "Currently active", Icon = StatCardIcon.Present, AccentColor = AppTheme.Colors.StatusActiveFg };
        _cardInactive = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 3, 0), Title = "Inactive Employees", Subtext = "Inactive / Resigned / Terminated", Icon = StatCardIcon.Alert, AccentColor = AppTheme.Colors.ErrorBorder };
        _cardRecent = new StatCard { Dock = DockStyle.Fill, Margin = new Padding(9, 0, 0, 0), Title = "Recently Hired", Subtext = "Joined within last 90 days", Icon = StatCardIcon.Payroll, AccentColor = AppTheme.Colors.PrimaryDeep };

        summaryRow.Controls.Add(_cardTotal, 0, 0);
        summaryRow.Controls.Add(_cardActive, 1, 0);
        summaryRow.Controls.Add(_cardInactive, 2, 0);
        summaryRow.Controls.Add(_cardRecent, 3, 0);

        // 3. Primary Filter Bar
        var filterBar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(0, 4, 0, 8),
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        filterBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        filterBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 168f));
        filterBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 158f));

        _searchInput = new SearchInput
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0),
            PlaceholderText = "Search by employee name or ID..."
        };
        _searchInput.SearchTextChanged += (_, _) => ResetPageAndBind();

        _companyFilter = CreateFilter(["All Companies", "MCT Hotel", "MCT Corp", "MCT Tech", "MCT Logistics"], "All Companies");
        _statusFilter = CreateFilter(["All Statuses", "Active", "Inactive", "Resigned", "Terminated"], "All Statuses");
        _statusFilter.Margin = new Padding(0);

        filterBar.Controls.Add(_searchInput, 0, 0);
        filterBar.Controls.Add(_companyFilter, 1, 0);
        filterBar.Controls.Add(_statusFilter, 2, 0);

        // 5. StyledDataGridView (#4DA2FF 56px header, zero black outlines, #EDF0F5 3px row dividers)
        var gridHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(0, 6, 0, 0)
        };

        _grid = new StyledDataGridView
        {
            Dock = DockStyle.Fill,
            HeaderBackColor = AppTheme.Colors.PrimarySolid,
            HeaderForeColor = Color.White,
            ColumnHeadersHeight = AppTheme.Spacing.TableHeaderCompactHeight,
            SortIndicatorColumn = "Employee",
            ReadOnly = true
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.Colors.SurfaceWhite,
            ForeColor = AppTheme.Colors.TextPrimary,
            SelectionBackColor = AppTheme.Colors.SurfaceInput,
            SelectionForeColor = AppTheme.Colors.TextPrimary,
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular)
        };

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "EMPLOYEE ID", FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Employee", HeaderText = "EMPLOYEE", FillWeight = 42 },
            new DataGridViewTextBoxColumn { Name = "Company", HeaderText = "COMPANY", FillWeight = 22 },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "STATUS", FillWeight = 18 }
        );

        _grid.CellPainting += Grid_CellPainting;
        _grid.CellDoubleClick += (_, e) =>
        {
            // Strict guard: only open modal on valid data rows (never header or empty background)
            if (e.RowIndex >= 0 && e.RowIndex < _grid.Rows.Count && _grid.Rows[e.RowIndex].Tag is EmployeeRecord emp)
            {
                OpenDetailsHub(emp);
            }
        };

        _emptyState = new EmptyState
        {
            Title = "No employees match your filters",
            Description = "Try adjusting your search query or filters.",
            ActionText = "Reset filters",
            Visible = false
        };
        _emptyState.ActionClicked += (_, _) => ResetAllFilters();

        gridHost.Controls.Add(_grid);
        gridHost.Controls.Add(_emptyState);

        // 6. Pagination Footer
        _pagination = new PaginationBar
        {
            Dock = DockStyle.Bottom,
            TotalPages = 1,
            CurrentPage = 1
        };
        _pagination.PageChanged += (_, page) =>
        {
            _currentPage = page;
            BindRows();
        };

        Controls.Add(gridHost);
        Controls.Add(filterBar);
        Controls.Add(summaryRow);
        Controls.Add(actionBar);
        Controls.Add(_pagination);

        RefreshSummaryCards();
        BindRows();
    }

    private FilterDropdown CreateFilter(string[] items, string defaultItem)
    {
        var dd = new FilterDropdown
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 8, 0)
        };
        dd.SetItems(items, defaultItem);
        dd.SelectedItemChanged += (_, _) => ResetPageAndBind();
        return dd;
    }

    private void ResetPageAndBind()
    {
        _currentPage = 1;
        BindRows();
    }

    private void ResetAllFilters()
    {
        _searchInput.Clear();
        _companyFilter.SelectedItem = "All Companies";
        _statusFilter.SelectedItem = "All Statuses";
        ResetPageAndBind();
    }

    private void RefreshSummaryCards()
    {
        var stats = EmployeeDirectoryService.ComputeSummary(_allEmployees);
        _cardTotal.Value = stats.TotalEmployees.ToString();
        _cardActive.Value = stats.ActiveEmployees.ToString();
        _cardInactive.Value = stats.InactiveEmployees.ToString();
        _cardRecent.Value = stats.RecentlyHired.ToString();
    }

    private void BindRows()
    {
        var criteria = new EmployeeFilterCriteria(
            Query: _searchInput.Text,
            Company: _companyFilter.SelectedItem,
            Department: "All Departments",
            Status: _statusFilter.SelectedItem,
            EmploymentType: "All Types",
            PositionQuery: string.Empty,
            HiredFrom: null,
            HiredTo: null);

        _lastFiltered = EmployeeDirectoryService.Filter(_allEmployees, criteria);

        int totalPages = Math.Max(1, (int)Math.Ceiling(_lastFiltered.Count / (double)PageSize));
        _currentPage = Math.Max(1, Math.Min(_currentPage, totalPages));
        var pageRows = _lastFiltered.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

        _grid.Rows.Clear();
        foreach (var emp in pageRows)
        {
            int idx = _grid.Rows.Add(
                emp.EmployeeId,
                emp.FullName,
                emp.Company,
                emp.Status);
            _grid.Rows[idx].Tag = emp;
        }

        bool hasRows = pageRows.Count > 0;
        _grid.Visible = hasRows;
        _emptyState.Visible = !hasRows;

        _pagination.SetPageInfo(pageRows.Count, _lastFiltered.Count, _currentPage, totalPages);
    }

    private void OpenDetailsHub(EmployeeRecord emp)
    {
        using var modal = new EmployeeDetailsHubModal(emp);
        if (modal.ShowDialog(FindForm()) == DialogResult.OK && !string.IsNullOrEmpty(modal.SelectedDestination))
        {
            EmployeePageRequested?.Invoke(this, (emp, modal.SelectedDestination));
        }
    }

    private void OpenAddEmployeeModal()
    {
        string nextId = $"EMP-2026-{_allEmployees.Count + 1:D3}";
        using var modal = new AddEmployeeModal(nextId);
        if (modal.ShowDialog(FindForm()) == DialogResult.OK && modal.CreatedEmployee is { } created)
        {
            _allEmployees.Insert(0, created);
            RefreshSummaryCards();
            ResetPageAndBind();
        }
    }

    private void ExportEmployeesCsv()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            FileName = $"MCT-Employees-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (sfd.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Employee ID,Full Name,Company,Department,Position,Employment Type,Status,Date Hired,Email,Phone");
        foreach (var e in _lastFiltered)
        {
            sb.AppendLine($"\"{e.EmployeeId}\",\"{e.FullName}\",\"{e.Company}\",\"{e.Department}\",\"{e.Position}\",\"{e.EmploymentType}\",\"{e.Status}\",\"{e.DateHired:yyyy-MM-dd}\",\"{e.Email}\",\"{e.Phone}\"");
        }
        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show(FindForm(), $"Exported {_lastFiltered.Count} employee record(s).", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count || e.Graphics is null)
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].Tag is not EmployeeRecord emp)
        {
            return;
        }

        e.PaintBackground(e.CellBounds, true);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };

        string colName = _grid.Columns[e.ColumnIndex].Name;
        if (colName == "Employee")
        {
            const int avatarSize = 40;
            var avatarRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2, avatarSize, avatarSize);
            using var avatarBrush = new SolidBrush(AppTheme.Colors.SurfaceOffDay);
            e.Graphics.FillEllipse(avatarBrush, avatarRect);

            using var initFont = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold);
            using var nameFont = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
            using var subFont = AppTheme.Typography.CreateFont(9f, FontStyle.Regular);
            using var primaryBrush = new SolidBrush(AppTheme.Colors.TextPrimary);
            using var mutedBrush = new SolidBrush(AppTheme.Colors.TextMuted);

            e.Graphics.DrawString(emp.Initials, initFont, primaryBrush, avatarRect, sfCenter);
            e.Graphics.DrawString(emp.FullName, nameFont, primaryBrush, new RectangleF(avatarRect.Right + 10, e.CellBounds.Y + 12, Math.Max(40, e.CellBounds.Width - 62), 20), sfLeft);
            e.Graphics.DrawString(emp.Email, subFont, mutedBrush, new RectangleF(avatarRect.Right + 10, e.CellBounds.Y + 34, Math.Max(40, e.CellBounds.Width - 62), 18), sfLeft);
            e.Handled = true;
        }
        else if (colName == "Status")
        {
            (Color bg, Color fg) = emp.Status switch
            {
                "Active" => (AppTheme.Colors.StatusActiveBg, AppTheme.Colors.StatusActiveFg),
                "Inactive" => (AppTheme.Colors.SurfaceMuted, AppTheme.Colors.TextSecondary),
                _ => (AppTheme.Colors.StatusLateBg, AppTheme.Colors.StatusLateFg)
            };

            var pillRect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, Math.Min(90, e.CellBounds.Width - 12), 24);
            using var path = AppTheme.CreateRoundedRectPath(pillRect, 12);
            using var bgBrush = new SolidBrush(bg);
            using var fgBrush = new SolidBrush(fg);
            using var font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold);
            e.Graphics.FillPath(bgBrush, path);
            e.Graphics.DrawString(emp.Status, font, fgBrush, pillRect, sfCenter);
            e.Handled = true;
        }
        else
        {
            string val = e.FormattedValue?.ToString() ?? string.Empty;
            using var font = AppTheme.Typography.CreateFont(10f, colName == "EmployeeId" ? FontStyle.Bold : FontStyle.Regular);
            using var brush = new SolidBrush(colName == "EmployeeId" ? AppTheme.Colors.PrimaryDeep : AppTheme.Colors.TextPrimary);
            e.Graphics.DrawString(val, font, brush, new RectangleF(e.CellBounds.X + 8, e.CellBounds.Y, Math.Max(20, e.CellBounds.Width - 16), e.CellBounds.Height), sfLeft);
            e.Handled = true;
        }
    }
}
