using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Circular initials avatar with optional online status dot.
/// </summary>
public class AvatarBadge : Control
{
    private string _initials = "BB";
    private bool _showOnlineIndicator;

    public AvatarBadge()
    {
        DoubleBuffered = true;
        Size = new Size(44, 44);
        Font = AppTheme.Typography.SubHeading();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Initials
    {
        get => _initials;
        set { _initials = (value ?? string.Empty).ToUpperInvariant(); Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool ShowOnlineIndicator
    {
        get => _showOnlineIndicator;
        set { _showOnlineIndicator = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(ResolveOpaqueParentColor());

        int size = Math.Min(Width, Height) - 2;
        if (size <= 0)
        {
            return;
        }

        var circleRect = new Rectangle(0, 0, size, size);
        using (var bgBrush = new SolidBrush(AppTheme.Colors.SurfaceOffDay))
        {
            e.Graphics.FillEllipse(bgBrush, circleRect);
        }

        TextRenderer.DrawText(
            e.Graphics,
            _initials,
            Font,
            circleRect,
            AppTheme.Colors.TextPrimary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        if (_showOnlineIndicator)
        {
            int dotSize = Math.Max(10, size / 4);
            var dotRect = new Rectangle(circleRect.Right - dotSize, circleRect.Bottom - dotSize, dotSize, dotSize);
            using var ringBrush = new SolidBrush(AppTheme.Colors.SurfaceWhite);
            using var dotBrush = new SolidBrush(AppTheme.Colors.OnlineDot);
            e.Graphics.FillEllipse(ringBrush, dotRect);
            dotRect.Inflate(-2, -2);
            e.Graphics.FillEllipse(dotBrush, dotRect);
        }
    }

    private Color ResolveOpaqueParentColor()
    {
        Control? current = Parent;
        while (current is not null)
        {
            if (current.BackColor != Color.Transparent && current.BackColor.A == 255)
            {
                return current.BackColor;
            }

            current = current.Parent;
        }

        return AppTheme.Colors.SurfaceWhite;
    }
}
