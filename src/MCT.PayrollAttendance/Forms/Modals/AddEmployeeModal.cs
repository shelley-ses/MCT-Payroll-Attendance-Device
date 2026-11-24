using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Modal dialog for adding a new employee record to the directory.
/// </summary>
public class AddEmployeeModal : BaseModalForm
{
    private readonly AppTextBox _txtFullName;
    private readonly AppTextBox _txtPosition;
    private readonly AppTextBox _txtEmail;
    private readonly AppTextBox _txtPhone;
    private readonly AppTextBox _txtSalary;
    private readonly ComboBox _cmbCompany;
    private readonly ComboBox _cmbDepartment;
    private readonly ComboBox _cmbEmploymentType;
    private readonly ComboBox _cmbStatus;
    private readonly DateTimePicker _dtpDateHired;
    private readonly string _nextEmployeeId;

    public EmployeeRecord? CreatedEmployee { get; private set; }

    public AddEmployeeModal(string nextEmployeeId)
    {
        _nextEmployeeId = nextEmployeeId;
        Size = new Size(640, 700);
        MinimumSize = new Size(540, 540);
        BackColor = AppTheme.Colors.SurfaceWhite;

        var header = BuildUnifiedHeader("Add Employee", asBadge: false);

        var btnCancel = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Cancel",
            Size = new Size(110, 42)
        };
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var btnSave = new AppButton
        {
            Variant = AppButtonVariant.PrimaryStandard,
            Text = "Save Employee",
            Size = new Size(150, 42),
            Margin = new Padding(12, 0, 0, 0)
        };
        btnSave.Click += (_, _) => HandleSave();

        var footer = BuildUnifiedFooter(btnCancel, btnSave);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(24, 16, 24, 16)
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        _txtFullName = CreateField("Full Name", "e.g. Juan Dela Cruz");
        _txtPosition = CreateField("Position", "e.g. Operations Supervisor");
        _txtEmail = CreateField("Email Address", "juan.delacruz@mct.com");
        _txtPhone = CreateField("Contact Number", "209 555-0188");
        _txtSalary = CreateField("Basic Salary (PHP)", "25000");

        _cmbCompany = CreateDropdown(["MCT Hotel", "MCT Corp", "MCT Tech", "MCT Logistics"]);
        _cmbDepartment = CreateDropdown(["Operations", "HR", "IT", "Finance"]);
        _cmbEmploymentType = CreateDropdown(["Regular", "Probationary", "Contractual"]);
        _cmbStatus = CreateDropdown(["Active", "Inactive"]);

        _dtpDateHired = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Font = AppTheme.Typography.Body(),
            Dock = DockStyle.Fill,
            Value = DateTime.Today
        };

        grid.Controls.Add(_txtFullName, 0, 0);
        grid.Controls.Add(_txtPosition, 1, 0);
        grid.Controls.Add(_txtEmail, 0, 1);
        grid.Controls.Add(_txtPhone, 1, 1);
        grid.Controls.Add(WrapLabeled("Company", _cmbCompany), 0, 2);
        grid.Controls.Add(WrapLabeled("Department", _cmbDepartment), 1, 2);
        grid.Controls.Add(WrapLabeled("Employment Type", _cmbEmploymentType), 0, 3);
        grid.Controls.Add(WrapLabeled("Status", _cmbStatus), 1, 3);
        grid.Controls.Add(_txtSalary, 0, 4);
        grid.Controls.Add(WrapLabeled("Date Hired", _dtpDateHired), 1, 4);

        body.Controls.Add(grid);

        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private static AppTextBox CreateField(string label, string placeholder) => new()
    {
        LabelText = label,
        PlaceholderText = placeholder,
        Dock = DockStyle.Fill,
        Height = 76,
        Margin = new Padding(6, 4, 6, 4)
    };

    private static ComboBox CreateDropdown(string[] items)
    {
        var cmb = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = AppTheme.Typography.Body(),
            BackColor = AppTheme.Colors.SurfaceWhite,
            ForeColor = AppTheme.Colors.TextDark,
            Dock = DockStyle.Fill
        };
        cmb.Items.AddRange(items);
        if (cmb.Items.Count > 0)
        {
            cmb.SelectedIndex = 0;
        }
        return cmb;
    }

    private static Control WrapLabeled(string label, Control input)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Height = 72,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(6, 4, 6, 4),
            BackColor = Color.Transparent
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        panel.Controls.Add(new Label
        {
            Text = label,
            Font = AppTheme.Typography.Label(),
            ForeColor = AppTheme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        }, 0, 0);
        panel.Controls.Add(input, 0, 1);
        return panel;
    }

    private void HandleSave()
    {
        bool valid = true;
        if (string.IsNullOrWhiteSpace(_txtFullName.Text))
        {
            _txtFullName.ErrorMessage = "Full Name is required.";
            valid = false;
        }
        else
        {
            _txtFullName.ErrorMessage = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_txtPosition.Text))
        {
            _txtPosition.ErrorMessage = "Position is required.";
            valid = false;
        }
        else
        {
            _txtPosition.ErrorMessage = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_txtEmail.Text) || !_txtEmail.Text.Contains('@'))
        {
            _txtEmail.ErrorMessage = "Valid email is required.";
            valid = false;
        }
        else
        {
            _txtEmail.ErrorMessage = string.Empty;
        }

        if (!decimal.TryParse(_txtSalary.Text.Trim(), out decimal salary) || salary <= 0)
        {
            _txtSalary.ErrorMessage = "Enter a valid positive salary.";
            valid = false;
        }
        else
        {
            _txtSalary.ErrorMessage = string.Empty;
        }

        if (!valid)
        {
            return;
        }

        string name = _txtFullName.Text.Trim();
        CreatedEmployee = new EmployeeRecord(
            EmployeeId: _nextEmployeeId,
            Initials: EmployeeDirectoryService.ComputeInitials(name),
            FullName: name,
            Company: _cmbCompany.SelectedItem?.ToString() ?? "MCT Hotel",
            Department: _cmbDepartment.SelectedItem?.ToString() ?? "Operations",
            Position: _txtPosition.Text.Trim(),
            EmploymentType: _cmbEmploymentType.SelectedItem?.ToString() ?? "Regular",
            Status: _cmbStatus.SelectedItem?.ToString() ?? "Active",
            DateHired: _dtpDateHired.Value.Date,
            Email: _txtEmail.Text.Trim(),
            Phone: string.IsNullOrWhiteSpace(_txtPhone.Text) ? "209 555-0100" : _txtPhone.Text.Trim(),
            BasicSalary: salary,
            Allowances: 3000m,
            WorkingDays: [false, true, true, true, true, true, false]);

        DialogResult = DialogResult.OK;
        Close();
    }
}
