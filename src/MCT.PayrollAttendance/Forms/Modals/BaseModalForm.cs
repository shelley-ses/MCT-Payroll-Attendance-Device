using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Unified borderless modal dialog providing a consistent header, body host, footer, blurred backdrop, Esc-to-close, and 1px border.
/// </summary>
public class BaseModalForm : Form
{
    private Form? _backdropForm;
    private Bitmap? _blurredBitmap;
    private Panel? _unifiedHeader;
    private Panel? _unifiedFooter;

    public BaseModalForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        DoubleBuffered = true;
        BackColor = AppTheme.Colors.SurfaceWhite;
        KeyPreview = true;
    }

    protected Panel BuildUnifiedHeader(string title, bool asBadge = false)
    {
        if (_unifiedHeader is not null)
        {
            return _unifiedHeader;
        }

        _unifiedHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(24, 0, 18, 0)
        };
        _unifiedHeader.Paint += (_, e) =>
        {
            using var pen = new Pen(AppTheme.Colors.Divider, 1f);
            e.Graphics.DrawLine(pen, 0, _unifiedHeader.Height - 1, _unifiedHeader.Width, _unifiedHeader.Height - 1);
        };

        if (asBadge)
        {
            var pill = new RoundedPanel
            {
                Location = new Point(24, 18),
                Size = new Size(210, 28),
                CornerRadius = AppTheme.Radii.Chip,
                BackColor = AppTheme.Colors.SurfaceMuted,
                BorderThickness = 0f
            };
            pill.Controls.Add(new Label
            {
                Text = title.ToUpperInvariant(),
                Font = AppTheme.Typography.CreateFont(8.5f, FontStyle.Bold),
                ForeColor = AppTheme.Colors.TextPrimary,
                UseCompatibleTextRendering = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            _unifiedHeader.Controls.Add(pill);
        }
        else
        {
            _unifiedHeader.Controls.Add(new Label
            {
                Text = title,
                Font = AppTheme.Typography.CreateFont(15f, FontStyle.Bold),
                ForeColor = AppTheme.Colors.TextPrimary,
                UseCompatibleTextRendering = true,
                AutoSize = true,
                Location = new Point(24, 18)
            });
        }

        var btnClose = new AppButton
        {
            Variant = AppButtonVariant.IconClose,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(_unifiedHeader.Width - 54, 14)
        };
        btnClose.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        _unifiedHeader.Controls.Add(btnClose);
        _unifiedHeader.Resize += (_, _) => btnClose.Location = new Point(_unifiedHeader.Width - 54, 14);

        return _unifiedHeader;
    }

    protected Panel BuildUnifiedFooter(params Control[] actions)
    {
        _unifiedFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 76,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(24, 14, 24, 14)
        };
        _unifiedFooter.Paint += (_, e) =>
        {
            using var pen = new Pen(AppTheme.Colors.Divider, 1f);
            e.Graphics.DrawLine(pen, 0, 0, _unifiedFooter.Width, 0);
        };

        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent
        };
        foreach (var act in actions)
        {
            flow.Controls.Add(act);
        }

        _unifiedFooter.Controls.Add(flow);
        _unifiedFooter.Layout += (_, _) =>
        {
            flow.Location = new Point(
                Math.Max(16, (_unifiedFooter.Width - flow.Width) / 2),
                Math.Max(10, (_unifiedFooter.Height - flow.Height) / 2));
        };

        return _unifiedFooter;
    }

    public new DialogResult ShowDialog() => ShowDialog(Form.ActiveForm);

    public new DialogResult ShowDialog(IWin32Window? owner)
    {
        Form? ownerForm = owner as Form ?? (owner as Control)?.FindForm() ?? Form.ActiveForm;
        if (ownerForm is null || ownerForm.IsDisposed || !ownerForm.Visible || ownerForm == this)
        {
            return base.ShowDialog(owner);
        }

        ShowBlurredBackdrop(ownerForm);
        bool prevEnabled = ownerForm.Enabled;
        try
        {
            ownerForm.Enabled = false;
            return base.ShowDialog(_backdropForm ?? (IWin32Window)ownerForm);
        }
        finally
        {
            ownerForm.Enabled = prevEnabled;
            CloseBlurredBackdrop();
            if (!ownerForm.IsDisposed && ownerForm.Visible)
            {
                ownerForm.Activate();
            }
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CloseBlurredBackdrop();
        }
        base.Dispose(disposing);
    }

    private void ShowBlurredBackdrop(Form ownerForm)
    {
        if (ownerForm.ClientSize.Width <= 0 || ownerForm.ClientSize.Height <= 0)
        {
            return;
        }

        Rectangle screenBounds = ownerForm.RectangleToScreen(ownerForm.ClientRectangle);
        _blurredBitmap = CaptureAndBlur(screenBounds);

        _backdropForm = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            ShowInTaskbar = false,
            Bounds = screenBounds,
            BackgroundImage = _blurredBitmap,
            BackgroundImageLayout = ImageLayout.Stretch
        };
        _backdropForm.Show(ownerForm);
    }

    private void CloseBlurredBackdrop()
    {
        var backdrop = _backdropForm;
        _backdropForm = null;
        if (backdrop is not null && !backdrop.IsDisposed)
        {
            backdrop.Close();
            backdrop.Dispose();
        }

        var bmp = _blurredBitmap;
        _blurredBitmap = null;
        bmp?.Dispose();
    }

    private static Bitmap CaptureAndBlur(Rectangle screenBounds)
    {
        using var raw = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(raw))
        {
            g.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
        }

        int smallW = Math.Max(16, screenBounds.Width / 10);
        int smallH = Math.Max(16, screenBounds.Height / 10);

        using var small = new Bitmap(smallW, smallH, PixelFormat.Format32bppPArgb);
        using (var g1 = Graphics.FromImage(small))
        {
            g1.InterpolationMode = InterpolationMode.Bilinear;
            g1.DrawImage(raw, new Rectangle(0, 0, smallW, smallH));
        }

        var result = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppPArgb);
        using (var gOut = Graphics.FromImage(result))
        {
            gOut.InterpolationMode = InterpolationMode.Bilinear;
            gOut.DrawImage(small, new Rectangle(0, 0, screenBounds.Width, screenBounds.Height));
            using var dimBrush = new SolidBrush(Color.FromArgb(75, 15, 23, 42));
            gOut.FillRectangle(dimBrush, 0, 0, screenBounds.Width, screenBounds.Height);
        }

        return result;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(AppTheme.Colors.BorderModal, 1f);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
