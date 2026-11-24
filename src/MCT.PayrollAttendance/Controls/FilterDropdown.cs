using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Modern flat ToolStrip renderer replacing the default Windows gray/blue ContextMenuStrip look.
/// </summary>
public sealed class MctMenuRenderer : ToolStripProfessionalRenderer
{
    public MctMenuRenderer() : base(new MctColorTable())
    {
        RoundedEdges = true;
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextFont = AppTheme.Typography.Body();
        e.TextColor = e.Item.Enabled ? AppTheme.Colors.TextPrimary : AppTheme.Colors.TextMuted;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var rect = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
        if (e.Item.Selected)
        {
            using var brush = new SolidBrush(AppTheme.Colors.SurfaceInput);
            using var path = AppTheme.CreateRoundedRectPath(rect, 6);
            e.Graphics.FillPath(brush, path);
        }
        else
        {
            using var brush = new SolidBrush(AppTheme.Colors.SurfaceWhite);
            e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
        }
    }

    private sealed class MctColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => AppTheme.Colors.SurfaceWhite;
        public override Color MenuBorder => AppTheme.Colors.BorderDefault;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => AppTheme.Colors.SurfaceInput;
        public override Color ImageMarginGradientBegin => AppTheme.Colors.SurfaceWhite;
        public override Color ImageMarginGradientMiddle => AppTheme.Colors.SurfaceWhite;
        public override Color ImageMarginGradientEnd => AppTheme.Colors.SurfaceWhite;
    }
}

/// <summary>
/// Reusable primary-tinted dropdown filter button with custom-styled popup menu.
/// </summary>
public class FilterDropdown : UserControl
{
    private readonly AppButton _button;
    private readonly ContextMenuStrip _menu;
    private string _selectedItem = "All Companies";
    private string[] _items = [];

    public event EventHandler<string>? SelectedItemChanged;

    public FilterDropdown()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Size = new Size(164, 40);

        _menu = new ContextMenuStrip
        {
            Renderer = new MctMenuRenderer(),
            ShowImageMargin = false,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Font = AppTheme.Typography.Body()
        };

        _button = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            CornerRadius = AppTheme.Radii.InputLarge,
            Dock = DockStyle.Fill,
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            Text = $"    {_selectedItem}"
        };
        _button.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.White, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            int cy = _button.Height / 2 - 2;
            e.Graphics.DrawLines(pen, [new Point(13, cy), new Point(17, cy + 4), new Point(21, cy)]);
        };

        _button.Click += (_, _) => _menu.Show(_button, new Point(0, _button.Height + 4));
        Controls.Add(_button);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            _button.Text = $"    {_selectedItem}";
            RefreshMenuChecks();
        }
    }

    public void SetItems(IEnumerable<string> items, string? defaultItem = null)
    {
        _items = items.ToArray();
        _menu.Items.Clear();

        foreach (string item in _items)
        {
            var menuItem = new ToolStripMenuItem(item)
            {
                Checked = string.Equals(item, defaultItem ?? _selectedItem, StringComparison.Ordinal)
            };
            menuItem.Click += (_, _) =>
            {
                SelectedItem = item;
                SelectedItemChanged?.Invoke(this, item);
            };
            _menu.Items.Add(menuItem);
        }

        if (defaultItem is not null)
        {
            SelectedItem = defaultItem;
        }
        else if (_items.Length > 0 && !_items.Contains(_selectedItem))
        {
            SelectedItem = _items[0];
        }
    }

    private void RefreshMenuChecks()
    {
        foreach (ToolStripItem item in _menu.Items)
        {
            if (item is ToolStripMenuItem mi)
            {
                mi.Checked = string.Equals(mi.Text, _selectedItem, StringComparison.Ordinal);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _menu.Dispose();
        }
        base.Dispose(disposing);
    }
}
