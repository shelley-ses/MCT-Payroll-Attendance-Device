using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Forms.Modals;

namespace MCT.PayrollAttendance.Forms.Pages;

/// <summary>
/// Attendance &amp; Timekeeping page matching Figma FINAL UI (445:2072 — Attendance &amp; Timekeeping).
/// </summary>
public class AttendanceTimekeepingControl : UserControl
{
    private sealed record AttendanceRow(
        string Initials,
        string Name,
        string Role,
        string Company,
        string StartDate,
        string ClockWindowAm,
        string ClockWindowPm,
        string Overtime,
        string Location,
        string Status);

    private const int PageSize = 10;

    private readonly List<AttendanceRow> _allRecords =
    [
        new("BB", "Bernadette Balagot", "Supervisor", "MCT Hotel", "1st May 2025", "7:30 AM - 12:00 PM", "1:00 PM - 3:30 PM", "2hrs 30 min", "Withston Street, Ware...", "Late"),
        new("JD", "John Doe", "Supervisor", "MCT Hotel", "1st May 2025", "7:00 AM - 12:00 PM", "1:00 PM - 3:30 PM", "2hrs 30 min", "Withston Street, Ware...", "Early"),
        new("MS", "Maria Santos", "HR Specialist", "MCT Corp", "1st May 2025", "8:00 AM - 12:00 PM", "1:00 PM - 5:00 PM", "-", "HQ Building, Floor 3", "Early"),
        new("AR", "Angelo Reyes", "IT Administrator", "MCT Tech", "1st May 2025", "8:15 AM - 12:00 PM", "1:00 PM - 5:15 PM", "30 mins", "Server Wing, HQ", "Late"),
        new("LC", "Liza Cruz", "Payroll Officer", "MCT Corp", "1st May 2025", "7:45 AM - 12:00 PM", "1:00 PM - 4:45 PM", "-", "Finance Office, HQ", "Early"),
        new("KT", "Kevin Tan", "Warehouse Lead", "MCT Logistics", "1st May 2025", "6:55 AM - 12:00 PM", "1:00 PM - 3:55 PM", "1hr 15 min", "Depot B, Logistics", "Early"),
        new("RG", "Rafael Garcia", "Concierge", "MCT Hotel", "1st May 2025", "7:35 AM - 12:00 PM", "1:00 PM - 4:35 PM", "-", "Lobby Desk, Hotel", "Late"),
        new("CM", "Camille Mendoza", "Accountant", "MCT Corp", "1st May 2025", "7:50 AM - 12:00 PM", "1:00 PM - 5:20 PM", "30 mins", "Finance Office, HQ", "Early"),
        new("DL", "Daniel Lim", "Systems Engineer", "MCT Tech", "1st May 2025", "8:10 AM - 12:00 PM", "1:00 PM - 6:10 PM", "1hr 00 min", "Tech Hub, Floor 2", "Late"),
        new("EV", "Elena Villanueva", "Fleet Coordinator", "MCT Logistics", "1st May 2025", "6:50 AM - 12:00 PM", "1:00 PM - 4:50 PM", "1hr 00 min", "Dispatch Bay 1", "Early")
    ];

    private readonly SearchInput _searchInput;
    private readonly FilterDropdown _companyFilter;
    private readonly StyledDataGridView _grid;
    private readonly EmptyState _emptyState;
    private readonly PaginationBar _pagination;
    private string _selectedCompany = "All Companies";
    private int _currentPage = 1;

    public AttendanceTimekeepingControl()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Dock = DockStyle.Fill;
        Padding = new Padding(0, 8, 0, 12);

        // Toolbar (SearchInput + 164x40 FilterDropdown)
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(AppTheme.Spacing.PagePadding, 6, AppTheme.Spacing.PagePadding, 12)
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 164f + AppTheme.Spacing.PagePadding));

        _searchInput = new SearchInput
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 14, 0),
            PlaceholderText = "Search for employee, location, or status..."
        };
        _searchInput.SearchTextChanged += (_, _) =>
        {
            _currentPage = 1;
            BindRows();
        };

        _companyFilter = new FilterDropdown
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        _companyFilter.SetItems(["All Companies", "MCT Hotel", "MCT Corp", "MCT Tech", "MCT Logistics"], "All Companies");
        _companyFilter.SelectedItemChanged += (_, company) =>
        {
            _selectedCompany = company;
            _currentPage = 1;
            BindRows();
        };

        toolbar.Controls.Add(_searchInput, 0, 0);
        toolbar.Controls.Add(_companyFilter, 1, 0);

        // Table + EmptyState host
        var gridHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Colors.SurfaceWhite
        };

        // StyledDataGridView (#4DA2FF 56px header per Figma 445:2072)
        _grid = new StyledDataGridView
        {
            Dock = DockStyle.Fill,
            HeaderBackColor = AppTheme.Colors.PrimarySolid,
            HeaderForeColor = Color.White,
            ColumnHeadersHeight = AppTheme.Spacing.TableHeaderCompactHeight,
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

        var colName = new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "NAME", FillWeight = 20 };
        var colDate = new DataGridViewTextBoxColumn { Name = "StartDate", HeaderText = "START DATE", FillWeight = 11 };
        var colClockAm = new DataGridViewTextBoxColumn { Name = "ClockWindowAm", HeaderText = "CLOCK IN - CLOCK OUT (AM)", FillWeight = 16 };
        var colClockPm = new DataGridViewTextBoxColumn { Name = "ClockWindowPm", HeaderText = "CLOCK IN - CLOCK OUT (PM)", FillWeight = 16 };
        var colOvertime = new DataGridViewTextBoxColumn { Name = "Overtime", HeaderText = "OVERTIME", FillWeight = 10 };
        var colLocation = new DataGridViewTextBoxColumn { Name = "Location", HeaderText = "LOCATION", FillWeight = 13 };
        var colStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "STATUS", FillWeight = 9 };
        var colAction = new DataGridViewTextBoxColumn { Name = "Action", HeaderText = "ACTION", FillWeight = 5 };
        foreach (var col in new[] { colDate, colClockAm, colClockPm, colOvertime, colLocation, colStatus, colAction })
        {
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        _grid.HandleCreated += (_, _) => _grid.ClearSelection();

        _grid.Columns.AddRange(colName, colDate, colClockAm, colClockPm, colOvertime, colLocation, colStatus, colAction);
        _grid.CellPainting += Grid_CellPainting;
        _grid.CellClick += Grid_CellClick;
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].Tag is AttendanceRow rec)
            {
                OpenViewDetails(rec);
            }
        };

        _emptyState = new EmptyState
        {
            Title = "No attendance records found",
            Description = "Try clearing your search query or switching the company filter.",
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
            Padding = new Padding(AppTheme.Spacing.PagePadding, 8, AppTheme.Spacing.PagePadding, 0),
            TotalPages = 2,
            CurrentPage = 1
        };
        _pagination.PageChanged += (_, page) =>
        {
            _currentPage = page;
            BindRows();
        };

        Controls.Add(gridHost);
        Controls.Add(toolbar);
        Controls.Add(_pagination);

        BindRows();
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Rows[e.RowIndex].Tag is not AttendanceRow rec)
        {
            return;
        }

        if (_grid.Columns[e.ColumnIndex].Name == "Action")
        {
            var menu = new ContextMenuStrip
            {
                Renderer = new MctMenuRenderer(),
                ShowImageMargin = false,
                BackColor = AppTheme.Colors.SurfaceWhite
            };
            menu.Items.Add("View Employee Profile", null, (_, _) => OpenViewDetails(rec));
            var cellRect = _grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            menu.Show(_grid, new Point(cellRect.Left, cellRect.Bottom));
        }
    }

    private void OpenViewDetails(AttendanceRow rec)
    {
        using var modal = new ViewDetailsModal(rec.Initials, rec.Name, rec.Company);
        modal.ShowDialog(FindForm());
    }

    private void BindRows()
    {
        string query = _searchInput.Text.Trim();

        var filtered = _allRecords.Where(r =>
            (_selectedCompany == "All Companies" || r.Company == _selectedCompany) &&
            (string.IsNullOrEmpty(query) ||
             r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
             r.Location.Contains(query, StringComparison.OrdinalIgnoreCase) ||
             r.Status.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();

        int totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
        _currentPage = Math.Max(1, Math.Min(_currentPage, totalPages));

        var pageRows = filtered.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

        _grid.Rows.Clear();
        foreach (var rec in pageRows)
        {
            int idx = _grid.Rows.Add(rec.Name, rec.StartDate, rec.ClockWindowAm, rec.ClockWindowPm, rec.Overtime, rec.Location, rec.Status, "⋮");
            _grid.Rows[idx].Tag = rec;
        }

        bool hasRows = pageRows.Count > 0;
        _grid.Visible = hasRows;
        _emptyState.Visible = !hasRows;

        _pagination.SetPageInfo(pageRows.Count, filtered.Count, _currentPage, totalPages);
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count || e.Graphics is null)
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].Tag is not AttendanceRow rec)
        {
            return;
        }

        using var cellBgBrush = new SolidBrush(AppTheme.Colors.SurfaceWhite);
        e.Graphics.FillRectangle(cellBgBrush, e.CellBounds);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };

        string colName = _grid.Columns[e.ColumnIndex].Name;
        if (colName == "Name")
        {
            const int avatarSize = 44;
            var avatarRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2, avatarSize, avatarSize);
            using var avatarBrush = new SolidBrush(AppTheme.Colors.SurfaceOffDay);
            e.Graphics.FillEllipse(avatarBrush, avatarRect);

            using var initFont = AppTheme.Typography.CreateFont(10f, FontStyle.Bold);
            using var nameFont = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
            using var roleFont = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular);
            using var primaryBrush = new SolidBrush(AppTheme.Colors.TextPrimary);
            using var mutedBrush = new SolidBrush(AppTheme.Colors.TextMuted);

            e.Graphics.DrawString(rec.Initials, initFont, primaryBrush, avatarRect, sfCenter);
            e.Graphics.DrawString(rec.Name, nameFont, primaryBrush, new RectangleF(avatarRect.Right + 10, e.CellBounds.Y + 12, Math.Max(40, e.CellBounds.Width - 68), 22), sfLeft);
            e.Graphics.DrawString(rec.Role, roleFont, mutedBrush, new RectangleF(avatarRect.Right + 10, e.CellBounds.Y + 34, Math.Max(40, e.CellBounds.Width - 68), 18), sfLeft);
            e.Handled = true;
        }
        else if (colName == "Overtime" && rec.Overtime != "-")
        {
            int chipW = Math.Min(96, e.CellBounds.Width - 12);
            var chipRect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - chipW) / 2, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, chipW, 24);
            using var path = AppTheme.CreateRoundedRectPath(chipRect, AppTheme.Radii.Chip);
            using var brush = new SolidBrush(AppTheme.Colors.SurfaceMuted);
            using var fgBrush = new SolidBrush(AppTheme.Colors.TextPrimary);
            using var font = AppTheme.Typography.CreateFont(10f, FontStyle.Regular);
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawString(rec.Overtime, font, fgBrush, chipRect, sfCenter);
            e.Handled = true;
        }
        else if (colName == "Status")
        {
            bool isLate = rec.Status.Equals("Late", StringComparison.OrdinalIgnoreCase);
            Color bg = isLate ? AppTheme.Colors.StatusLateBg : AppTheme.Colors.StatusEarlyBg;
            Color fg = isLate ? AppTheme.Colors.StatusLateFg : AppTheme.Colors.StatusEarlyFg;

            int pillW = Math.Min(88, e.CellBounds.Width - 12);
            var pillRect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - pillW) / 2, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, pillW, 24);
            using var path = AppTheme.CreateRoundedRectPath(pillRect, 12);
            using var brush = new SolidBrush(bg);
            using var fgBrush = new SolidBrush(fg);
            using var font = AppTheme.Typography.CreateFont(10f, FontStyle.Bold);
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawString(rec.Status, font, fgBrush, pillRect, sfCenter);
            e.Handled = true;
        }
        else if (colName == "Action")
        {
            using var dotBrush = new SolidBrush(AppTheme.Colors.TextSecondary);
            float dotSize = 3.5f;
            float cx = e.CellBounds.X + (e.CellBounds.Width - dotSize) / 2f;
            float cy = e.CellBounds.Y + e.CellBounds.Height / 2f;
            e.Graphics.FillEllipse(dotBrush, cx, cy - 7f, dotSize, dotSize);
            e.Graphics.FillEllipse(dotBrush, cx, cy - 1.75f, dotSize, dotSize);
            e.Graphics.FillEllipse(dotBrush, cx, cy + 3.5f, dotSize, dotSize);
            e.Handled = true;
        }
        else
        {
            string val = e.FormattedValue?.ToString() ?? string.Empty;
            using var font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
            using var brush = new SolidBrush(AppTheme.Colors.TextPrimary);
            StringFormat fmt = colName == "Location" ? sfLeft : sfCenter;
            e.Graphics.DrawString(val, font, brush, new RectangleF(e.CellBounds.X + 8, e.CellBounds.Y, Math.Max(20, e.CellBounds.Width - 16), e.CellBounds.Height), fmt);
            e.Handled = true;
        }
    }
}
