using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Step 1 of password recovery: collects the user's email address using the unified modal UI.
/// </summary>
public class ResetPasswordModal : BaseModalForm
{
    private readonly AppTextBox _txtEmail;

    public ResetPasswordModal()
    {
        Size = new Size(560, 480);

        var header = BuildUnifiedHeader("Account Recovery", asBadge: true);

        var body = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(48, 32, 48, 24),
            BackColor = AppTheme.Colors.SurfaceWhite
        };

        var lblTitle = new Label
        {
            Text = "Forgot your password?",
            Font = AppTheme.Typography.ModalTitle(),
            ForeColor = AppTheme.Colors.Primary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblDescription = new Label
        {
            Text = "Enter the email address linked to your account. We’ll send you a link to reset your password.",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            MaximumSize = new Size(460, 0),
            Margin = new Padding(0, 0, 0, 22)
        };

        _txtEmail = new AppTextBox
        {
            LabelText = "Email address",
            PlaceholderText = "Johndoe@mct.com",
            CornerRadius = AppTheme.Radii.InputLarge,
            Width = 460,
            Height = 82,
            Margin = new Padding(0, 0, 0, 12)
        };

        var lblGuidance = new Label
        {
            Text = "Look for an email from us in the next few minutes.",
            Font = AppTheme.Typography.Label(),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            MaximumSize = new Size(460, 0),
            Margin = new Padding(0)
        };

        body.Controls.Add(lblTitle);
        body.Controls.Add(lblDescription);
        body.Controls.Add(_txtEmail);
        body.Controls.Add(lblGuidance);

        var btnCancel = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Cancel",
            Size = new Size(100, 44),
            Margin = new Padding(0, 0, 12, 0)
        };
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var btnSend = new AppButton
        {
            Variant = AppButtonVariant.PrimaryStandard,
            Text = "Send reset link",
            Size = new Size(154, 44)
        };
        btnSend.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtEmail.Text) || !_txtEmail.Text.Contains('@'))
            {
                _txtEmail.ErrorMessage = "Please enter a valid email address.";
                return;
            }

            _txtEmail.ErrorMessage = string.Empty;
            DialogResult = DialogResult.OK;
            Close();
        };

        var footer = BuildUnifiedFooter(btnCancel, btnSend);

        Controls.Add(body);
        Controls.Add(header);
        Controls.Add(footer);
    }

    public string EmailAddress => _txtEmail.Text.Trim();
}
