using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

public enum StatusPillKind
{
    Active,
    Late,
    Early,
    Duration
}

/// <summary>
/// Pill badge for attendance statuses (Active, Late, Early) and duration chips.
/// </summary>
public class StatusPill : Control
{
    private StatusPillKind _kind = StatusPillKind.Active;

    public StatusPill()
    {
        DoubleBuffered = true;
        Size = new Size(94, 24);
        Font = AppTheme.Typography.BodyMedium();
        Text = "Active";
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public StatusPillKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            Text = _kind switch
            {
                StatusPillKind.Active => "Active",
                StatusPillKind.Late => "Late",
                StatusPillKind.Early => "Early",
                StatusPillKind.Duration => "2hrs 30 min",
                _ => Text
            };
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(ResolveOpaqueParentColor());

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        (Color bg, Color fg, int radius) = _kind switch
        {
            StatusPillKind.Active => (AppTheme.Colors.StatusActiveBg, AppTheme.Colors.StatusActiveFg, 10),
            StatusPillKind.Late => (AppTheme.Colors.StatusLateBg, AppTheme.Colors.StatusLateFg, 12),
            StatusPillKind.Early => (AppTheme.Colors.StatusEarlyBg, AppTheme.Colors.StatusEarlyFg, 12),
            StatusPillKind.Duration => (AppTheme.Colors.SurfaceMuted, AppTheme.Colors.TextPrimary, 4),
            _ => (AppTheme.Colors.SurfaceMuted, AppTheme.Colors.TextPrimary, 10)
        };

        using var path = AppTheme.CreateRoundedRectPath(rect, radius);
        using var fillBrush = new SolidBrush(bg);
        e.Graphics.FillPath(fillBrush, path);

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            fg,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    private Color ResolveOpaqueParentColor()
    {
        Control? current = Parent;
        while (current is not null)
        {
            if (current.BackColor.A == 255 && current.BackColor != Color.Transparent)
            {
                return current.BackColor;
            }
            current = current.Parent;
        }

        return AppTheme.Colors.SurfaceWhite;
    }
}
