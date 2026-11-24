using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Double-buffered panel with rounded corners, configurable border, and optional brand gradient fill.
/// </summary>
public class RoundedPanel : Panel
{
    private int _cornerRadius = AppTheme.Radii.Card;
    private Color _borderColor = AppTheme.Colors.BorderDefault;
    private float _borderThickness = 1f;
    private bool _useBrandGradient;
    private float _gradientAngle = 180f;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Colors.SurfaceWhite;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public float BorderThickness
    {
        get => _borderThickness;
        set { _borderThickness = Math.Max(0f, value); Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool UseBrandGradient
    {
        get => _useBrandGradient;
        set { _useBrandGradient = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public float GradientAngle
    {
        get => _gradientAngle;
        set { _gradientAngle = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        using var path = AppTheme.CreateRoundedRectPath(rect, _cornerRadius);

        if (_useBrandGradient)
        {
            using var gradientBrush = AppTheme.CreateBrandGradientBrush(rect, _gradientAngle);
            e.Graphics.FillPath(gradientBrush, path);
        }
        else
        {
            using var fillBrush = new SolidBrush(BackColor);
            e.Graphics.FillPath(fillBrush, path);
        }

        if (_borderThickness > 0f && _borderColor.A > 0)
        {
            using var pen = new Pen(_borderColor, _borderThickness);
            e.Graphics.DrawPath(pen, path);
        }

        base.OnPaint(e);
    }
}
