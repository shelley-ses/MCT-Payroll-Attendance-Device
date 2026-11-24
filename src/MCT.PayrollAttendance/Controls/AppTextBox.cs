using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

public enum InputLeadingIcon
{
    None,
    Email,
    Lock
}

/// <summary>
/// Input control supporting labeled/unlabeled modes, leading icons, password eye toggle, Focus (#00A4FF), and Error (#CB4154) states.
/// </summary>
public class AppTextBox : UserControl
{
    private readonly Label _lblTitle;
    private readonly TextBox _innerTextBox;
    private readonly Label _lblError;
    private readonly Panel _eyeToggle;

    private string _errorMessage = string.Empty;
    private bool _isFocused;
    private bool _showLabel = true;
    private bool _showPasswordToggle;
    private int _cornerRadius = AppTheme.Radii.Input;
    private int _boxHeight = 40;
    private InputLeadingIcon _leadingIcon = InputLeadingIcon.None;
    private Color _boxFillColor = AppTheme.Colors.SurfaceWhite;
    private Color _defaultBorderColor = AppTheme.Colors.BorderInput;

    public AppTextBox()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Size = new Size(320, 76);

        _lblTitle = new Label
        {
            AutoSize = true,
            Font = AppTheme.Typography.Label(),
            ForeColor = AppTheme.Colors.TextPrimary,
            Location = new Point(0, 0),
            Text = "Field Label"
        };

        _innerTextBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextDark,
            BackColor = _boxFillColor,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };

        _eyeToggle = new Panel
        {
            Size = new Size(32, 32),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Visible = false
        };
        _eyeToggle.Paint += EyeToggle_Paint;
        _eyeToggle.Click += (_, _) =>
        {
            UseSystemPasswordChar = !UseSystemPasswordChar;
            _eyeToggle.Invalidate();
        };

        _lblError = new Label
        {
            AutoSize = true,
            Font = AppTheme.Typography.Validation(),
            ForeColor = AppTheme.Colors.ErrorText,
            Visible = false
        };

        _innerTextBox.Enter += (_, _) => { _isFocused = true; Invalidate(); };
        _innerTextBox.Leave += (_, _) => { _isFocused = false; Invalidate(); };
        _innerTextBox.TextChanged += (_, e) => TextChanged?.Invoke(this, e);

        Controls.Add(_lblTitle);
        Controls.Add(_innerTextBox);
        Controls.Add(_eyeToggle);
        Controls.Add(_lblError);
        LayoutInnerControls();
    }

    public new event EventHandler? TextChanged;

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool ShowLabel
    {
        get => _showLabel;
        set { _showLabel = value; _lblTitle.Visible = value; LayoutInnerControls(); Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string LabelText
    {
        get => _lblTitle.Text;
        set => _lblTitle.Text = value;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string PlaceholderText
    {
        get => _innerTextBox.PlaceholderText;
        set => _innerTextBox.PlaceholderText = value;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [AllowNull]
    public override string Text
    {
        get => _innerTextBox.Text;
        set => _innerTextBox.Text = value ?? string.Empty;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Font InputFont
    {
        get => _innerTextBox.Font;
        set { _innerTextBox.Font = value; LayoutInnerControls(); }
    }

    [Category("Behavior")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool UseSystemPasswordChar
    {
        get => _innerTextBox.UseSystemPasswordChar;
        set { _innerTextBox.UseSystemPasswordChar = value; _eyeToggle.Invalidate(); }
    }

    [Category("Behavior")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool ShowPasswordToggle
    {
        get => _showPasswordToggle;
        set { _showPasswordToggle = value; _eyeToggle.Visible = value; LayoutInnerControls(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public InputLeadingIcon LeadingIcon
    {
        get => _leadingIcon;
        set { _leadingIcon = value; LayoutInnerControls(); Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BoxFillColor
    {
        get => _boxFillColor;
        set { _boxFillColor = value; _innerTextBox.BackColor = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color DefaultBorderColor
    {
        get => _defaultBorderColor;
        set { _defaultBorderColor = value; Invalidate(); }
    }

    [Category("Layout")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int BoxHeight
    {
        get => _boxHeight;
        set { _boxHeight = Math.Max(32, value); LayoutInnerControls(); Invalidate(); }
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value ?? string.Empty;
            _lblError.Text = _errorMessage;
            _lblError.Visible = !string.IsNullOrWhiteSpace(_errorMessage);
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

    public bool HasError => !string.IsNullOrWhiteSpace(_errorMessage);

    private int BoxTop => _showLabel ? 20 : 0;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutInnerControls();
    }

    private void LayoutInnerControls()
    {
        if (_innerTextBox is null || _lblError is null || _eyeToggle is null)
        {
            return;
        }

        int top = BoxTop;
        int leftPad = _leadingIcon == InputLeadingIcon.None ? 14 : 46;
        int rightPad = _showPasswordToggle ? 44 : 14;

        int tbHeight = _innerTextBox.PreferredHeight;
        _innerTextBox.Location = new Point(leftPad, top + Math.Max(4, (_boxHeight - tbHeight) / 2));
        _innerTextBox.Width = Math.Max(40, Width - leftPad - rightPad);

        _eyeToggle.Location = new Point(Width - 38, top + (_boxHeight - _eyeToggle.Height) / 2);
        _lblError.Location = new Point(0, top + _boxHeight + 4);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var boxRect = new Rectangle(0, BoxTop, Width - 1, _boxHeight);
        if (boxRect.Width <= 0)
        {
            return;
        }

        Color borderColor = HasError
            ? AppTheme.Colors.ErrorBorder
            : (_isFocused ? AppTheme.Colors.PrimaryFocus : _defaultBorderColor);
        float borderWidth = _isFocused && !HasError ? 2f : 1f;

        using var path = AppTheme.CreateRoundedRectPath(boxRect, _cornerRadius);
        using var fillBrush = new SolidBrush(_boxFillColor);
        e.Graphics.FillPath(fillBrush, path);

        using var pen = new Pen(borderColor, borderWidth);
        e.Graphics.DrawPath(pen, path);

        if (_leadingIcon != InputLeadingIcon.None)
        {
            PaintLeadingIcon(e.Graphics, boxRect);
        }
    }

    private void PaintLeadingIcon(Graphics g, Rectangle boxRect)
    {
        using var pen = new Pen(Color.FromArgb(82, 101, 117), 1.6f);
        int cx = 16;
        int cy = boxRect.Y + (boxRect.Height - 16) / 2;

        if (_leadingIcon == InputLeadingIcon.Email)
        {
            var env = new Rectangle(cx, cy + 2, 18, 13);
            g.DrawRectangle(pen, env);
            g.DrawLine(pen, env.Left, env.Top + 1, env.Left + 9, env.Top + 8);
            g.DrawLine(pen, env.Right, env.Top + 1, env.Left + 9, env.Top + 8);
        }
        else if (_leadingIcon == InputLeadingIcon.Lock)
        {
            g.DrawArc(pen, cx + 4, cy, 10, 10, 180, 180);
            g.DrawRectangle(pen, cx + 2, cy + 6, 14, 10);
        }
    }

    private void EyeToggle_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.FromArgb(82, 101, 117), 1.6f);
        int cx = 6;
        int cy = 9;
        e.Graphics.DrawArc(pen, cx, cy, 20, 14, 190, 160);
        e.Graphics.DrawArc(pen, cx, cy, 20, 14, 10, 160);
        e.Graphics.DrawEllipse(pen, cx + 7, cy + 4, 6, 6);

        if (!UseSystemPasswordChar)
        {
            e.Graphics.DrawLine(pen, cx + 2, cy + 13, cx + 18, cy + 1);
        }
    }
}
