using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

public enum AppButtonVariant
{
    PrimaryPill,
    PrimaryStandard,
    LoginPrimary,
    CompactAction,
    SecondaryOutline,
    PaginationNav,
    IconClose
}

/// <summary>
/// Custom-painted rounded button implementing all MCT Payroll Attendance button variants.
/// </summary>
public class AppButton : Button
{
    private AppButtonVariant _variant = AppButtonVariant.PrimaryStandard;
    private int _cornerRadius = AppTheme.Radii.Button;
    private bool _isHovered;
    private bool _isPressed;

    public AppButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        Size = new Size(140, 44);
        Font = AppTheme.Typography.SubHeading();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override bool ShowFocusCues => false;

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public AppButtonVariant Variant
    {
        get => _variant;
        set
        {
            _variant = value;
            ApplyVariantDefaults();
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    private void ApplyVariantDefaults()
    {
        switch (_variant)
        {
            case AppButtonVariant.PrimaryPill:
                _cornerRadius = AppTheme.Radii.Pill;
                Height = 55;
                break;
            case AppButtonVariant.PrimaryStandard:
                _cornerRadius = AppTheme.Radii.Button;
                Height = 44;
                break;
            case AppButtonVariant.LoginPrimary:
                _cornerRadius = AppTheme.Radii.Button;
                Height = 49;
                break;
            case AppButtonVariant.CompactAction:
                _cornerRadius = AppTheme.Radii.ButtonCompact;
                Height = 36;
                break;
            case AppButtonVariant.SecondaryOutline:
                _cornerRadius = AppTheme.Radii.Button;
                Height = 44;
                break;
            case AppButtonVariant.PaginationNav:
                _cornerRadius = 12;
                Size = new Size(34, 29);
                break;
            case AppButtonVariant.IconClose:
                _cornerRadius = AppTheme.Radii.Button;
                Size = new Size(36, 36);
                Text = "×";
                break;
        }
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
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        if (mevent.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _isPressed = false;
        Invalidate();
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color? CanvasBackColor { get; set; }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        pevent.Graphics.Clear(ResolveOpaqueParentColor());

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        (Color bg, Color border, Color fg) = ResolvePalette();
        float borderWidth = _variant == AppButtonVariant.PaginationNav ? 2f : 1f;
        if (borderWidth > 1f)
        {
            rect.Inflate(-1, -1);
        }

        using var path = AppTheme.CreateRoundedRectPath(rect, _cornerRadius);
        using var fillBrush = new SolidBrush(bg);
        pevent.Graphics.FillPath(fillBrush, path);

        if (border.A > 0)
        {
            using var pen = new Pen(border, borderWidth);
            pevent.Graphics.DrawPath(pen, path);
        }

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            rect,
            fg,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    private Color ResolveOpaqueParentColor()
    {
        if (CanvasBackColor.HasValue && CanvasBackColor.Value.A == 255)
        {
            return CanvasBackColor.Value;
        }

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

    private (Color bg, Color border, Color fg) ResolvePalette()
    {
        if (!Enabled)
        {
            if (_variant == AppButtonVariant.PaginationNav)
            {
                return (
                    AppTheme.Colors.SurfaceWhite,
                    AppTheme.Colors.BorderPagination,
                    AppTheme.Colors.TextMuted);
            }

            return (
                Color.FromArgb(229, 234, 237),
                AppTheme.Colors.BorderDefault,
                AppTheme.Colors.TextMuted);
        }

        return _variant switch
        {
            AppButtonVariant.PrimaryPill => (
                _isPressed ? AppTheme.Colors.PrimarySolid : (_isHovered ? AppTheme.Colors.PrimaryHover : AppTheme.Colors.Primary),
                Color.FromArgb(128, 151, 151, 151),
                Color.White),

            AppButtonVariant.PrimaryStandard => (
                _isPressed ? AppTheme.Colors.PrimaryDeep : (_isHovered ? AppTheme.Colors.PrimaryHover : AppTheme.Colors.PrimarySoft),
                Color.Transparent,
                AppTheme.Colors.SurfaceCanvas),

            AppButtonVariant.LoginPrimary => (
                _isPressed ? AppTheme.Colors.PrimarySolid : (_isHovered ? AppTheme.Colors.PrimaryHover : AppTheme.Colors.Primary),
                Color.Transparent,
                AppTheme.Colors.TextOnPrimaryDark),

            AppButtonVariant.CompactAction => (
                _isPressed ? AppTheme.Colors.PrimarySolid : (_isHovered ? AppTheme.Colors.PrimaryHover : AppTheme.Colors.Primary),
                Color.Transparent,
                Color.White),

            AppButtonVariant.SecondaryOutline => (
                _isPressed ? AppTheme.Colors.BorderSubtle : (_isHovered ? AppTheme.Colors.SurfaceMuted : AppTheme.Colors.SurfaceModal),
                AppTheme.Colors.BorderModal,
                AppTheme.Colors.TextPrimary),

            AppButtonVariant.PaginationNav => (
                _isPressed ? AppTheme.Colors.SurfaceMuted : (_isHovered ? AppTheme.Colors.SurfaceInput : AppTheme.Colors.SurfaceWhite),
                AppTheme.Colors.BorderPagination,
                AppTheme.Colors.TextPrimary),

            AppButtonVariant.IconClose => (
                _isPressed ? AppTheme.Colors.BorderDefault : (_isHovered ? AppTheme.Colors.SurfaceOffDay : AppTheme.Colors.SurfaceModal),
                Color.Transparent,
                AppTheme.Colors.TextSecondary),

            _ => (AppTheme.Colors.Primary, Color.Transparent, Color.White)
        };
    }
}
