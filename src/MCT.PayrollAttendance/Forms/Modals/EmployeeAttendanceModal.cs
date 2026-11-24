using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Employee Attendance Details modal matching Figma FINAL UI (Employee attendance details).
/// </summary>
public class EmployeeAttendanceModal : BaseModalForm
{
    public EmployeeAttendanceModal(
        string initials = "BB",
        string name = "Bernadette Balagot",
        string company = "MCT Hotel",
        string role = "Supervisor",
        string empId = "EMP12345")
    {
        Size = new Size(774, 683);
        BackColor = AppTheme.Colors.SurfaceWhite;

        var header = BuildUnifiedHeader("Employee Attendance Details", asBadge: true);

        // Top Employee Details Card (730x214)
        var detailsCard = new RoundedPanel
        {
            Location = new Point(22, 90),
            Size = new Size(730, 214),
            CornerRadius = 14,
            BackColor = AppTheme.Colors.SurfaceWhite,
            BorderColor = AppTheme.Colors.BorderSubtle
        };

        var profileCard = new RoundedPanel
        {
            Location = new Point(6, 6),
            Size = new Size(346, 200),
            CornerRadius = 9,
            BorderThickness = 0f,
            UseBrandGradient = true,
            GradientAngle = 196f
        };

        var avatar = new AvatarBadge
        {
            Initials = initials,
            ShowOnlineIndicator = true,
            Size = new Size(68, 68),
            Font = AppTheme.Typography.CreateFont(18f, FontStyle.Bold),
            Location = new Point(22, 20)
        };

        var btnViewDetails = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            CornerRadius = AppTheme.Radii.Chip,
            Text = "View Details",
            Size = new Size(92, 30),
            Font = AppTheme.Typography.CreateFont(9f, FontStyle.Bold),
            Location = new Point(236, 16)
        };
        btnViewDetails.Click += (_, _) =>
        {
            using var view = new ViewDetailsModal(initials, name, company);
            view.ShowDialog(this);
        };

        var lblEmpId = new Label
        {
            Text = empId,
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(20, 98)
        };

        var lblEmpName = new Label
        {
            Text = name,
            Font = AppTheme.Typography.CreateFont(13f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(20, 134)
        };

        var activePill = new StatusPill
        {
            Kind = StatusPillKind.Active,
            Size = new Size(70, 21),
            Location = new Point(200, 135)
        };

        var lblRoleSub = new Label
        {
            Text = role,
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(20, 166)
        };

        profileCard.Controls.Add(avatar);
        profileCard.Controls.Add(btnViewDetails);
        profileCard.Controls.Add(lblEmpId);
        profileCard.Controls.Add(lblEmpName);
        profileCard.Controls.Add(activePill);
        profileCard.Controls.Add(lblRoleSub);
        detailsCard.Controls.Add(profileCard);

        AddMetadataRow(detailsCard, 375, 22, "Company", company, false);
        AddMetadataRow(detailsCard, 375, 62, "Role", role, false);
        AddMetadataRow(detailsCard, 375, 102, "Avg. Work Hours", "8hrs 43 mins", true);
        AddMetadataRow(detailsCard, 375, 142, "Avg. Overtime", "1hrs 30mins", true);

        // Attendance Summary (730x107)
        var lblSummaryHeading = new Label
        {
            Text = "Attendance Summary",
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(22, 322)
        };

        var summaryBox = new RoundedPanel
        {
            Location = new Point(22, 348),
            Size = new Size(730, 74),
            CornerRadius = 5,
            BackColor = AppTheme.Colors.SurfaceWhite,
            BorderColor = AppTheme.Colors.BorderSubtle
        };
        summaryBox.Paint += (_, e) =>
        {
            using var pen = new Pen(AppTheme.Colors.BorderSubtle, 1f);
            e.Graphics.DrawLine(pen, 243, 0, 243, 74);
            e.Graphics.DrawLine(pen, 486, 0, 486, 74);
        };
        AddSummaryColumn(summaryBox, 16, "Total Presents Days", "1,298 days");
        AddSummaryColumn(summaryBox, 259, "Total Absent Days", "30 Days");
        AddSummaryColumn(summaryBox, 502, "Total Leave Days", "423 Days");

        // Records toolbar + table
        var lblMonth = new Label
        {
            Text = "March 2025",
            Font = AppTheme.Typography.CreateFont(11.5f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(22, 456)
        };

        var btnExport = new AppButton
        {
            Variant = AppButtonVariant.CompactAction,
            CornerRadius = 6,
            Text = "Export CSV",
            Size = new Size(122, 36),
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold),
            Location = new Point(538, 446)
        };

        var btnFilter = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            CornerRadius = 6,
            Text = "Filter",
            Size = new Size(80, 36),
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Bold),
            Location = new Point(672, 446)
        };

        var grid = new DataGridView
        {
            Location = new Point(22, 496),
            Size = new Size(730, 165),
            BackgroundColor = AppTheme.Colors.SurfaceWhite,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = AppTheme.Colors.Divider,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles = false,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 50 }
        };
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.Colors.PrimarySolid,
            SelectionBackColor = AppTheme.Colors.PrimarySolid,
            ForeColor = Color.White,
            Font = AppTheme.Typography.CreateFont(9f, FontStyle.Bold)
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.Colors.SurfaceWhite,
            ForeColor = AppTheme.Colors.TextPrimary,
            SelectionBackColor = AppTheme.Colors.SurfaceInput,
            SelectionForeColor = AppTheme.Colors.TextPrimary,
            Font = AppTheme.Typography.CreateFont(9.5f, FontStyle.Regular)
        };
        grid.Columns.Add("Date", "Start Date");
        grid.Columns.Add("Clock", "Clock in - Clock Out");
        grid.Columns.Add("Overtime", "Overtime");
        grid.Columns.Add("Location", "Location");
        grid.Columns.Add("Status", "Status");
        grid.Rows.Add("1st May 2025", "7:30 AM - 3:30 PM", "2hrs 30 min", "Withston Street, Ware...", "Late");
        grid.Rows.Add("2nd May 2025", "7:30 AM - 3:30 PM", "-", "Withston Street, Ware...", "Early");

        Controls.Add(header);
        Controls.Add(detailsCard);
        Controls.Add(lblSummaryHeading);
        Controls.Add(summaryBox);
        Controls.Add(lblMonth);
        Controls.Add(btnExport);
        Controls.Add(btnFilter);
        Controls.Add(grid);
    }

    private static void AddMetadataRow(Control parent, int x, int y, string label, string value, bool asChip)
    {
        parent.Controls.Add(new Label
        {
            Text = label,
            Font = AppTheme.Typography.CreateFont(10f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(x, y)
        });

        if (asChip)
        {
            var chip = new StatusPill
            {
                Kind = StatusPillKind.Duration,
                Text = value,
                Size = new Size(106, 24),
                Location = new Point(x + 180, y - 2)
            };
            parent.Controls.Add(chip);
        }
        else
        {
            parent.Controls.Add(new Label
            {
                Text = value,
                Font = AppTheme.Typography.CreateFont(10f, FontStyle.Regular),
                ForeColor = AppTheme.Colors.TextSecondary,
                UseCompatibleTextRendering = true,
                AutoSize = true,
                Location = new Point(x + 180, y)
            });
        }
    }

    private static void AddSummaryColumn(Control parent, int x, string title, string value)
    {
        parent.Controls.Add(new Label
        {
            Text = title,
            Font = AppTheme.Typography.CreateFont(9f, FontStyle.Regular),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(x, 12)
        });
        parent.Controls.Add(new Label
        {
            Text = value,
            Font = AppTheme.Typography.CreateFont(12f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Location = new Point(x, 38)
        });
    }
}
