using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Reusable rounded search input with leading magnifying glass icon and focus border highlight.
/// </summary>
public class SearchInput : UserControl
{
    private readonly RoundedPanel _container;
    private readonly TextBox _textBox;

    public event EventHandler? SearchTextChanged;

    public SearchInput()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Size = new Size(480, 40);
        MinimumSize = new Size(180, 40);

        _container = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            CornerRadius = AppTheme.Radii.InputLarge,
            BackColor = AppTheme.Colors.SurfaceWhite,
            BorderColor = AppTheme.Colors.BorderInput,
            BorderThickness = 1f,
            Padding = new Padding(38, 9, 12, 8)
        };

        _container.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(AppTheme.Colors.TextPlaceholder, 1.5f);
            int cy = (_container.Height - 14) / 2;
            e.Graphics.DrawEllipse(pen, 14, cy, 12, 12);
            e.Graphics.DrawLine(pen, 24, cy + 10, 29, cy + 15);
        };

        _textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = AppTheme.Typography.CreateFont(11f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextPrimary,
            BackColor = AppTheme.Colors.SurfaceWhite,
            PlaceholderText = "Search..."
        };

        _textBox.TextChanged += (_, _) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
        _textBox.GotFocus += (_, _) =>
        {
            _container.BorderColor = AppTheme.Colors.PrimaryFocus;
            _container.BorderThickness = 2f;
        };
        _textBox.LostFocus += (_, _) =>
        {
            _container.BorderColor = AppTheme.Colors.BorderInput;
            _container.BorderThickness = 1f;
        };

        _container.Controls.Add(_textBox);
        Controls.Add(_container);
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string PlaceholderText
    {
        get => _textBox.PlaceholderText;
        set => _textBox.PlaceholderText = value;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new string Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value;
    }

    public void Clear() => _textBox.Clear();
}
