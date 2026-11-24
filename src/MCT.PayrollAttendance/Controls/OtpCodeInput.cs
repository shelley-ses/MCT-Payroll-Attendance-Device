using System.ComponentModel;
using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// 6-digit verification code input with auto-advance and #247BA4 focus highlight.
/// </summary>
public class OtpCodeInput : UserControl
{
    private readonly TextBox[] _boxes = new TextBox[6];
    private int _focusedIndex = -1;

    public OtpCodeInput()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Size = new Size(410, 62);

        const int boxSize = 60;
        const int gap = 10;

        for (int i = 0; i < 6; i++)
        {
            int index = i;
            var tb = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = AppTheme.Typography.ModalTitle(),
                ForeColor = AppTheme.Colors.TextPrimary,
                BackColor = AppTheme.Colors.SurfaceModal,
                MaxLength = 1,
                TextAlign = HorizontalAlignment.Center,
                Size = new Size(boxSize - 12, 32),
                Location = new Point(i * (boxSize + gap) + 6, 15)
            };

            tb.Enter += (_, _) => { _focusedIndex = index; Invalidate(); };
            tb.Leave += (_, _) => { _focusedIndex = -1; Invalidate(); };
            tb.KeyPress += (_, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
            tb.TextChanged += (_, _) =>
            {
                if (tb.Text.Length == 1 && index < 5)
                {
                    _boxes[index + 1].Focus();
                }

                CodeChanged?.Invoke(this, EventArgs.Empty);
                if (IsComplete)
                {
                    CodeCompleted?.Invoke(this, Code);
                }
            };
            tb.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Back && string.IsNullOrEmpty(tb.Text) && index > 0)
                {
                    _boxes[index - 1].Focus();
                }
            };

            _boxes[i] = tb;
            Controls.Add(tb);
        }
    }

    public event EventHandler? CodeChanged;
    public event EventHandler<string>? CodeCompleted;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Code
    {
        get => string.Concat(_boxes.Select(b => b.Text));
        set
        {
            string digits = new((value ?? string.Empty).Where(char.IsDigit).Take(6).ToArray());
            for (int i = 0; i < 6; i++)
            {
                _boxes[i].Text = i < digits.Length ? digits[i].ToString() : string.Empty;
            }
        }
    }

    public bool IsComplete => _boxes.All(b => b.Text.Length == 1);

    public void Clear() => Code = string.Empty;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        const int boxSize = 60;
        const int gap = 10;

        for (int i = 0; i < 6; i++)
        {
            var rect = new Rectangle(i * (boxSize + gap), 0, boxSize - 1, boxSize - 1);
            bool focused = i == _focusedIndex;
            Color borderColor = focused ? AppTheme.Colors.PrimaryDeep : AppTheme.Colors.BorderModal;
            float borderWidth = focused ? 2f : 1f;

            using var path = AppTheme.CreateRoundedRectPath(rect, AppTheme.Radii.InputLarge);
            using var fillBrush = new SolidBrush(AppTheme.Colors.SurfaceModal);
            e.Graphics.FillPath(fillBrush, path);

            using var pen = new Pen(borderColor, borderWidth);
            e.Graphics.DrawPath(pen, path);
        }
    }
}
