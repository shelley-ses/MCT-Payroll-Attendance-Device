using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Employee information preview modal matching Figma FINAL UI (View details).
/// </summary>
public class ViewDetailsModal : BaseModalForm
{
    public ViewDetailsModal(
        string initials = "BB",
        string name = "Bernadette Balagot",
        string company = "MCT Hotel",
        string email = "balagotbernadette@gmail.com",
        string phone = "209 555-0104",
        string salary = "₱20,000",
        bool[]? workingDays = null)
    {
        Size = new Size(690, 654);
        BackColor = AppTheme.Colors.SurfaceWhite;

        var header = BuildUnifiedHeader("Employee information");

        // Body
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Colors.SurfaceWhite
        };

        var avatar = new AvatarBadge
        {
            Initials = initials,
            Size = new Size(70, 70),
            Font = AppTheme.Typography.CreateFont(20f, FontStyle.Bold),
            Location = new Point(32, 26)
        };

        var lblName = new Label
        {
            Text = name,
            Font = AppTheme.Typography.CreateFont(22f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(120, 24)
        };

        var lblCompanyTop = new Label
        {
            Text = company,
            Font = AppTheme.Typography.CreateFont(13f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(122, 66)
        };

        var divider1 = new Panel
        {
            BackColor = AppTheme.Colors.Divider,
            Location = new Point(28, 118),
            Size = new Size(634, 1)
        };

        var lblEmailCaption = new Label
        {
            Text = "Email address",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(28, 136)
        };
        var lblEmail = new Label
        {
            Text = email,
            Font = AppTheme.Typography.CreateFont(13f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(28, 160)
        };

        var lblPhoneCaption = new Label
        {
            Text = "Contact number",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(440, 136)
        };
        var lblPhone = new Label
        {
            Text = phone,
            Font = AppTheme.Typography.CreateFont(13f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(440, 160)
        };

        // Employment details card
        var empCard = new RoundedPanel
        {
            Location = new Point(28, 210),
            Size = new Size(634, 76),
            CornerRadius = AppTheme.Radii.Card,
            BackColor = AppTheme.Colors.SurfaceWhite,
            BorderColor = AppTheme.Colors.SurfaceOffDay
        };
        empCard.Paint += (_, e) =>
        {
            using var pen = new Pen(AppTheme.Colors.Divider, 1f);
            e.Graphics.DrawLine(pen, 392, 12, 392, 64);
        };

        empCard.Controls.Add(new Label
        {
            Text = "Company",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(20, 12)
        });
        empCard.Controls.Add(new Label
        {
            Text = company.ToUpperInvariant(),
            Font = AppTheme.Typography.CreateFont(14f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(20, 36)
        });
        empCard.Controls.Add(new Label
        {
            Text = "Basic salary",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(416, 12)
        });
        empCard.Controls.Add(new Label
        {
            Text = salary,
            Font = AppTheme.Typography.CreateFont(16f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(416, 34)
        });

        // Working days section
        bool[] days = workingDays ?? [false, true, true, true, true, true, false];
        int activeCount = days.Count(d => d);

        body.Controls.Add(new Label
        {
            Text = "Working days",
            Font = AppTheme.Typography.CreateFont(13f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(28, 306)
        });
        body.Controls.Add(new Label
        {
            Text = $"{activeCount} working days",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(550, 310)
        });

        var daysCard = new RoundedPanel
        {
            Location = new Point(28, 338),
            Size = new Size(634, 64),
            CornerRadius = AppTheme.Radii.InputLarge,
            BackColor = AppTheme.Colors.SurfaceWhite,
            BorderColor = AppTheme.Colors.SurfaceCanvas
        };
        var daysStrip = new WorkingDaysStrip
        {
            DisplayMode = WorkingDaysDisplayMode.ExpandedChips,
            WorkingDays = days,
            Location = new Point(14, 10)
        };
        daysCard.Controls.Add(daysStrip);

        // Attendance summary row
        var attendanceCard = new RoundedPanel
        {
            Location = new Point(28, 418),
            Size = new Size(634, 64),
            CornerRadius = AppTheme.Radii.InputLarge,
            BackColor = AppTheme.Colors.SurfaceWhite,
            BorderColor = AppTheme.Colors.SurfaceCanvas
        };
        attendanceCard.Controls.Add(new Label
        {
            Text = "Attendance",
            Font = AppTheme.Typography.CreateFont(13f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextDark,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(16, 10)
        });
        attendanceCard.Controls.Add(new Label
        {
            Text = "November 16-21",
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(16, 36)
        });
        var lnkSeeAll = new LinkLabel
        {
            Text = "See All  →",
            Font = AppTheme.Typography.CreateFont(12f, FontStyle.Bold),
            LinkColor = AppTheme.Colors.Primary,
            ActiveLinkColor = AppTheme.Colors.PrimaryFocus,
            LinkBehavior = LinkBehavior.HoverUnderline,
            AutoSize = true,
            Location = new Point(536, 20)
        };
        lnkSeeAll.LinkClicked += (_, _) =>
        {
            using var attModal = new EmployeeAttendanceModal(initials, name, company);
            attModal.ShowDialog(this);
        };
        attendanceCard.Controls.Add(lnkSeeAll);

        // Bottom outline buttons
        var btnPayroll = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Payroll & Deductions",
            Size = new Size(181, 42),
            Location = new Point(263, 504),
            Font = AppTheme.Typography.CreateFont(11f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.Primary
        };
        var btnPayslips = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Payslips",
            Size = new Size(181, 42),
            Location = new Point(477, 504),
            Font = AppTheme.Typography.CreateFont(11f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.Primary
        };

        body.Controls.Add(avatar);
        body.Controls.Add(lblName);
        body.Controls.Add(lblCompanyTop);
        body.Controls.Add(divider1);
        body.Controls.Add(lblEmailCaption);
        body.Controls.Add(lblEmail);
        body.Controls.Add(lblPhoneCaption);
        body.Controls.Add(lblPhone);
        body.Controls.Add(empCard);
        body.Controls.Add(daysCard);
        body.Controls.Add(attendanceCard);
        body.Controls.Add(btnPayroll);
        body.Controls.Add(btnPayslips);

        Controls.Add(body);
        Controls.Add(header);
    }
}
