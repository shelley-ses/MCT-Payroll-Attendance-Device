using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Reusable centered empty-state feedback control for zero-result searches and empty tables.
/// </summary>
public class EmptyState : UserControl
{
    private readonly Label _lblTitle;
    private readonly Label _lblDescription;
    private readonly AppButton _btnAction;

    public event EventHandler? ActionClicked;

    public EmptyState()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Colors.SurfaceWhite;

        var centerStack = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent
        };
        centerStack.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360f));

        var iconCanvas = new Panel
        {
            Size = new Size(360, 64),
            BackColor = Color.Transparent
        };
        iconCanvas.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var circle = new Rectangle(152, 4, 56, 56);
            using var bgBrush = new SolidBrush(AppTheme.Colors.SurfaceMuted);
            e.Graphics.FillEllipse(bgBrush, circle);

            using var pen = new Pen(AppTheme.Colors.TextMuted, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            e.Graphics.DrawEllipse(pen, 169, 20, 16, 16);
            e.Graphics.DrawLine(pen, 182, 33, 190, 41);
        };

        _lblTitle = new Label
        {
            Text = "No matching records",
            Font = AppTheme.Typography.Heading(),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter
        };

        _lblDescription = new Label
        {
            Text = "Try adjusting your search query or company filter.",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextMuted,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            Height = 40,
            TextAlign = ContentAlignment.TopCenter
        };

        var btnHost = new Panel
        {
            Size = new Size(360, 44),
            BackColor = Color.Transparent
        };
        _btnAction = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Reset filters",
            Size = new Size(140, 36),
            Location = new Point(110, 4)
        };
        _btnAction.Click += (_, _) => ActionClicked?.Invoke(this, EventArgs.Empty);
        btnHost.Controls.Add(_btnAction);

        centerStack.Controls.Add(iconCanvas, 0, 0);
        centerStack.Controls.Add(_lblTitle, 0, 1);
        centerStack.Controls.Add(_lblDescription, 0, 2);
        centerStack.Controls.Add(btnHost, 0, 3);

        Controls.Add(centerStack);
        Resize += (_, _) =>
        {
            centerStack.Location = new Point(
                Math.Max(0, (Width - centerStack.Width) / 2),
                Math.Max(0, (Height - centerStack.Height) / 2));
        };
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Title
    {
        get => _lblTitle.Text;
        set => _lblTitle.Text = value;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Description
    {
        get => _lblDescription.Text;
        set => _lblDescription.Text = value;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string ActionText
    {
        get => _btnAction.Text;
        set
        {
            _btnAction.Text = value;
            _btnAction.Visible = !string.IsNullOrWhiteSpace(value);
        }
    }
}
