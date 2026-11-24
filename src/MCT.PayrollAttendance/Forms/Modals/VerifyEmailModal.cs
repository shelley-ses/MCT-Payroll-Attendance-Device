using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Step 2 of password recovery: 6-digit email verification modal using the unified modal UI.
/// </summary>
public class VerifyEmailModal : BaseModalForm
{
    private readonly OtpCodeInput _otpInput;
    private readonly Label _lblError;

    public VerifyEmailModal(string email = "Johndoe@mct.com")
    {
        Size = new Size(560, 500);

        var header = BuildUnifiedHeader("Email Verification", asBadge: true);

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
            Text = "Check your inbox",
            Font = AppTheme.Typography.ModalTitle(),
            ForeColor = AppTheme.Colors.Primary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblDescription = new Label
        {
            Text = $"Enter the 6-digit verification code sent to {email}.",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            MaximumSize = new Size(460, 0),
            Margin = new Padding(0, 0, 0, 22)
        };

        _otpInput = new OtpCodeInput
        {
            Margin = new Padding(26, 0, 0, 12)
        };

        _lblError = new Label
        {
            Text = string.Empty,
            Font = AppTheme.Typography.Validation(),
            ForeColor = AppTheme.Colors.ErrorText,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            MaximumSize = new Size(460, 0),
            Margin = new Padding(0, 0, 0, 8),
            Visible = false
        };

        var resendPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 4, 0, 0),
            BackColor = Color.Transparent
        };
        var lblResendPrompt = new Label
        {
            AutoSize = true,
            Text = "Didn’t receive it?",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true
        };
        var lnkResend = new LinkLabel
        {
            AutoSize = true,
            Text = "Resend code",
            Font = AppTheme.Typography.Label(),
            LinkColor = AppTheme.Colors.PrimaryDeep,
            ActiveLinkColor = AppTheme.Colors.PrimaryFocus,
            LinkBehavior = LinkBehavior.HoverUnderline
        };
        lnkResend.LinkClicked += (_, _) =>
        {
            _otpInput.Clear();
            _lblError.Visible = false;
        };
        resendPanel.Controls.Add(lblResendPrompt);
        resendPanel.Controls.Add(lnkResend);

        body.Controls.Add(lblTitle);
        body.Controls.Add(lblDescription);
        body.Controls.Add(_otpInput);
        body.Controls.Add(_lblError);
        body.Controls.Add(resendPanel);

        var btnBack = new AppButton
        {
            Variant = AppButtonVariant.SecondaryOutline,
            Text = "Go back",
            Size = new Size(106, 44),
            Margin = new Padding(0, 0, 12, 0)
        };
        btnBack.Click += (_, _) =>
        {
            DialogResult = DialogResult.Retry;
            Close();
        };

        var btnVerify = new AppButton
        {
            Variant = AppButtonVariant.PrimaryStandard,
            Text = "Verify email",
            Size = new Size(134, 44)
        };
        btnVerify.Click += (_, _) =>
        {
            if (!_otpInput.IsComplete)
            {
                _lblError.Text = "Please enter all 6 digits of the verification code.";
                _lblError.Visible = true;
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        };

        var footer = BuildUnifiedFooter(btnBack, btnVerify);

        Controls.Add(body);
        Controls.Add(header);
        Controls.Add(footer);
    }

    public string VerificationCode => _otpInput.Code;
}
