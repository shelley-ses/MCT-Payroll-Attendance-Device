using System.Drawing.Drawing2D;
using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Employee Details Hub modal opened on double-click of an Employee row, offering profile summary and 4 page navigation cards.
/// </summary>
public class EmployeeDetailsHubModal : BaseModalForm
{
    public string SelectedDestination { get; private set; } = string.Empty;

    public EmployeeDetailsHubModal(EmployeeRecord employee)
    {
        Size = new Size(740, 740);
        MinimumSize = new Size(600, 540);
        BackColor = AppTheme.Colors.SurfaceWhite;

        var header = BuildUnifiedHeader("Employee Details", asBadge: true);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(24, 16, 24, 16)
        };

        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        // 1. Profile Header Card
        stack.Controls.Add(BuildProfileHeader(employee), 0, 0);

        // 2. Personal & Employment Information
        stack.Controls.Add(BuildEmploymentSection(employee), 0, 1);

        // 3. Employee Information (Email & Contact)
        stack.Controls.Add(BuildContactSection(employee), 0, 2);

        // 4. Employee Pages (4 Interactive Cards)
        stack.Controls.Add(BuildPagesSection(), 0, 3);

        body.Controls.Add(stack);

        Controls.Add(body);
        Controls.Add(header);
    }

    private static Control BuildProfileHeader(EmployeeRecord emp)
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Top,
            Height = 112,
            CornerRadius = AppTheme.Radii.Card,
            BorderColor = AppTheme.Colors.BorderDefault,
            BackColor = AppTheme.Colors.SurfaceInput,
            Padding = new Padding(18, 14, 18, 14),
            Margin = new Padding(0, 0, 0, 14)
        };

        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));

        var avatar = new AvatarBadge
        {
            Initials = emp.Initials,
            ShowOnlineIndicator = false,
            Size = new Size(64, 64),
            Margin = new Padding(0, 8, 0, 0)
        };

        var textStack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(4, 4, 0, 0)
        };
        textStack.Controls.Add(new Label
        {
            Text = emp.FullName,
            Font = AppTheme.Typography.CreateFont(17f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            AutoSize = true,
            Margin = new Padding(0)
        });
        textStack.Controls.Add(new Label
        {
            Text = $"{emp.EmployeeId}  ·  {emp.Position}",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            UseMnemonic = false,
            AutoSize = true,
            Margin = new Padding(0, 2, 0, 0)
        });

        var statusPill = new StatusPill
        {
            Kind = emp.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? StatusPillKind.Active : StatusPillKind.Late,
            Text = emp.Status,
            Size = new Size(94, 26),
            Margin = new Padding(8, 26, 0, 0)
        };

        row.Controls.Add(avatar, 0, 0);
        row.Controls.Add(textStack, 1, 0);
        row.Controls.Add(statusPill, 2, 0);
        card.Controls.Add(row);
        return card;
    }

    private static SectionCard BuildEmploymentSection(EmployeeRecord emp)
    {
        var section = new SectionCard
        {
            Dock = DockStyle.Top,
            Height = 148,
            Title = "Personal & Employment Information",
            Margin = new Padding(0, 0, 0, 14)
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        for (int i = 0; i < 4; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        AddField(grid, 0, "Company", emp.Company);
        AddField(grid, 1, "Department", emp.Department);
        AddField(grid, 2, "Employment Type", emp.EmploymentType);
        AddField(grid, 3, "Date Hired", emp.DateHired.ToString("MMM dd, yyyy"));

        section.BodyHost.Controls.Add(grid);
        return section;
    }

    private static SectionCard BuildContactSection(EmployeeRecord emp)
    {
        var section = new SectionCard
        {
            Dock = DockStyle.Top,
            Height = 132,
            Title = "Employee Information",
            Margin = new Padding(0, 0, 0, 14)
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        AddField(grid, 0, "Email Address", emp.Email);
        AddField(grid, 1, "Contact Number", emp.Phone);

        section.BodyHost.Controls.Add(grid);
        return section;
    }

    private SectionCard BuildPagesSection()
    {
        var section = new SectionCard
        {
            Dock = DockStyle.Top,
            Height = 250,
            Title = "Employee Pages",
            Subtitle = "Select a section to open this employee's dedicated view",
            Margin = new Padding(0)
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        grid.Controls.Add(CreateNavTile("Overview", "Employee profile and information", "Overview"), 0, 0);
        grid.Controls.Add(CreateNavTile("Attendance", "Weekly records and attendance history", "Attendance"), 1, 0);
        grid.Controls.Add(CreateNavTile("Payroll & Deductions", "Salary breakdown and deductions", "Payroll"), 0, 1);
        grid.Controls.Add(CreateNavTile("Payslips", "Payslip history and PDF exports", "Payslips"), 1, 1);

        section.BodyHost.Controls.Add(grid);
        return section;
    }

    private sealed class NavTileButton : Button
    {
        private readonly string _title;
        private readonly string _description;
        private bool _hovered;

        public NavTileButton(string title, string description)
        {
            _title = title;
            _description = description;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Dock = DockStyle.Fill;
            Margin = new Padding(4);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override bool ShowFocusCues => false;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(AppTheme.Colors.SurfaceWhite);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Color bg = _hovered ? AppTheme.Colors.SurfaceInput : AppTheme.Colors.SurfaceMuted;
            Color border = _hovered ? AppTheme.Colors.PrimarySolid : AppTheme.Colors.BorderDefault;

            using var path = AppTheme.CreateRoundedRectPath(rect, AppTheme.Radii.InputLarge);
            using var bgBrush = new SolidBrush(bg);
            using var pen = new Pen(border, 1.2f);
            g.FillPath(bgBrush, path);
            g.DrawPath(pen, path);

            using var titleFont = AppTheme.Typography.SubHeading();
            using var descFont = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular);
            using var titleBrush = new SolidBrush(AppTheme.Colors.PrimaryDeep);
            using var descBrush = new SolidBrush(AppTheme.Colors.TextSecondary);
            using var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };

            g.DrawString($"{_title}  →", titleFont, titleBrush, new RectangleF(14, 10, Width - 28, 26), sf);
            g.DrawString(_description, descFont, descBrush, new RectangleF(14, 36, Width - 28, Height - 44), sf);
        }
    }

    private Control CreateNavTile(string title, string description, string destinationKey)
    {
        var tile = new NavTileButton(title, description);
        tile.Click += (_, _) =>
        {
            SelectedDestination = destinationKey;
            DialogResult = DialogResult.OK;
            Close();
        };
        return tile;
    }

    private static void AddField(TableLayoutPanel grid, int col, string label, string value)
    {
        grid.Controls.Add(new Label
        {
            Text = label.ToUpperInvariant(),
            Font = AppTheme.Typography.Overline(),
            ForeColor = AppTheme.Colors.TextMuted,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        }, col, 0);

        grid.Controls.Add(new Label
        {
            Text = value,
            Font = AppTheme.Typography.BodyMedium(),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft
        }, col, 1);
    }
}
