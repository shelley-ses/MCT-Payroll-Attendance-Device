using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// 276px gradient left navigation sidebar matching Figma FINAL UI, with vector icons, active underline, and Log Out footer.
/// </summary>
public class AppSidebar : RoundedPanel
{
    private sealed class NavButton : Control
    {
        private bool _isHovered;

        public string Key { get; }
        public string Label { get; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive { get; set; }

        public NavButton(string key, string label)
        {
            Key = key;
            Label = label;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Size = new Size(244, 42);
            Margin = new Padding(0, 0, 0, 14);
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (_isHovered && !IsActive)
            {
                using var hoverBrush = new SolidBrush(Color.FromArgb(45, 255, 255, 255));
                using var hoverPath = AppTheme.CreateRoundedRectPath(new Rectangle(0, 0, Width - 1, Height - 1), 8);
                e.Graphics.FillPath(hoverBrush, hoverPath);
            }

            Color fg = IsActive ? AppTheme.Colors.Primary : AppTheme.Colors.TextPrimary;
            var iconRect = new Rectangle(4, (Height - 20) / 2, 20, 20);
            DrawNavIcon(e.Graphics, Key, iconRect, fg);

            var textRect = new Rectangle(34, 2, Width - 38, Height - 2);
            TextRenderer.DrawText(
                e.Graphics,
                Label,
                Font,
                textRect,
                fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            if (IsActive)
            {
                Size measured = TextRenderer.MeasureText(Label, Font, textRect.Size, TextFormatFlags.NoPrefix);
                int underlineEnd = Math.Min(Width - 4, 34 + measured.Width);
                using var pen = new Pen(AppTheme.Colors.PrimarySolid, 2f);
                e.Graphics.DrawLine(pen, 4, Height - 2, underlineEnd, Height - 2);
            }
        }
    }

    private readonly Dictionary<string, NavButton> _navButtons = new();
    private string _activePage = "Dashboard";

    public event EventHandler<string>? NavigationRequested;
    public event EventHandler? LogoutRequested;

    public AppSidebar()
    {
        Dock = DockStyle.Left;
        Width = AppTheme.Spacing.SidebarWidth;
        CornerRadius = 0;
        BorderThickness = 0f;
        UseBrandGradient = true;
        GradientAngle = 180f;
        Padding = new Padding(18, 150, 14, 20);

        var navItems = new (string Key, string Label)[]
        {
            ("Dashboard", "Dashboard"),
            ("Employees", "Employee Management"),
            ("Attendance", "Attendance"),
            ("Payroll", "Payroll Processing"),
            ("Deductions", "Deductions"),
            ("Payslips", "Payslip Management"),
            ("Admin", "System Admin")
        };

        var navStack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        foreach (var (key, label) in navItems)
        {
            var btn = new NavButton(key, label)
            {
                IsActive = key == _activePage
            };
            btn.Click += (_, _) =>
            {
                SetActivePage(key);
                NavigationRequested?.Invoke(this, key);
            };
            _navButtons[key] = btn;
            navStack.Controls.Add(btn);
        }

        var logoutPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 8, 0, 0)
        };

        var btnLogout = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            CornerRadius = AppTheme.Radii.InputLarge,
            Text = "Log Out",
            Dock = DockStyle.Fill,
            ForeColor = AppTheme.Colors.PrimaryDeep,
            CanvasBackColor = Color.FromArgb(190, 226, 245),
            Font = AppTheme.Typography.CreateFont(11f, FontStyle.Bold)
        };
        btnLogout.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(AppTheme.Colors.PrimaryDeep, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            int x = 58;
            int cy = btnLogout.Height / 2;
            e.Graphics.DrawLines(pen, [new Point(x + 4, cy - 6), new Point(x, cy - 6), new Point(x, cy + 6), new Point(x + 4, cy + 6)]);
            e.Graphics.DrawLine(pen, x + 4, cy, x + 13, cy);
            e.Graphics.DrawLines(pen, [new Point(x + 10, cy - 3), new Point(x + 13, cy), new Point(x + 10, cy + 3)]);
        };
        btnLogout.Click += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        logoutPanel.Controls.Add(btnLogout);

        Controls.Add(navStack);
        Controls.Add(logoutPanel);
    }

    public string ActivePage => _activePage;

    public void SetActivePage(string pageKey)
    {
        _activePage = pageKey;
        foreach (var (key, btn) in _navButtons)
        {
            btn.IsActive = key == pageKey;
            btn.Invalidate();
        }
    }

    private static void DrawNavIcon(Graphics g, string key, Rectangle r, Color color)
    {
        using var pen = new Pen(color, 1.7f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var brush = new SolidBrush(color);

        switch (key)
        {
            case "Dashboard":
                int s = 7;
                g.FillRectangle(brush, r.X + 2, r.Y + 2, s, s);
                g.FillRectangle(brush, r.X + 11, r.Y + 2, s, s);
                g.FillRectangle(brush, r.X + 2, r.Y + 11, s, s);
                g.FillRectangle(brush, r.X + 11, r.Y + 11, s, s);
                break;

            case "Employees":
                g.DrawEllipse(pen, r.X + 3, r.Y + 2, 7, 7);
                g.DrawArc(pen, r.X + 1, r.Y + 11, 11, 9, 180, 180);
                g.DrawEllipse(pen, r.X + 12, r.Y + 4, 5, 5);
                g.DrawArc(pen, r.X + 11, r.Y + 12, 8, 7, 190, 160);
                break;

            case "Attendance":
                g.DrawEllipse(pen, r.X + 2, r.Y + 2, 16, 16);
                g.DrawLine(pen, r.X + 10, r.Y + 6, r.X + 10, r.Y + 10);
                g.DrawLine(pen, r.X + 10, r.Y + 10, r.X + 14, r.Y + 12);
                break;

            case "Payroll":
                g.DrawRectangle(pen, r.X + 2, r.Y + 4, 16, 12);
                g.DrawLine(pen, r.X + 2, r.Y + 8, r.X + 18, r.Y + 8);
                g.DrawLine(pen, r.X + 5, r.Y + 12, r.X + 11, r.Y + 12);
                break;

            case "Deductions":
                g.DrawEllipse(pen, r.X + 4, r.Y + 2, 10, 10);
                g.DrawLine(pen, r.X + 9, r.Y + 5, r.X + 9, r.Y + 9);
                g.DrawLine(pen, r.X + 2, r.Y + 15, r.X + 14, r.Y + 15);
                g.DrawLine(pen, r.X + 14, r.Y + 15, r.X + 18, r.Y + 12);
                break;

            case "Payslips":
                g.DrawRectangle(pen, r.X + 3, r.Y + 2, 14, 16);
                g.DrawLine(pen, r.X + 6, r.Y + 7, r.X + 14, r.Y + 7);
                g.DrawLine(pen, r.X + 6, r.Y + 11, r.X + 14, r.Y + 11);
                g.DrawLine(pen, r.X + 6, r.Y + 15, r.X + 11, r.Y + 15);
                break;

            default:
                g.DrawEllipse(pen, r.X + 6, r.Y + 2, 8, 8);
                g.DrawArc(pen, r.X + 2, r.Y + 11, 16, 9, 180, 180);
                break;
        }
    }
}
