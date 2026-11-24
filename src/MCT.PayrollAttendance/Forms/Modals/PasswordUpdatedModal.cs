using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Forms.Modals;

/// <summary>
/// Step 3 of password recovery: confirmation modal returning user to Sign In using the unified modal UI.
/// </summary>
public class PasswordUpdatedModal : BaseModalForm
{
    public PasswordUpdatedModal()
    {
        Size = new Size(560, 480);

        var header = BuildUnifiedHeader("Account Security", asBadge: true);

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
            Text = "Password updated",
            Font = AppTheme.Typography.ModalTitle(),
            ForeColor = AppTheme.Colors.Primary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        var lblDescription = new Label
        {
            Text = "Your new password is ready to use. You can now sign in securely with your updated details.",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            MaximumSize = new Size(460, 0),
            Margin = new Padding(0, 0, 0, 24)
        };

        var securityCard = new RoundedPanel
        {
            Width = 460,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(460, 92),
            CornerRadius = AppTheme.Radii.InputLarge,
            BackColor = AppTheme.Colors.SurfaceMuted,
            BorderColor = AppTheme.Colors.BorderDefault,
            Padding = new Padding(16)
        };

        var cardStack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        var lblSecTitle = new Label
        {
            Text = "Your account stays protected",
            Font = AppTheme.Typography.SubHeading(),
            ForeColor = AppTheme.Colors.TextPrimary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };

        var lblSecDetail = new Label
        {
            Text = "For your security, use a unique password that you don’t use anywhere else.",
            Font = AppTheme.Typography.Body(),
            ForeColor = AppTheme.Colors.TextSecondary,
            UseCompatibleTextRendering = true,
            AutoSize = true,
            MaximumSize = new Size(424, 0)
        };

        cardStack.Controls.Add(lblSecTitle);
        cardStack.Controls.Add(lblSecDetail);
        securityCard.Controls.Add(cardStack);

        body.Controls.Add(lblTitle);
        body.Controls.Add(lblDescription);
        body.Controls.Add(securityCard);

        var btnBackToSignIn = new AppButton
        {
            Variant = AppButtonVariant.PrimaryStandard,
            Text = "Back to sign in",
            Size = new Size(200, 44)
        };
        btnBackToSignIn.Click += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        var footer = BuildUnifiedFooter(btnBackToSignIn);

        Controls.Add(body);
        Controls.Add(header);
        Controls.Add(footer);
    }
}
