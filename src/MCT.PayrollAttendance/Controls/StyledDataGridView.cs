using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Double-buffered DataGridView with Figma-accurate GDI+ header, 3px #EDF0F5 row dividers, and modern rounded checkboxes.
/// </summary>
public class StyledDataGridView : DataGridView
{
    private bool _headerChecked;

    public event EventHandler<bool>? HeaderCheckboxChanged;

    public StyledDataGridView()
    {
        DoubleBuffered = true;
        BackgroundColor = AppTheme.Colors.SurfaceWhite;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.None;
        GridColor = AppTheme.Colors.SurfaceWhite;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        EnableHeadersVisualStyles = false;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        AllowUserToResizeRows = false;
        RowHeadersVisible = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        MultiSelect = false;
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        ColumnHeadersHeight = AppTheme.Spacing.TableHeaderCompactHeight;
        RowTemplate.Height = AppTheme.Spacing.TableRowHeight;
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color HeaderBackColor { get; set; } = AppTheme.Colors.SurfaceMuted;

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color HeaderForeColor { get; set; } = AppTheme.Colors.TextSecondary;

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int RowDividerThickness { get; set; } = AppTheme.Spacing.RowDividerThickness;

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string? SortIndicatorColumn { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HeaderChecked
    {
        get => _headerChecked;
        set
        {
            _headerChecked = value;
            Invalidate();
        }
    }

    public override DataGridViewAdvancedBorderStyle AdjustColumnHeaderBorderStyle(
        DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput,
        DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder,
        bool isFirstDisplayedColumn,
        bool isLastVisibleColumn)
    {
        dataGridViewAdvancedBorderStylePlaceholder.All = DataGridViewAdvancedCellBorderStyle.None;
        return dataGridViewAdvancedBorderStylePlaceholder;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.None;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
    }

    protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex >= 0 && Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
        {
            _headerChecked = !_headerChecked;
            foreach (DataGridViewRow row in Rows)
            {
                row.Cells[e.ColumnIndex].Value = _headerChecked;
            }
            Invalidate();
            HeaderCheckboxChanged?.Invoke(this, _headerChecked);
            return;
        }

        base.OnColumnHeaderMouseClick(e);
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        if (e.Graphics is null)
        {
            base.OnCellPainting(e);
            return;
        }

        if (e.RowIndex == -1 && e.ColumnIndex >= 0)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var bgBrush = new SolidBrush(HeaderBackColor);
            e.Graphics.FillRectangle(bgBrush, e.CellBounds);

            if (Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
            {
                DrawModernCheckbox(e.Graphics, e.CellBounds, _headerChecked);
                e.Handled = true;
                return;
            }

            string text = Columns[e.ColumnIndex].HeaderText;
            if (!string.IsNullOrEmpty(text))
            {
                using var fgBrush = new SolidBrush(HeaderForeColor);
                using var font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold);
                bool center = Columns[e.ColumnIndex].DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter;
                using var sf = new StringFormat
                {
                    Alignment = center ? StringAlignment.Center : StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                var textBounds = new RectangleF(
                    e.CellBounds.X + 8,
                    e.CellBounds.Y,
                    Math.Max(10, e.CellBounds.Width - 16),
                    e.CellBounds.Height);
                e.Graphics.DrawString(text, font, fgBrush, textBounds, sf);

                if (!string.IsNullOrEmpty(SortIndicatorColumn) &&
                    string.Equals(Columns[e.ColumnIndex].Name, SortIndicatorColumn, StringComparison.OrdinalIgnoreCase))
                {
                    SizeF measured = e.Graphics.MeasureString(text, font);
                    int iconX = e.CellBounds.X + 14 + (int)measured.Width;
                    int iconY = e.CellBounds.Y + (e.CellBounds.Height - 12) / 2;
                    DrawSortArrows(e.Graphics, iconX, iconY, HeaderForeColor);
                }
            }

            e.Handled = true;
            return;
        }

        if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
        {
            e.PaintBackground(e.CellBounds, true);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            bool isChecked = e.Value is bool b && b;
            DrawModernCheckbox(e.Graphics, e.CellBounds, isChecked);
            e.Handled = true;
            return;
        }

        base.OnCellPainting(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Clean any 1px system border/seam artifacts on header edges, column seams, and left edge
        using var headerBrush = new SolidBrush(HeaderBackColor);
        using var bodyBrush = new SolidBrush(BackgroundColor);

        e.Graphics.FillRectangle(headerBrush, 0, 0, Width, 1);
        e.Graphics.FillRectangle(headerBrush, 0, 0, 1, ColumnHeadersHeight);
        e.Graphics.FillRectangle(headerBrush, 0, ColumnHeadersHeight - 1, Width, 1);
        e.Graphics.FillRectangle(bodyBrush, 0, ColumnHeadersHeight, 1, Math.Max(0, Height - ColumnHeadersHeight));

        int x = 0;
        foreach (DataGridViewColumn col in Columns)
        {
            if (!col.Visible) continue;
            x += col.Width;
            if (x > 0 && x < Width)
            {
                e.Graphics.FillRectangle(headerBrush, x - 1, 0, 2, ColumnHeadersHeight);
                e.Graphics.FillRectangle(bodyBrush, x - 1, ColumnHeadersHeight, 2, Math.Max(0, Height - ColumnHeadersHeight));
            }
        }

        if (RowDividerThickness > 0)
        {
            using var dividerBrush = new SolidBrush(AppTheme.Colors.Divider);
            foreach (DataGridViewRow row in Rows)
            {
                if (!row.Visible || !row.Displayed) continue;
                Rectangle r = GetRowDisplayRectangle(row.Index, false);
                if (r.Height > 0)
                {
                    e.Graphics.FillRectangle(dividerBrush, 0, r.Bottom - RowDividerThickness, Width, RowDividerThickness);
                }
            }
        }
    }

    protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
    {
        base.OnRowPostPaint(e);
        if (RowDividerThickness <= 0)
        {
            return;
        }

        int y = e.RowBounds.Bottom - RowDividerThickness;
        if (y < e.RowBounds.Top)
        {
            return;
        }

        using var dividerBrush = new SolidBrush(AppTheme.Colors.Divider);
        e.Graphics.FillRectangle(dividerBrush, e.RowBounds.Left, y, e.RowBounds.Width, RowDividerThickness);
    }

    private static void DrawModernCheckbox(Graphics g, Rectangle cellBounds, bool isChecked)
    {
        const int size = 22;
        int x = cellBounds.X + (cellBounds.Width - size) / 2;
        int y = cellBounds.Y + (cellBounds.Height - size) / 2;
        var rect = new Rectangle(x, y, size, size);

        using var path = AppTheme.CreateRoundedRectPath(rect, AppTheme.Radii.Checkbox);
        using var fillBrush = new SolidBrush(isChecked ? AppTheme.Colors.Primary : AppTheme.Colors.SurfaceWhite);
        g.FillPath(fillBrush, path);

        using var borderPen = new Pen(isChecked ? AppTheme.Colors.PrimarySolid : AppTheme.Colors.BorderCheckbox, 2f);
        g.DrawPath(borderPen, path);

        if (isChecked)
        {
            using var checkPen = new Pen(Color.White, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLines(checkPen,
            [
                new Point(x + 5, y + 11),
                new Point(x + 9, y + 15),
                new Point(x + 17, y + 7)
            ]);
        }
    }

    private static void DrawSortArrows(Graphics g, int x, int y, Color color)
    {
        using var pen = new Pen(color, 1.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        g.DrawLines(pen, [new Point(x, y + 4), new Point(x + 3, y + 1), new Point(x + 6, y + 4)]);
        g.DrawLines(pen, [new Point(x, y + 8), new Point(x + 3, y + 11), new Point(x + 6, y + 8)]);
    }
}
