using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

public enum WorkingDaysDisplayMode
{
    CompactCircles,
    ExpandedChips
}

/// <summary>
/// Displays an employee's weekly schedule either as S M T W T F S circles or Mon-Fri chips.
/// </summary>
public class WorkingDaysStrip : Control
{
    private static readonly string[] DayLetters = ["S", "M", "T", "W", "T", "F", "S"];
    private static readonly string[] DayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    private bool[] _workingDays = [false, true, true, true, true, true, false];
    private WorkingDaysDisplayMode _displayMode = WorkingDaysDisplayMode.CompactCircles;

    public WorkingDaysStrip()
    {
        DoubleBuffered = true;
        Size = new Size(266, 36);
        Font = AppTheme.Typography.BodyMedium();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public WorkingDaysDisplayMode DisplayMode
    {
        get => _displayMode;
        set
        {
            _displayMode = value;
            Size = _displayMode == WorkingDaysDisplayMode.CompactCircles ? new Size(266, 36) : new Size(540, 44);
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool[] WorkingDays
    {
        get => (bool[])_workingDays.Clone();
        set
        {
            if (value is { Length: 7 })
            {
                _workingDays = (bool[])value.Clone();
                Invalidate();
            }
        }
    }

    public void SetWorkingDays(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
    {
        _workingDays = [sun, mon, tue, wed, thu, fri, sat];
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(ResolveOpaqueParentColor());

        if (_displayMode == WorkingDaysDisplayMode.CompactCircles)
        {
            PaintCompactCircles(e.Graphics);
        }
        else
        {
            PaintExpandedChips(e.Graphics);
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

    private void PaintCompactCircles(Graphics g)
    {
        const int size = 32;
        const int gap = 6;

        for (int i = 0; i < 7; i++)
        {
            var rect = new Rectangle(i * (size + gap), (Height - size) / 2, size, size);
            bool active = _workingDays[i];
            Color bg = active ? AppTheme.Colors.Primary : AppTheme.Colors.SurfaceOffDay;
            Color fg = active ? Color.White : AppTheme.Colors.TextSecondary;

            using var brush = new SolidBrush(bg);
            g.FillEllipse(brush, rect);

            TextRenderer.DrawText(
                g,
                DayLetters[i],
                Font,
                rect,
                fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private void PaintExpandedChips(Graphics g)
    {
        const int chipWidth = 99;
        const int chipHeight = 42;
        const int gap = 8;
        int x = 0;

        for (int i = 0; i < 7; i++)
        {
            if (!_workingDays[i])
            {
                continue;
            }

            var rect = new Rectangle(x, (Height - chipHeight) / 2, chipWidth, chipHeight);
            using var path = AppTheme.CreateRoundedRectPath(rect, AppTheme.Radii.InputLarge);
            using var brush = new SolidBrush(AppTheme.Colors.Primary);
            g.FillPath(brush, path);

            TextRenderer.DrawText(
                g,
                DayNames[i],
                Font,
                rect,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            x += chipWidth + gap;
        }
    }
}
