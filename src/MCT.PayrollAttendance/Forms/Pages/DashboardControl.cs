using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Forms.Pages;

/// <summary>
/// Dashboard overview page built with reusable MCT UI components (StatCard, SectionCard, StyledDataGridView).
/// </summary>
public class DashboardControl : UserControl
{
    private sealed record CompanyMetric(string Company, int StaffCount, int TotalStaff, Color BarColor);
    private sealed record RecentScan(string Initials, string Name, string Company, string Time, string Status);

    private readonly List<CompanyMetric> _companyMetrics =
    [
        new("MCT Hotel", 48, 128, AppTheme.Colors.PrimarySolid),
        new("MCT Corp", 36, 128, AppTheme.Colors.Primary),
        new("MCT Tech", 24, 128, AppTheme.Colors.PrimaryDeep),
        new("MCT Logistics", 20, 128, AppTheme.Colors.PrimarySoft)
    ];

    private readonly List<RecentScan> _recentScans =
    [
        new("BB", "Bernadette Balagot", "MCT Hotel", "7:30 AM", "Late"),
        new("JD", "John Doe", "MCT Hotel", "7:00 AM", "Early"),
        new("MS", "Maria Santos", "MCT Corp", "8:00 AM", "Early"),
        new("AR", "Angelo Reyes", "MCT Tech", "8:15 AM", "Late"),
        new("KT", "Kevin Tan", "MCT Logistics", "6:55 AM", "Early"),
        new("LC", "Liza Cruz", "MCT Corp", "7:45 AM", "Early"),
        new("DM", "Daniel Mendoza", "MCT Hotel", "8:05 AM", "Late"),
        new("SR", "Sofia Ramos", "MCT Tech", "7:10 AM", "Early"),
        new("PN", "Paolo Navarro", "MCT Logistics", "7:50 AM", "Early"),
        new("CG", "Camille Garcia", "MCT Corp", "8:20 AM", "Late")
    ];

    public event EventHandler<string>? NavigateRequested;

    public DashboardControl()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        AutoScroll = true;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Padding = new Padding(AppTheme.Spacing.PagePadding, 8, AppTheme.Spacing.PagePadding, AppTheme.Spacing.PagePadding);

        var root = new TableLayoutPanel
        {
            Location = new Point(AppTheme.Spacing.PagePadding, 8),
            Width = 1080,
            Height = 880,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 148f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 288f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 444f));

        root.Controls.Add(BuildKpiRow(), 0, 0);
        root.Controls.Add(BuildMiddleSummaryRow(), 0, 1);
        root.Controls.Add(BuildRecentActivitySection(), 0, 2);

        Controls.Add(root);
        Resize += (_, _) =>
        {
            int scrollW = VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
            root.Width = Math.Max(640, ClientSize.Width - Padding.Horizontal - scrollW);
        };
    }

    private TableLayoutPanel BuildKpiRow()
    {
        var kpiGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 0, 0, 2),
            Margin = new Padding(0, 0, 0, AppTheme.Spacing.SectionGap),
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        for (int i = 0; i < 4; i++)
        {
            kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }
        kpiGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        kpiGrid.Controls.Add(new StatCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0),
            Title = "Total Employees",
            Value = "128",
            Subtext = "Across 4 MCT companies",
            Icon = StatCardIcon.Employees,
            AccentColor = AppTheme.Colors.PrimarySolid
        }, 0, 0);

        kpiGrid.Controls.Add(new StatCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 0, 8, 0),
            Title = "Present Today",
            Value = "116",
            Subtext = "90.6% attendance rate",
            Icon = StatCardIcon.Present,
            AccentColor = AppTheme.Colors.StatusActiveFg
        }, 1, 0);

        kpiGrid.Controls.Add(new StatCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 0, 4, 0),
            Title = "Late / Absent",
            Value = "12",
            Subtext = "8 late · 4 on leave/absent",
            Icon = StatCardIcon.Alert,
            AccentColor = AppTheme.Colors.ErrorBorder
        }, 2, 0);

        kpiGrid.Controls.Add(new StatCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(12, 0, 0, 0),
            Title = "Next Payroll Run",
            Value = "May 15",
            Subtext = "Est. ₱1,482,500 gross",
            Icon = StatCardIcon.Payroll,
            AccentColor = AppTheme.Colors.PrimaryDeep
        }, 3, 0);

        return kpiGrid;
    }

    private TableLayoutPanel BuildMiddleSummaryRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, AppTheme.Spacing.SectionGap),
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        // Left: Company Distribution Card
        var companyCard = new SectionCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 8, 0),
            Title = "Workforce by Company",
            Subtitle = "Active headcount across MCT Group"
        };

        var btnStaff = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Manage Staff",
            Size = new Size(116, 32),
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold)
        };
        btnStaff.Click += (_, _) => NavigateRequested?.Invoke(this, "Employees");
        companyCard.SetHeaderAction(btnStaff);

        var barsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        barsPanel.Paint += (_, e) => PaintCompanyDistribution(e.Graphics, barsPanel.ClientRectangle);
        barsPanel.Resize += (_, _) => barsPanel.Invalidate();
        companyCard.BodyHost.Controls.Add(barsPanel);

        // Right: Payroll Cycle Card
        var payrollCard = new SectionCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 0, 0, 0),
            Title = "Payroll Cycle Overview",
            Subtitle = "May 1 – May 15, 2025 cutoff"
        };

        var statusPill = new StatusPill
        {
            Kind = StatusPillKind.Active,
            Text = "On Track",
            Size = new Size(88, 26)
        };
        payrollCard.SetHeaderAction(statusPill);

        var cycleLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        cycleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
        cycleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
        cycleLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        cycleLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        cycleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        AddCycleRow(cycleLayout, 0, "RFID Logs Validated", "116 / 128 employees");
        AddCycleRow(cycleLayout, 1, "Pending Approvals", "3 overtime requests");

        var actionFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 4, 0, 0),
            BackColor = Color.Transparent
        };

        var btnAttendance = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            Text = "Review Attendance",
            Size = new Size(185, 34),
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            CornerRadius = AppTheme.Radii.Button
        };
        btnAttendance.Click += (_, _) => NavigateRequested?.Invoke(this, "Attendance");

        var btnPayroll = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Open Payroll",
            Size = new Size(132, 34),
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            Margin = new Padding(8, 0, 0, 0)
        };
        btnPayroll.Click += (_, _) => NavigateRequested?.Invoke(this, "Payroll");

        actionFlow.Controls.Add(btnAttendance);
        actionFlow.Controls.Add(btnPayroll);
        cycleLayout.Controls.Add(actionFlow, 0, 2);
        cycleLayout.SetColumnSpan(actionFlow, 2);

        payrollCard.BodyHost.Controls.Add(cycleLayout);

        row.Controls.Add(companyCard, 0, 0);
        row.Controls.Add(payrollCard, 1, 0);
        return row;
    }

    private SectionCard BuildRecentActivitySection()
    {
        var card = new SectionCard
        {
            Dock = DockStyle.Fill,
            Title = "Recent Attendance Scans",
            Subtitle = "Latest RFID check-ins across all branches"
        };

        var btnViewAll = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "View All →",
            Size = new Size(104, 32),
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold)
        };
        btnViewAll.Click += (_, _) => NavigateRequested?.Invoke(this, "Attendance");
        card.SetHeaderAction(btnViewAll);

        var grid = new StyledDataGridView
        {
            Dock = DockStyle.Fill,
            HeaderBackColor = AppTheme.Colors.PrimarySolid,
            HeaderForeColor = Color.White,
            ColumnHeadersHeight = 44,
            ScrollBars = ScrollBars.None,
            ReadOnly = true
        };
        grid.RowTemplate.Height = 48;

        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Employee", HeaderText = "EMPLOYEE", FillWeight = 34 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Company", HeaderText = "COMPANY", FillWeight = 24 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "CLOCK IN", FillWeight = 22 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "STATUS", FillWeight = 20 });

        const int pageSize = 5;
        int currentPage = 1;
        int totalPages = Math.Max(1, (int)Math.Ceiling(_recentScans.Count / (double)pageSize));

        var pagination = new PaginationBar
        {
            Dock = DockStyle.Bottom,
            Padding = new Padding(0, 6, 0, 0),
            TotalPages = totalPages,
            CurrentPage = currentPage
        };

        void BindScans()
        {
            var pageRows = _recentScans.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            grid.Rows.Clear();
            foreach (var scan in pageRows)
            {
                int idx = grid.Rows.Add(scan.Name, scan.Company, scan.Time, scan.Status);
                grid.Rows[idx].Tag = scan;
            }
            grid.ClearSelection();
            pagination.SetPageInfo(pageRows.Count, _recentScans.Count, currentPage, totalPages, "recent scans");
        }

        pagination.PageChanged += (_, page) =>
        {
            currentPage = page;
            BindScans();
        };

        BindScans();
        grid.HandleCreated += (_, _) => grid.ClearSelection();

        grid.CellPainting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.Graphics is null || grid.Rows[e.RowIndex].Tag is not RecentScan scan)
            {
                return;
            }

            using var cellBgBrush = new SolidBrush(AppTheme.Colors.SurfaceWhite);
            e.Graphics.FillRectangle(cellBgBrush, e.CellBounds);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
            using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };

            string col = grid.Columns[e.ColumnIndex].Name;
            if (col == "Employee")
            {
                const int avatarSize = 32;
                var avatarRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - avatarSize) / 2, avatarSize, avatarSize);
                using var avatarBrush = new SolidBrush(AppTheme.Colors.SurfaceOffDay);
                e.Graphics.FillEllipse(avatarBrush, avatarRect);

                using var initFont = AppTheme.Typography.CreateFont(9f, FontStyle.Bold);
                using var nameFont = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
                using var primaryBrush = new SolidBrush(AppTheme.Colors.TextPrimary);

                e.Graphics.DrawString(scan.Initials, initFont, primaryBrush, avatarRect, sfCenter);
                e.Graphics.DrawString(scan.Name, nameFont, primaryBrush, new RectangleF(avatarRect.Right + 10, e.CellBounds.Y, Math.Max(40, e.CellBounds.Width - 56), e.CellBounds.Height), sfLeft);
                e.Handled = true;
            }
            else if (col == "Status")
            {
                bool isLate = scan.Status.Equals("Late", StringComparison.OrdinalIgnoreCase);
                Color bg = isLate ? AppTheme.Colors.StatusLateBg : AppTheme.Colors.StatusEarlyBg;
                Color fg = isLate ? AppTheme.Colors.StatusLateFg : AppTheme.Colors.StatusEarlyFg;
                var pillRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, 76, 24);
                using var path = AppTheme.CreateRoundedRectPath(pillRect, 12);
                using var brush = new SolidBrush(bg);
                using var fgBrush = new SolidBrush(fg);
                using var font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold);
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawString(scan.Status, font, fgBrush, pillRect, sfCenter);
                e.Handled = true;
            }
            else
            {
                string val = e.FormattedValue?.ToString() ?? string.Empty;
                using var font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
                using var brush = new SolidBrush(AppTheme.Colors.TextPrimary);
                e.Graphics.DrawString(val, font, brush, new RectangleF(e.CellBounds.X + 8, e.CellBounds.Y, Math.Max(20, e.CellBounds.Width - 16), e.CellBounds.Height), sfLeft);
                e.Handled = true;
            }
        };

        card.BodyHost.Controls.Add(grid);
        card.BodyHost.Controls.Add(pagination);
        return card;
    }

    private void PaintCompanyDistribution(Graphics g, Rectangle bounds)
    {
        if (bounds.Width <= 40 || bounds.Height <= 20)
        {
            return;
        }

        g.SmoothingMode = SmoothingMode.AntiAlias;
        int rowHeight = Math.Max(28, (bounds.Height - 8) / _companyMetrics.Count);

        using var labelFont = AppTheme.Typography.CreateFont(10f, FontStyle.Bold);
        using var countFont = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular);
        using var labelBrush = new SolidBrush(AppTheme.Colors.TextPrimary);
        using var mutedBrush = new SolidBrush(AppTheme.Colors.TextSecondary);
        using var trackBrush = new SolidBrush(AppTheme.Colors.SurfaceMuted);

        for (int i = 0; i < _companyMetrics.Count; i++)
        {
            var m = _companyMetrics[i];
            int y = bounds.Y + 4 + i * rowHeight;

            g.DrawString(m.Company, labelFont, labelBrush, bounds.X, y);
            string countText = $"{m.StaffCount} staff";
            SizeF countSize = g.MeasureString(countText, countFont);
            g.DrawString(countText, countFont, mutedBrush, bounds.Right - countSize.Width, y);

            int barY = y + 20;
            int barWidth = Math.Max(40, bounds.Width - 4);
            var trackRect = new Rectangle(bounds.X, barY, barWidth, 8);
            using var trackPath = AppTheme.CreateRoundedRectPath(trackRect, 4);
            g.FillPath(trackBrush, trackPath);

            int fillWidth = Math.Max(12, (int)(barWidth * (m.StaffCount / (float)m.TotalStaff)));
            var fillRect = new Rectangle(bounds.X, barY, fillWidth, 8);
            using var fillPath = AppTheme.CreateRoundedRectPath(fillRect, 4);
            using var fillBrush = new SolidBrush(m.BarColor);
            g.FillPath(fillBrush, fillPath);
        }
    }

    private static void AddCycleRow(TableLayoutPanel layout, int row, string label, string value)
    {
        var lbl = new Label
        {
            Text = label,
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var val = new Label
        {
            Text = value,
            Font = AppTheme.Typography.BodyMedium(),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        layout.Controls.Add(lbl, 0, row);
        layout.Controls.Add(val, 1, row);
    }
}
