using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Text;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Forms.Pages;

/// <summary>
/// Dedicated Employee Profile host preserving selected EmployeeRecord context across Overview, Attendance, Payroll &amp; Deductions, and Payslips.
/// </summary>
public class EmployeeProfileHostControl : UserControl
{
    private readonly EmployeeRecord _employee;
    private readonly Panel _tabBodyHost;
    private readonly Dictionary<string, AppButton> _tabButtons = new();
    private string _activeTab;
    private bool _showFullAttendanceHistory;
    private string _selectedPayslipPeriod = "All Periods";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool CanViewSalary { get; set; } = true;
    public event EventHandler? BackRequested;

    public EmployeeProfileHostControl(EmployeeRecord employee, string initialTab = "Overview")
    {
        _employee = employee;
        _activeTab = initialTab;

        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Padding = new Padding(AppTheme.Spacing.PagePadding, 4, AppTheme.Spacing.PagePadding, AppTheme.Spacing.PagePadding);

        // Top Bar: Back Button + Employee Header + Tab Strip
        var topContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 162,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        topContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
        topContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
        topContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));

        // Row 0: Back to Employees
        var backRow = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var btnBack = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "← Back to Employees",
            Size = new Size(196, 34),
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold),
            Location = new Point(0, 2)
        };
        btnBack.Click += (_, _) => BackRequested?.Invoke(this, EventArgs.Empty);
        backRow.Controls.Add(btnBack);

        // Row 1: Employee Identity Strip
        var idRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        idRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58f));
        idRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        idRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));

        var avatar = new AvatarBadge
        {
            Initials = _employee.Initials,
            ShowOnlineIndicator = false,
            Size = new Size(48, 48),
            Margin = new Padding(0, 12, 0, 0)
        };

        var infoFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(4, 4, 0, 0),
            BackColor = Color.Transparent
        };
        infoFlow.Controls.Add(new Label
        {
            Text = _employee.FullName,
            Font = AppTheme.Typography.CreateFont(15.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            AutoSize = true,
            Margin = new Padding(0)
        });
        infoFlow.Controls.Add(new Label
        {
            Text = $"{_employee.EmployeeId}  ·  {_employee.Position}  ·  {_employee.Department} ({_employee.Company})",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            AutoSize = true,
            Margin = new Padding(0, 2, 0, 0)
        });

        var pill = new StatusPill
        {
            Kind = _employee.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? StatusPillKind.Active : StatusPillKind.Late,
            Text = _employee.Status,
            Size = new Size(92, 26),
            Margin = new Padding(8, 22, 0, 0)
        };

        idRow.Controls.Add(avatar, 0, 0);
        idRow.Controls.Add(infoFlow, 1, 0);
        idRow.Controls.Add(pill, 2, 0);

        // Row 2: 4-Tab Bar
        var tabBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 4),
            BackColor = Color.Transparent
        };

        var tabFont = AppTheme.Typography.CreateFont(10f, FontStyle.Bold);
        foreach (var (key, label) in new (string, string)[]
        {
            ("Overview", "01. Overview"),
            ("Attendance", "02. Attendance"),
            ("Payroll", "03. Payroll & Deductions"),
            ("Payslips", "04. Payslips")
        })
        {
            int tabWidth = Math.Max(148, TextRenderer.MeasureText(label, tabFont).Width + 36);
            var tabBtn = new AppButton
            {
                Text = label,
                Size = new Size(tabWidth, 36),
                Margin = new Padding(0, 0, 10, 0),
                CornerRadius = AppTheme.Radii.Button,
                Font = tabFont
            };
            tabBtn.Click += (_, _) => SelectTab(key);
            _tabButtons[key] = tabBtn;
            tabBar.Controls.Add(tabBtn);
        }

        topContainer.Controls.Add(backRow, 0, 0);
        topContainer.Controls.Add(idRow, 0, 1);
        topContainer.Controls.Add(tabBar, 0, 2);

        _tabBodyHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(0, 8, 0, 0)
        };

        Controls.Add(_tabBodyHost);
        Controls.Add(topContainer);

        SelectTab(_activeTab);
    }

    public void SelectTab(string tabKey)
    {
        _activeTab = tabKey;
        foreach (var (key, btn) in _tabButtons)
        {
            btn.Variant = key == tabKey ? AppButtonVariant.CompactAction : AppButtonVariant.SecondaryOutline;
        }

        _tabBodyHost.Controls.Clear();
        Control page = tabKey switch
        {
            "Attendance" => BuildAttendancePage(),
            "Payroll" => BuildPayrollPage(),
            "Payslips" => BuildPayslipsPage(),
            _ => BuildOverviewPage()
        };
        _tabBodyHost.Controls.Add(page);
    }

    // 01. EMPLOYEE OVERVIEW
    private Control BuildOverviewPage()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        // Personal & Employment Info Card
        var infoCard = new SectionCard
        {
            Dock = DockStyle.Fill,
            Height = 308,
            Title = "Personal & Employment Details",
            Margin = new Padding(0, 0, 8, 14)
        };
        var infoGrid = CreateKeyValueGrid([
            ("Employee ID", _employee.EmployeeId),
            ("Company", _employee.Company),
            ("Department", _employee.Department),
            ("Position", _employee.Position),
            ("Employment Type", _employee.EmploymentType),
            ("Date Hired", _employee.DateHired.ToString("MMMM d, yyyy")),
            ("Email", _employee.Email),
            ("Contact", _employee.Phone)
        ]);
        infoCard.BodyHost.Controls.Add(infoGrid);

        // Salary Summary Card
        decimal gross = _employee.BasicSalary + _employee.Allowances;
        decimal deductions = Math.Round(gross * 0.145m, 2);
        decimal net = gross - deductions;

        var salaryCard = new SectionCard
        {
            Dock = DockStyle.Fill,
            Height = 308,
            Title = "Salary & Recent Payroll Summary",
            Margin = new Padding(8, 0, 0, 14)
        };
        var salaryGrid = CreateKeyValueGrid(CanViewSalary
            ? [
                ("Basic Salary", $"₱{_employee.BasicSalary:N2}"),
                ("Allowances", $"₱{_employee.Allowances:N2}"),
                ("Gross Pay", $"₱{gross:N2}"),
                ("Est. Deductions", $"₱{deductions:N2}"),
                ("Est. Net Pay", $"₱{net:N2}"),
                ("Last Payroll Run", "May 1 – May 15, 2025 (Released)")
            ]
            : [
                ("Salary Access", "Restricted — Payroll/Admin permission required")
            ]);
        salaryCard.BodyHost.Controls.Add(salaryGrid);

        // Weekly Attendance Summary Card
        var weeklyCard = new SectionCard
        {
            Dock = DockStyle.Top,
            Height = 176,
            Title = "Weekly Attendance Summary",
            Subtitle = "Scheduled working days & current week hours",
            Margin = new Padding(0)
        };

        var weeklyPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        weeklyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
        weeklyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));

        var strip = new WorkingDaysStrip
        {
            DisplayMode = WorkingDaysDisplayMode.ExpandedChips,
            WorkingDays = _employee.WorkingDays,
            Margin = new Padding(0, 12, 0, 0)
        };

        var hoursSummary = new Label
        {
            Text = $"Scheduled Days: {_employee.WorkingDays.Count(d => d)} days / week\nCurrent Week Logged: 38.5 hrs  (2.5 hrs OT)",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };

        weeklyPanel.Controls.Add(strip, 0, 0);
        weeklyPanel.Controls.Add(hoursSummary, 1, 0);
        weeklyCard.BodyHost.Controls.Add(weeklyPanel);

        layout.Controls.Add(infoCard, 0, 0);
        layout.Controls.Add(salaryCard, 1, 0);
        layout.Controls.Add(weeklyCard, 0, 1);
        layout.SetColumnSpan(weeklyCard, 2);
        return layout;
    }

    // 02. EMPLOYEE ATTENDANCE
    private Control BuildAttendancePage()
    {
        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 148f));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        // KPI Row
        var kpis = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
        kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        for (int i = 0; i < 4; i++)
        {
            kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }

        kpis.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 9, 0), Title = "Days Worked", Value = _showFullAttendanceHistory ? "21" : "5", Subtext = _showFullAttendanceHistory ? "Current month" : "Current week", Icon = StatCardIcon.Present, AccentColor = AppTheme.Colors.StatusActiveFg }, 0, 0);
        kpis.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(3, 0, 6, 0), Title = "Total Hours", Value = _showFullAttendanceHistory ? "168.5h" : "40.5h", Subtext = "Regular + Overtime", Icon = StatCardIcon.Employees, AccentColor = AppTheme.Colors.PrimarySolid }, 1, 0);
        kpis.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 3, 0), Title = "Overtime / Undertime", Value = _showFullAttendanceHistory ? "6.5h / 0h" : "2.5h / 0h", Subtext = "Approved OT", Icon = StatCardIcon.Payroll, AccentColor = AppTheme.Colors.PrimaryDeep }, 2, 0);
        kpis.Controls.Add(new StatCard { Dock = DockStyle.Fill, Margin = new Padding(9, 0, 0, 0), Title = "Late / Absences", Value = _showFullAttendanceHistory ? "2 / 0" : "1 / 0", Subtext = "Attendance exceptions", Icon = StatCardIcon.Alert, AccentColor = AppTheme.Colors.ErrorBorder }, 3, 0);

        // Toggle Bar
        var toggleBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var lblScope = new Label
        {
            Text = _showFullAttendanceHistory ? "Showing Full Attendance History (May 2025)" : "Showing Current Week Attendance (May 12 – May 16, 2025)",
            Font = AppTheme.Typography.SubHeading(),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(0, 10)
        };
        string toggleText = _showFullAttendanceHistory ? "Show Current Week Only" : "View Full Attendance History";
        var toggleFont = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold);
        int toggleWidth = Math.Max(240, TextRenderer.MeasureText(toggleText, toggleFont).Width + 36);
        var btnToggle = new AppButton
        {
            Variant = _showFullAttendanceHistory ? AppButtonVariant.CompactAction : AppButtonVariant.SecondaryOutline,
            Text = toggleText,
            Font = toggleFont,
            Size = new Size(toggleWidth, 36),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(Math.Max(260, toggleBar.Width - toggleWidth), 4)
        };
        btnToggle.Click += (_, _) =>
        {
            _showFullAttendanceHistory = !_showFullAttendanceHistory;
            SelectTab("Attendance");
        };
        toggleBar.Resize += (_, _) => btnToggle.Location = new Point(Math.Max(260, toggleBar.Width - btnToggle.Width), 4);
        toggleBar.Controls.Add(lblScope);
        toggleBar.Controls.Add(btnToggle);

        // Daily Time Records Grid
        var grid = new StyledDataGridView
        {
            Dock = DockStyle.Fill,
            HeaderBackColor = AppTheme.Colors.PrimarySolid,
            HeaderForeColor = Color.White,
            ColumnHeadersHeight = 48,
            ReadOnly = true
        };
        grid.RowTemplate.Height = 52;
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "DATE", FillWeight = 22 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ClockIn", HeaderText = "CLOCK IN", FillWeight = 16 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ClockOut", HeaderText = "CLOCK OUT", FillWeight = 16 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hours", HeaderText = "HOURS", FillWeight = 14 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Overtime", HeaderText = "OVERTIME", FillWeight = 16 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "STATUS", FillWeight = 16 });

        var rows = new (string Date, string In, string Out, string Hrs, string Ot, string Status)[]
        {
            ("Mon, May 12", "7:55 AM", "5:00 PM", "8.0 hrs", "-", "Early"),
            ("Tue, May 13", "8:12 AM", "5:30 PM", "8.0 hrs", "30 mins", "Late"),
            ("Wed, May 14", "7:50 AM", "6:00 PM", "9.0 hrs", "1hr 00 min", "Early"),
            ("Thu, May 15", "7:58 AM", "5:00 PM", "8.0 hrs", "-", "Early"),
            ("Fri, May 16", "8:00 AM", "6:00 PM", "9.0 hrs", "1hr 00 min", "Early")
        };

        foreach (var r in rows)
        {
            grid.Rows.Add(r.Date, r.In, r.Out, r.Hrs, r.Ot, r.Status);
        }

        if (_showFullAttendanceHistory)
        {
            grid.Rows.Add("Fri, May 9", "7:52 AM", "5:00 PM", "8.0 hrs", "-", "Early");
            grid.Rows.Add("Thu, May 8", "8:09 AM", "6:00 PM", "8.5 hrs", "1hr 00 min", "Late");
            grid.Rows.Add("Wed, May 7", "7:48 AM", "5:00 PM", "8.0 hrs", "-", "Early");
        }

        container.Controls.Add(kpis, 0, 0);
        container.Controls.Add(toggleBar, 0, 1);
        container.Controls.Add(grid, 0, 2);
        return container;
    }

    // 03. PAYROLL & DEDUCTIONS
    private Control BuildPayrollPage()
    {
        if (!CanViewSalary)
        {
            return new EmptyState
            {
                Title = "Salary Information Restricted",
                Description = "Your current role does not have permission to view salary and deduction details.",
                ActionText = string.Empty
            };
        }

        decimal gross = _employee.BasicSalary + _employee.Allowances;
        decimal sss = Math.Round(_employee.BasicSalary * 0.045m, 2);
        decimal philHealth = Math.Round(_employee.BasicSalary * 0.025m, 2);
        decimal pagIbig = 200m;
        decimal tax = Math.Round(Math.Max(0m, gross - 20833m) * 0.15m, 2);
        decimal loans = 1500m;
        decimal totalDeductions = sss + philHealth + pagIbig + tax + loans;
        decimal net = gross - totalDeductions;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 268f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var topCards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 14)
        };
        topCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        topCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        var earningsCard = new SectionCard { Dock = DockStyle.Fill, Title = "Compensation Breakdown", Margin = new Padding(0, 0, 8, 0) };
        earningsCard.BodyHost.Controls.Add(CreateKeyValueGrid([
            ("Basic Salary", $"₱{_employee.BasicSalary:N2}"),
            ("Allowances", $"₱{_employee.Allowances:N2}"),
            ("Gross Monthly Salary", $"₱{gross:N2}"),
            ("Estimated Net Salary", $"₱{net:N2}")
        ]));

        var deductionsCard = new SectionCard { Dock = DockStyle.Fill, Title = "Statutory & Other Deductions", Margin = new Padding(8, 0, 0, 0) };
        deductionsCard.BodyHost.Controls.Add(CreateKeyValueGrid([
            ("SSS Contribution", $"₱{sss:N2}"),
            ("PhilHealth", $"₱{philHealth:N2}"),
            ("Pag-IBIG Fund", $"₱{pagIbig:N2}"),
            ("Withholding Tax", $"₱{tax:N2}"),
            ("Loans & Cash Advances", $"₱{loans:N2}"),
            ("Total Deductions", $"₱{totalDeductions:N2}")
        ]));

        topCards.Controls.Add(earningsCard, 0, 0);
        topCards.Controls.Add(deductionsCard, 1, 0);

        var historyCard = new SectionCard { Dock = DockStyle.Fill, Title = "Payroll History" };
        var grid = new StyledDataGridView
        {
            Dock = DockStyle.Fill,
            HeaderBackColor = AppTheme.Colors.SurfaceMuted,
            HeaderForeColor = AppTheme.Colors.TextSecondary,
            ColumnHeadersHeight = 46,
            ReadOnly = true
        };
        grid.RowTemplate.Height = 50;
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Period", HeaderText = "PAY PERIOD", FillWeight = 28 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Gross", HeaderText = "GROSS PAY", FillWeight = 20 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Deductions", HeaderText = "DEDUCTIONS", FillWeight = 20 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Net", HeaderText = "NET PAY", FillWeight = 20 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "STATUS", FillWeight = 12 });

        decimal halfGross = gross / 2m;
        decimal halfDed = totalDeductions / 2m;
        decimal halfNet = net / 2m;
        grid.Rows.Add("May 1 – May 15, 2025", $"₱{halfGross:N2}", $"₱{halfDed:N2}", $"₱{halfNet:N2}", "Released");
        grid.Rows.Add("Apr 16 – Apr 30, 2025", $"₱{halfGross:N2}", $"₱{halfDed:N2}", $"₱{halfNet:N2}", "Released");
        grid.Rows.Add("Apr 1 – Apr 15, 2025", $"₱{halfGross:N2}", $"₱{halfDed:N2}", $"₱{halfNet:N2}", "Released");

        historyCard.BodyHost.Controls.Add(grid);

        layout.Controls.Add(topCards, 0, 0);
        layout.Controls.Add(historyCard, 0, 1);
        return layout;
    }

    // 04. PAYSLIPS
    private Control BuildPayslipsPage()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        // Top Filter & Export Bar
        var actionBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var periodFilter = new FilterDropdown { Size = new Size(200, 36), Location = new Point(0, 6) };
        periodFilter.SetItems(["All Periods", "May 2025", "April 2025", "March 2025"], _selectedPayslipPeriod);

        var downloadFont = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold);
        string downloadText = "Download Payslip";
        var btnDownload = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            Text = downloadText,
            Font = downloadFont,
            Size = new Size(Math.Max(168, TextRenderer.MeasureText(downloadText, downloadFont).Width + 32), 36),
            CornerRadius = AppTheme.Radii.Button,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        var btnPrint = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Print Payslip",
            Font = downloadFont,
            Size = new Size(146, 36),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        actionBar.Resize += (_, _) =>
        {
            btnPrint.Location = new Point(actionBar.Width - btnPrint.Width, 6);
            btnDownload.Location = new Point(btnPrint.Left - 10 - btnDownload.Width, 6);
        };

        actionBar.Controls.Add(periodFilter);
        actionBar.Controls.Add(btnDownload);
        actionBar.Controls.Add(btnPrint);

        // Split: Payslips Table + Preview Card
        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));

        var grid = new StyledDataGridView
        {
            Dock = DockStyle.Fill,
            HeaderBackColor = AppTheme.Colors.PrimarySolid,
            HeaderForeColor = Color.White,
            ColumnHeadersHeight = 48,
            ReadOnly = true
        };
        grid.RowTemplate.Height = 52;
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ref", HeaderText = "PAYSLIP ID", FillWeight = 24 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Period", HeaderText = "PAY PERIOD", FillWeight = 34 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NetPay", HeaderText = "NET PAY", FillWeight = 22 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Released", HeaderText = "DATE", FillWeight = 20 });

        decimal netHalf = Math.Round((_employee.BasicSalary + _employee.Allowances) * 0.855m / 2m, 2);
        var allPayslips = new (string Ref, string Period, string MonthGroup, string Net, string Date)[]
        {
            ("PS-2025-05A", "May 1 – May 15, 2025", "May 2025", $"₱{netHalf:N2}", "May 15, 2025"),
            ("PS-2025-04B", "Apr 16 – Apr 30, 2025", "April 2025", $"₱{netHalf:N2}", "Apr 30, 2025"),
            ("PS-2025-04A", "Apr 1 – Apr 15, 2025", "April 2025", $"₱{netHalf:N2}", "Apr 15, 2025"),
            ("PS-2025-03B", "Mar 16 – Mar 31, 2025", "March 2025", $"₱{netHalf:N2}", "Mar 31, 2025")
        };

        var previewCard = new SectionCard
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(12, 0, 0, 0),
            Title = "Payslip Preview",
            Subtitle = "PS-2025-05A · May 1 – May 15, 2025"
        };
        var lblPreview = new Label
        {
            Dock = DockStyle.Fill,
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true
        };
        previewCard.BodyHost.Controls.Add(lblPreview);

        string activePsId = "PS-2025-05A";
        string activePeriod = "May 1 – May 15, 2025";
        string activePayDate = "May 15, 2025";

        void RefreshPayslips()
        {
            grid.Rows.Clear();
            foreach (var p in allPayslips.Where(x => _selectedPayslipPeriod == "All Periods" || x.MonthGroup == _selectedPayslipPeriod))
            {
                grid.Rows.Add(p.Ref, p.Period, p.Net, p.Date);
            }
            if (grid.Rows.Count > 0)
            {
                UpdatePreview(grid.Rows[0]);
            }
        }

        void UpdatePreview(DataGridViewRow row)
        {
            activePsId = row.Cells[0].Value?.ToString() ?? "PS-2025-05A";
            activePeriod = row.Cells[1].Value?.ToString() ?? "";
            string netVal = row.Cells[2].Value?.ToString() ?? "";
            activePayDate = row.Cells[3].Value?.ToString() ?? "";
            previewCard.Subtitle = $"{activePsId} · {activePeriod}";
            lblPreview.Text =
                $"Employee: {_employee.FullName} ({_employee.EmployeeId})\n" +
                $"Company: {_employee.Company} — {_employee.Department}\n" +
                $"Position: {_employee.Position}\n\n" +
                $"Pay Period: {activePeriod}\n" +
                $"Pay Date: {activePayDate}\n" +
                $"Basic Pay (Semi-monthly): ₱{_employee.BasicSalary / 2m:N2}\n" +
                $"Allowances (Semi-monthly): ₱{_employee.Allowances / 2m:N2}\n" +
                $"Net Take-Home Pay: {netVal}";
        }

        periodFilter.SelectedItemChanged += (_, period) =>
        {
            _selectedPayslipPeriod = period;
            RefreshPayslips();
        };
        grid.SelectionChanged += (_, _) =>
        {
            if (grid.SelectedRows.Count > 0)
            {
                UpdatePreview(grid.SelectedRows[0]);
            }
        };

        btnDownload.Click += (_, _) =>
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf|Word Document (*.doc)|*.doc|CSV Spreadsheet (*.csv)|*.csv|Printable HTML (*.html)|*.html",
                FileName = $"{_employee.EmployeeId}-{activePsId}"
            };
            if (sfd.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
            switch (ext)
            {
                case ".pdf":
                    ExportPayslipPdf(sfd.FileName, activePsId, activePeriod, activePayDate);
                    break;
                case ".csv":
                    File.WriteAllText(sfd.FileName, BuildPayslipCsv(activePsId, activePeriod, activePayDate), Encoding.UTF8);
                    break;
                case ".doc":
                    File.WriteAllText(sfd.FileName, BuildPayslipHtml(activePsId, activePeriod, activePayDate, forWord: true), Encoding.UTF8);
                    break;
                default:
                    File.WriteAllText(sfd.FileName, BuildPayslipHtml(activePsId, activePeriod, activePayDate, forWord: false), Encoding.UTF8);
                    break;
            }

            MessageBox.Show(FindForm(), "Payslip exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        btnPrint.Click += (_, _) =>
        {
            using var doc = new PrintDocument();
            doc.DocumentName = $"{_employee.EmployeeId}-{activePsId}";
            doc.PrintPage += (_, args) =>
            {
                if (args.Graphics is not null)
                {
                    DrawPayslipDocument(args.Graphics, args.MarginBounds, activePsId, activePeriod, activePayDate);
                }
            };
            using var preview = new PrintPreviewDialog { Document = doc, Width = 840, Height = 640 };
            preview.ShowDialog(FindForm());
        };

        RefreshPayslips();
        split.Controls.Add(grid, 0, 0);
        split.Controls.Add(previewCard, 1, 0);

        layout.Controls.Add(actionBar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        return layout;
    }

    private static TableLayoutPanel CreateKeyValueGrid((string Key, string Value)[] items)
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = items.Length,
            BackColor = Color.Transparent
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56f));

        for (int i = 0; i < items.Length; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            table.Controls.Add(new Label
            {
                Text = items[i].Key,
                Font = AppTheme.Typography.Body(),
                ForeColor = AppTheme.Colors.TextSecondary,
                UseCompatibleTextRendering = true,
                UseMnemonic = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, i);
            table.Controls.Add(new Label
            {
                Text = items[i].Value,
                Font = AppTheme.Typography.BodyMedium(),
                ForeColor = AppTheme.Colors.TextDark,
                UseCompatibleTextRendering = true,
                UseMnemonic = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            }, 1, i);
        }

        return table;
    }

    private (decimal BasicSemi, decimal AllowSemi, decimal GrossSemi, decimal DeductionsSemi, decimal NetSemi) ComputeSemiMonthlyPayslip()
    {
        decimal basicSemi = Math.Round(_employee.BasicSalary / 2m, 2);
        decimal allowSemi = Math.Round(_employee.Allowances / 2m, 2);
        decimal grossSemi = basicSemi + allowSemi;
        decimal netSemi = Math.Round((_employee.BasicSalary + _employee.Allowances) * 0.855m / 2m, 2);
        decimal deductionsSemi = grossSemi - netSemi;
        return (basicSemi, allowSemi, grossSemi, deductionsSemi, netSemi);
    }

    private void ExportPayslipPdf(string filePath, string payslipId, string period, string payDate)
    {
        const string pdfPrinter = "Microsoft Print to PDF";
        if (!PrinterSettings.InstalledPrinters.Cast<string>().Any(p => string.Equals(p, pdfPrinter, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(
                FindForm(),
                "Microsoft Print to PDF is not available on this system. Please export as Word (.doc), CSV (.csv), or HTML (.html).",
                "PDF Printer Unavailable",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        using var doc = new PrintDocument();
        doc.DocumentName = $"{_employee.EmployeeId}-{payslipId}";
        doc.PrinterSettings.PrinterName = pdfPrinter;
        doc.PrinterSettings.PrintToFile = true;
        doc.PrinterSettings.PrintFileName = filePath;
        doc.PrintController = new StandardPrintController();
        doc.PrintPage += (_, args) =>
        {
            if (args.Graphics is not null)
            {
                DrawPayslipDocument(args.Graphics, args.MarginBounds, payslipId, period, payDate);
            }
        };
        doc.Print();
    }

    private void DrawPayslipDocument(Graphics g, RectangleF bounds, string payslipId, string period, string payDate)
    {
        var (basicSemi, allowSemi, grossSemi, deductionsSemi, netSemi) = ComputeSemiMonthlyPayslip();
        g.SmoothingMode = SmoothingMode.AntiAlias;

        float x = bounds.Left;
        float y = bounds.Top;
        float w = bounds.Width;

        // Top Brand Header Banner (#4DA2FF)
        var headerRect = new RectangleF(x, y, w, 72);
        using (var headerBrush = new SolidBrush(AppTheme.Colors.PrimarySolid))
        using (var headerPath = AppTheme.CreateRoundedRectPath(Rectangle.Round(headerRect), 10))
        {
            g.FillPath(headerBrush, headerPath);
        }

        using var titleFont = AppTheme.Typography.CreateFont(16f, FontStyle.Bold);
        using var subFont = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular);
        using var sectionFont = AppTheme.Typography.CreateFont(11f, FontStyle.Bold);
        using var bodyFont = AppTheme.Typography.CreateFont(10f, FontStyle.Regular);
        using var boldFont = AppTheme.Typography.CreateFont(10f, FontStyle.Bold);
        using var netFont = AppTheme.Typography.CreateFont(14f, FontStyle.Bold);

        using var darkBrush = new SolidBrush(AppTheme.Colors.TextDark);
        using var secBrush = new SolidBrush(AppTheme.Colors.TextSecondary);
        using var mutedBrush = new SolidBrush(AppTheme.Colors.SurfaceMuted);
        using var borderPen = new Pen(AppTheme.Colors.BorderDefault, 1f);

        g.DrawString("MCT Payroll Management System", titleFont, Brushes.White, x + 20, y + 14);
        g.DrawString("OFFICIAL EMPLOYEE PAYSLIP", subFont, Brushes.White, x + 20, y + 42);
        using var sfRight = new StringFormat { Alignment = StringAlignment.Far };
        g.DrawString($"Payslip ID: {payslipId}", boldFont, Brushes.White, new RectangleF(x, y + 18, w - 20, 22), sfRight);
        g.DrawString($"Pay Date: {payDate}", subFont, Brushes.White, new RectangleF(x, y + 40, w - 20, 20), sfRight);

        y += 90;

        // Employee & Period Info Box
        var infoRect = new RectangleF(x, y, w, 86);
        using (var infoPath = AppTheme.CreateRoundedRectPath(Rectangle.Round(infoRect), 8))
        {
            g.FillPath(mutedBrush, infoPath);
            g.DrawPath(borderPen, infoPath);
        }

        g.DrawString("Employee Name", subFont, secBrush, x + 16, y + 12);
        g.DrawString($"{_employee.FullName} ({_employee.EmployeeId})", boldFont, darkBrush, x + 16, y + 28);
        g.DrawString("Company / Department", subFont, secBrush, x + 16, y + 50);
        g.DrawString($"{_employee.Company} — {_employee.Department} ({_employee.Position})", bodyFont, darkBrush, x + 16, y + 65);

        float midX = x + w * 0.58f;
        g.DrawString("Pay Period", subFont, secBrush, midX, y + 12);
        g.DrawString(period, boldFont, darkBrush, midX, y + 28);
        g.DrawString("Status", subFont, secBrush, midX, y + 50);
        g.DrawString("Released", boldFont, new SolidBrush(AppTheme.Colors.StatusActiveFg), midX, y + 65);

        y += 106;

        // Earnings & Deductions Table
        var tableHeader = new RectangleF(x, y, w, 34);
        using (var thBrush = new SolidBrush(AppTheme.Colors.PrimarySolid))
        {
            g.FillRectangle(thBrush, tableHeader);
        }
        g.DrawString("DESCRIPTION", boldFont, Brushes.White, x + 14, y + 9);
        g.DrawString("AMOUNT (PHP)", boldFont, Brushes.White, new RectangleF(x, y + 9, w - 14, 20), sfRight);
        y += 34;

        var lineItems = new (string Label, string Amount, bool Bold)[]
        {
            ("Basic Pay (Semi-monthly)", $"₱{basicSemi:N2}", false),
            ("Allowances (Semi-monthly)", $"₱{allowSemi:N2}", false),
            ("Gross Pay", $"₱{grossSemi:N2}", true),
            ("Estimated Deductions (Statutory & Tax)", $"-₱{deductionsSemi:N2}", false)
        };

        using var dividerPen = new Pen(AppTheme.Colors.Divider, 1f);
        foreach (var item in lineItems)
        {
            g.DrawString(item.Label, item.Bold ? boldFont : bodyFont, darkBrush, x + 14, y + 10);
            g.DrawString(item.Amount, item.Bold ? boldFont : bodyFont, darkBrush, new RectangleF(x, y + 10, w - 14, 22), sfRight);
            y += 36;
            g.DrawLine(dividerPen, x, y, x + w, y);
        }

        y += 18;

        // Net Take-Home Pay Highlight Card
        var netRect = new RectangleF(x, y, w, 58);
        using (var netBg = new SolidBrush(AppTheme.Colors.StatusActiveBg))
        using (var netPath = AppTheme.CreateRoundedRectPath(Rectangle.Round(netRect), 8))
        using (var netFg = new SolidBrush(AppTheme.Colors.StatusActiveFg))
        {
            g.FillPath(netBg, netPath);
            g.DrawString("NET TAKE-HOME PAY", sectionFont, netFg, x + 18, y + 19);
            g.DrawString($"₱{netSemi:N2}", netFont, netFg, new RectangleF(x, y + 16, w - 18, 30), sfRight);
        }
    }

    private string BuildPayslipHtml(string payslipId, string period, string payDate, bool forWord)
    {
        var (basicSemi, allowSemi, grossSemi, deductionsSemi, netSemi) = ComputeSemiMonthlyPayslip();
        string E(string s) => System.Net.WebUtility.HtmlEncode(s);
        string wordNs = forWord ? " xmlns:w=\"urn:schemas-microsoft-com:office:word\"" : string.Empty;

        return $@"<!DOCTYPE html>
<html{wordNs}>
<head>
<meta charset=""utf-8"" />
<title>Payslip {E(payslipId)} - {E(_employee.FullName)}</title>
<style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #F1F2F5; color: #0B0F2B; margin: 0; padding: 32px; }}
  .sheet {{ max-width: 760px; margin: 0 auto; background: #FFFFFF; border: 1px solid #E3E5EA; border-radius: 12px; overflow: hidden; }}
  .header {{ background: #4DA2FF; color: #FFFFFF; padding: 24px 28px; display: flex; justify-content: space-between; align-items: center; }}
  .header h1 {{ margin: 0; font-size: 20px; }}
  .header .sub {{ font-size: 12px; opacity: 0.9; margin-top: 4px; }}
  .header .meta {{ text-align: right; font-size: 13px; }}
  .content {{ padding: 24px 28px; }}
  .info-box {{ background: #F7F8FA; border: 1px solid #E3E5EA; border-radius: 10px; padding: 16px 20px; margin-bottom: 22px; }}
  .info-table {{ width: 100%; border-collapse: collapse; }}
  .info-table td {{ padding: 4px 8px; font-size: 13px; vertical-align: top; }}
  .lbl {{ color: #737E95; font-size: 11px; text-transform: uppercase; font-weight: 600; }}
  .val {{ color: #0B0F2B; font-weight: 600; margin-top: 2px; }}
  .items {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
  .items th {{ background: #4DA2FF; color: #FFFFFF; text-align: left; padding: 10px 14px; font-size: 12px; letter-spacing: 0.5px; }}
  .items th.amt, .items td.amt {{ text-align: right; }}
  .items td {{ padding: 11px 14px; border-bottom: 1px solid #EDF0F5; font-size: 13.5px; }}
  .items tr.total td {{ font-weight: 700; background: #F8FBFD; }}
  .net-box {{ background: #EFF8F4; border: 1px solid #CBE7DC; border-radius: 10px; padding: 16px 20px; display: flex; justify-content: space-between; align-items: center; color: #438E77; font-weight: 700; }}
  .net-box .amount {{ font-size: 22px; }}
  .footer {{ padding: 14px 28px 20px; font-size: 11.5px; color: #939DAF; text-align: center; }}
</style>
</head>
<body>
  <div class=""sheet"">
    <div class=""header"">
      <div>
        <h1>MCT Payroll Management System</h1>
        <div class=""sub"">OFFICIAL EMPLOYEE PAYSLIP</div>
      </div>
      <div class=""meta"">
        <div><strong>{E(payslipId)}</strong></div>
        <div>Pay Date: {E(payDate)}</div>
      </div>
    </div>
    <div class=""content"">
      <div class=""info-box"">
        <table class=""info-table"">
          <tr>
            <td style=""width:58%""><div class=""lbl"">Employee</div><div class=""val"">{E(_employee.FullName)} ({E(_employee.EmployeeId)})</div></td>
            <td><div class=""lbl"">Pay Period</div><div class=""val"">{E(period)}</div></td>
          </tr>
          <tr>
            <td><div class=""lbl"">Company &amp; Role</div><div class=""val"">{E(_employee.Company)} — {E(_employee.Department)} ({E(_employee.Position)})</div></td>
            <td><div class=""lbl"">Status</div><div class=""val"" style=""color:#438E77"">Released</div></td>
          </tr>
        </table>
      </div>
      <table class=""items"">
        <thead>
          <tr><th>DESCRIPTION</th><th class=""amt"">AMOUNT (PHP)</th></tr>
        </thead>
        <tbody>
          <tr><td>Basic Pay (Semi-monthly)</td><td class=""amt"">&#8369;{basicSemi:N2}</td></tr>
          <tr><td>Allowances (Semi-monthly)</td><td class=""amt"">&#8369;{allowSemi:N2}</td></tr>
          <tr class=""total""><td>Gross Pay</td><td class=""amt"">&#8369;{grossSemi:N2}</td></tr>
          <tr><td>Estimated Deductions (Statutory &amp; Tax)</td><td class=""amt"">-&#8369;{deductionsSemi:N2}</td></tr>
        </tbody>
      </table>
      <div class=""net-box"">
        <span>NET TAKE-HOME PAY</span>
        <span class=""amount"">&#8369;{netSemi:N2}</span>
      </div>
    </div>
    <div class=""footer"">Generated by MCT Payroll Management System</div>
  </div>
</body>
</html>";
    }

    private string BuildPayslipCsv(string payslipId, string period, string payDate)
    {
        var (basicSemi, allowSemi, grossSemi, deductionsSemi, netSemi) = ComputeSemiMonthlyPayslip();
        var sb = new StringBuilder();
        sb.AppendLine("MCT Payroll Management System - Employee Payslip");
        sb.AppendLine($"Payslip ID,\"{payslipId}\"");
        sb.AppendLine($"Employee ID,\"{_employee.EmployeeId}\"");
        sb.AppendLine($"Employee Name,\"{_employee.FullName}\"");
        sb.AppendLine($"Company,\"{_employee.Company}\"");
        sb.AppendLine($"Department,\"{_employee.Department}\"");
        sb.AppendLine($"Position,\"{_employee.Position}\"");
        sb.AppendLine($"Pay Period,\"{period}\"");
        sb.AppendLine($"Pay Date,\"{payDate}\"");
        sb.AppendLine();
        sb.AppendLine("Description,Amount (PHP)");
        sb.AppendLine($"Basic Pay (Semi-monthly),{basicSemi:F2}");
        sb.AppendLine($"Allowances (Semi-monthly),{allowSemi:F2}");
        sb.AppendLine($"Gross Pay,{grossSemi:F2}");
        sb.AppendLine($"Estimated Deductions,-{deductionsSemi:F2}");
        sb.AppendLine($"Net Take-Home Pay,{netSemi:F2}");
        return sb.ToString();
    }
}
