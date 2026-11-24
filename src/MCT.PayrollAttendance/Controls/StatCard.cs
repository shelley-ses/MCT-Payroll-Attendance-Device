using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

public enum StatCardIcon
{
    Employees,
    Present,
    Alert,
    Payroll
}

/// <summary>
/// Dashboard KPI card displaying a metric title, primary value, subtitle/trend, and vector icon badge.
/// </summary>
public class StatCard : RoundedPanel
{
    private readonly Label _lblTitle;
    private readonly Label _lblValue;
    private readonly Label _lblSubtext;
    private StatCardIcon _icon = StatCardIcon.Employees;
    private Color _accentColor = AppTheme.Colors.PrimarySolid;

    public StatCard()
    {
        CornerRadius = AppTheme.Radii.Card;
        BorderColor = AppTheme.Colors.BorderDefault;
        BorderThickness = 1f;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Padding = new Padding(AppTheme.Spacing.CardPadding, 14, AppTheme.Spacing.CardPadding, 14);
        Size = new Size(260, 134);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _lblTitle = new Label
        {
            Text = "Metric Title",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblValue = new Label
        {
            Text = "0",
            Font = AppTheme.Typography.CreateFont(24f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblSubtext = new Label
        {
            Text = "Updated just now",
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextMuted,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var iconBox = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };
        iconBox.Paint += IconBox_Paint;

        layout.Controls.Add(_lblTitle, 0, 0);
        layout.Controls.Add(_lblValue, 0, 1);
        layout.Controls.Add(_lblSubtext, 0, 2);
        layout.Controls.Add(iconBox, 1, 0);
        layout.SetRowSpan(iconBox, 2);

        Controls.Add(layout);
    }

    [Category("Data")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Title
    {
        get => _lblTitle.Text;
        set => _lblTitle.Text = value;
    }

    [Category("Data")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Value
    {
        get => _lblValue.Text;
        set => _lblValue.Text = value;
    }

    [Category("Data")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Subtext
    {
        get => _lblSubtext.Text;
        set => _lblSubtext.Text = value;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public StatCardIcon Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(true); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color AccentColor
    {
        get => _accentColor;
        set { _accentColor = value; Invalidate(true); }
    }

    private void IconBox_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Control box)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var badgeRect = new Rectangle(4, 4, 36, 36);
        using var bgPath = AppTheme.CreateRoundedRectPath(badgeRect, 10);
        using var bgBrush = new SolidBrush(Color.FromArgb(26, _accentColor));
        e.Graphics.FillPath(bgBrush, bgPath);

        using var pen = new Pen(_accentColor, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        int cx = badgeRect.X + 9;
        int cy = badgeRect.Y + 9;

        switch (_icon)
        {
            case StatCardIcon.Employees:
                e.Graphics.DrawEllipse(pen, cx + 2, cy, 7, 7);
                e.Graphics.DrawArc(pen, cx, cy + 9, 11, 8, 180, 180);
                e.Graphics.DrawEllipse(pen, cx + 11, cy + 2, 5, 5);
                break;

            case StatCardIcon.Present:
                e.Graphics.DrawEllipse(pen, cx + 1, cy + 1, 16, 16);
                e.Graphics.DrawLine(pen, cx + 5, cy + 9, cx + 8, cy + 12);
                e.Graphics.DrawLine(pen, cx + 8, cy + 12, cx + 13, cy + 6);
                break;

            case StatCardIcon.Alert:
                e.Graphics.DrawEllipse(pen, cx + 1, cy + 1, 16, 16);
                e.Graphics.DrawLine(pen, cx + 9, cy + 5, cx + 9, cy + 10);
                e.Graphics.DrawLine(pen, cx + 9, cy + 13, cx + 9, cy + 13.5f);
                break;

            case StatCardIcon.Payroll:
                e.Graphics.DrawRectangle(pen, cx + 1, cy + 3, 16, 12);
                e.Graphics.DrawLine(pen, cx + 1, cy + 7, cx + 17, cy + 7);
                break;
        }
    }
}
