using System.ComponentModel;
using MCT.PayrollAttendance.Configuration;

namespace MCT.PayrollAttendance.Controls;

/// <summary>
/// Footer pagination bar with item count summary and Previous / Page / Next controls.
/// </summary>
public class PaginationBar : UserControl
{
    private readonly Label _lblSummary;
    private readonly AppButton _btnPrev;
    private readonly AppButton _btnPage;
    private readonly AppButton _btnNext;

    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _visibleCount;
    private int _totalCount;

    public PaginationBar()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Size = new Size(900, 44);

        _lblSummary = new Label
        {
            AutoSize = true,
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextSecondary,
            Location = new Point(0, 12),
            Text = "Showing 0 of 0 staff members"
        };

        _btnPrev = new AppButton
        {
            Variant = AppButtonVariant.PaginationNav,
            CornerRadius = 12,
            Size = new Size(34, 29),
            Text = "<",
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _btnPage = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            CornerRadius = 12,
            Size = new Size(34, 29),
            Text = "1",
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _btnNext = new AppButton
        {
            Variant = AppButtonVariant.PaginationNav,
            CornerRadius = 12,
            Size = new Size(34, 29),
            Text = ">",
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _btnPrev.Click += (_, _) =>
        {
            if (_currentPage > 1)
            {
                CurrentPage--;
                PageChanged?.Invoke(this, _currentPage);
            }
        };

        _btnNext.Click += (_, _) =>
        {
            if (_currentPage < _totalPages)
            {
                CurrentPage++;
                PageChanged?.Invoke(this, _currentPage);
            }
        };

        Controls.Add(_lblSummary);
        Controls.Add(_btnPrev);
        Controls.Add(_btnPage);
        Controls.Add(_btnNext);
        LayoutButtons();
    }

    public event EventHandler<int>? PageChanged;

    [Category("Data")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            _currentPage = Math.Max(1, Math.Min(value, Math.Max(1, _totalPages)));
            UpdateState();
        }
    }

    [Category("Data")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int TotalPages
    {
        get => _totalPages;
        set
        {
            _totalPages = Math.Max(1, value);
            if (_currentPage > _totalPages)
            {
                _currentPage = _totalPages;
            }
            UpdateState();
        }
    }

    public void SetCounts(int visibleCount, int totalCount)
    {
        _visibleCount = Math.Max(0, visibleCount);
        _totalCount = Math.Max(0, totalCount);
        _lblSummary.Text = $"Showing {_visibleCount} of {_totalCount} staff members";
    }

    public void SetPageInfo(int visibleCount, int totalCount, int currentPage, int totalPages, string entityLabel = "staff members")
    {
        _totalPages = Math.Max(1, totalPages);
        _currentPage = Math.Max(1, Math.Min(currentPage, _totalPages));
        _visibleCount = Math.Max(0, visibleCount);
        _totalCount = Math.Max(0, totalCount);
        _lblSummary.Text = $"Showing {_visibleCount} of {_totalCount} {entityLabel}";
        UpdateState();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutButtons();
    }

    private void LayoutButtons()
    {
        if (_btnNext is null || _btnPage is null || _btnPrev is null)
        {
            return;
        }

        int right = Width - 4;
        _btnNext.Location = new Point(right - _btnNext.Width, 7);
        _btnPage.Location = new Point(_btnNext.Left - 8 - _btnPage.Width, 7);
        _btnPrev.Location = new Point(_btnPage.Left - 8 - _btnPrev.Width, 7);
    }

    private void UpdateState()
    {
        _btnPage.Text = _currentPage.ToString();
        _btnPrev.Enabled = _currentPage > 1;
        _btnNext.Enabled = _currentPage < _totalPages;
    }
}
