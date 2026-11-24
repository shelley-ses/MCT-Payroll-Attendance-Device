using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Forms.Pages;
using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Forms;

public partial class MainForm : Form
{
    private readonly AppSidebar _sidebar;
    private readonly AppHeader _header;
    private readonly Panel _contentHost;
    private EmployeeManagementControl? _employeesControl;

    public bool LoggedOut { get; private set; }

    public MainForm()
    {
        InitializeComponent();

        Size = new Size(1440, 900);
        MinimumSize = new Size(1120, 720);
        BackColor = AppTheme.Colors.SurfaceWhite;
        DoubleBuffered = true;

        _sidebar = new AppSidebar();
        _sidebar.NavigationRequested += (_, key) => NavigateTo(key);
        _sidebar.LogoutRequested += (_, _) =>
        {
            LoggedOut = true;
            Close();
        };

        var rightArea = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Colors.SurfaceWhite
        };

        _header = new AppHeader();

        _contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Colors.SurfaceWhite
        };

        rightArea.Controls.Add(_contentHost);
        rightArea.Controls.Add(_header);

        Controls.Add(rightArea);
        Controls.Add(_sidebar);

        NavigateTo("Dashboard");
    }

    public void NavigateTo(string pageKey)
    {
        _sidebar.SetActivePage(pageKey);
        ClearContentHost();

        switch (pageKey)
        {
            case "Dashboard":
                _header.SetHeader("Dashboard", "Workforce overview, attendance pulse, and payroll status.");
                var dashboard = new DashboardControl();
                dashboard.NavigateRequested += (_, target) => NavigateTo(target);
                _contentHost.Controls.Add(dashboard);
                break;

            case "Employees":
                _header.SetHeader("MCT Staff", "Manage employee profiles, attendance, and salary information.");
                _employeesControl ??= CreateEmployeesControl();
                _contentHost.Controls.Add(_employeesControl);
                break;

            case "Attendance":
                _header.SetHeader("Attendance", "Manage employees attendance.");
                _contentHost.Controls.Add(new AttendanceTimekeepingControl());
                break;

            case "Payroll":
                _header.SetHeader("Payroll Processing", "Calculate, review, and approve company payroll batches.");
                _contentHost.Controls.Add(new PayrollProcessingControl());
                break;

            case "Deductions":
                _header.SetHeader("Deductions", "Statutory contributions, withholding tax, and employee loans.");
                _contentHost.Controls.Add(new DeductionsControl());
                break;

            default:
                _header.SetHeader(pageKey, $"{pageKey} module is coming soon.");
                var placeholder = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = AppTheme.Typography.Heading(),
                    ForeColor = AppTheme.Colors.TextMuted,
                    Text = $"{pageKey} — Ready for implementation"
                };
                _contentHost.Controls.Add(placeholder);
                break;
        }
    }

    private EmployeeManagementControl CreateEmployeesControl()
    {
        var ctrl = new EmployeeManagementControl();
        ctrl.EmployeePageRequested += (_, args) => OpenEmployeeProfile(args.Employee, args.DestinationKey);
        return ctrl;
    }

    private void OpenEmployeeProfile(EmployeeRecord employee, string destinationKey)
    {
        _sidebar.SetActivePage("Employees");
        _header.SetHeader(employee.FullName, $"Employee Profile · {employee.EmployeeId} · {employee.Company}");
        ClearContentHost();

        var host = new EmployeeProfileHostControl(employee, destinationKey);
        host.BackRequested += (_, _) => NavigateTo("Employees");
        _contentHost.Controls.Add(host);
    }

    private void ClearContentHost()
    {
        foreach (Control ctrl in _contentHost.Controls)
        {
            if (!ReferenceEquals(ctrl, _employeesControl))
            {
                ctrl.Dispose();
            }
        }
        _contentHost.Controls.Clear();
    }
}
