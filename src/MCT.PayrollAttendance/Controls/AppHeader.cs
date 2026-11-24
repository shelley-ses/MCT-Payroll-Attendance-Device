using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// DPI-safe top page header using TableLayoutPanel with left-aligned title/subtitle and right-aligned live clock/date.
/// </summary>
public class AppHeader : UserControl
{
    private readonly Label _lblPageTitle;
    private readonly Label _lblPageSubtitle;
    private readonly Label _lblClock;
    private readonly Label _lblDate;
    private readonly System.Windows.Forms.Timer _clockTimer;

    public AppHeader()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Top;
        Height = 136;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Padding = new Padding(AppTheme.Spacing.PagePadding, 14, AppTheme.Spacing.PagePadding, 8);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Margin = new Padding(0)
        };
        leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _lblPageTitle = new Label
        {
            Text = "Dashboard",
            Font = AppTheme.Typography.CreateFont(28f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.Primary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2)
        };

        _lblPageSubtitle = new Label
        {
            Text = "Workforce overview, attendance pulse, and payroll status.",
            Font = AppTheme.Typography.CreateFont(12.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextMuted,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Margin = new Padding(2, 0, 0, 0)
        };

        leftPanel.Controls.Add(_lblPageTitle, 0, 0);
        leftPanel.Controls.Add(_lblPageSubtitle, 0, 1);

        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Margin = new Padding(0)
        };
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));

        _lblClock = new Label
        {
            Text = DateTime.Now.ToString("h:mm tt"),
            Font = AppTheme.Typography.ClockDisplay(),
            ForeColor = AppTheme.Colors.Primary,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };

        _lblDate = new Label
        {
            Text = DateTime.Now.ToString("dddd, MMMM d, yyyy"),
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };

        var lblSystem = new Label
        {
            Text = "MCT Payroll Management System",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };

        rightPanel.Controls.Add(_lblClock, 0, 0);
        rightPanel.Controls.Add(_lblDate, 0, 1);
        rightPanel.Controls.Add(lblSystem, 0, 2);

        layout.Controls.Add(leftPanel, 0, 0);
        layout.Controls.Add(rightPanel, 1, 0);
        Controls.Add(layout);

        _clockTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _clockTimer.Tick += (_, _) =>
        {
            string t = DateTime.Now.ToString("h:mm tt");
            if (_lblClock.Text != t) _lblClock.Text = t;
            string d = DateTime.Now.ToString("dddd, MMMM d, yyyy");
            if (_lblDate.Text != d) _lblDate.Text = d;
        };
        _clockTimer.Start();
    }

    public void SetHeader(string title, string subtitle)
    {
        _lblPageTitle.Text = title;
        _lblPageSubtitle.Text = subtitle;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clockTimer.Stop();
            _clockTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
