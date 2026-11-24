using System.ComponentModel;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Reusable titled content card container with optional header action control.
/// </summary>
public class SectionCard : RoundedPanel
{
    private readonly TableLayoutPanel _headerPanel;
    private readonly Label _lblTitle;
    private readonly Label _lblSubtitle;
    private readonly Panel _headerRight;
    private readonly Panel _bodyHost;

    public SectionCard()
    {
        CornerRadius = AppTheme.Radii.Card;
        BorderColor = AppTheme.Colors.BorderDefault;
        BorderThickness = 1f;
        BackColor = AppTheme.Colors.SurfaceWhite;
        Padding = new Padding(AppTheme.Spacing.CardPadding, 14, AppTheme.Spacing.CardPadding, 14);

        _headerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Margin = new Padding(0),
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        _headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
        _headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
        _headerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var titleStack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        _lblTitle = new Label
        {
            Text = "Section Title",
            Font = AppTheme.Typography.Heading(),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            AutoSize = true,
            Margin = new Padding(0)
        };

        _lblSubtitle = new Label
        {
            Text = string.Empty,
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextMuted,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            AutoSize = true,
            Visible = false,
            Margin = new Padding(0, 1, 0, 0)
        };

        titleStack.Controls.Add(_lblTitle);
        titleStack.Controls.Add(_lblSubtitle);

        _headerRight = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 36,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        _headerPanel.Controls.Add(titleStack, 0, 0);
        _headerPanel.Controls.Add(_headerRight, 1, 0);

        _bodyHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0)
        };

        Controls.Add(_bodyHost);
        Controls.Add(_headerPanel);
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
    public string Subtitle
    {
        get => _lblSubtitle.Text;
        set
        {
            _lblSubtitle.Text = value;
            bool hasSubtitle = !string.IsNullOrWhiteSpace(value);
            _lblSubtitle.Visible = hasSubtitle;
            _headerPanel.Height = hasSubtitle ? 60 : 38;
        }
    }

    public Panel BodyHost => _bodyHost;

    public void SetHeaderAction(Control actionControl)
    {
        _headerRight.Controls.Clear();
        actionControl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _headerRight.Controls.Add(actionControl);
        actionControl.Location = new Point(Math.Max(0, _headerRight.Width - actionControl.Width), 4);
        _headerRight.Resize += (_, _) =>
        {
            actionControl.Location = new Point(Math.Max(0, _headerRight.Width - actionControl.Width), 4);
        };
    }
}
