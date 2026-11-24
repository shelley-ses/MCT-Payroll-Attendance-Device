using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;
using MCT.PayrollAttendance.Forms.Modals;

namespace MCT.PayrollAttendance.Forms;

/// <summary>
/// Split-screen Login form matching Figma FINAL UI (427:330 — Login).
/// </summary>
public class LoginForm : Form
{
    private readonly AppTextBox _txtEmail;
    private readonly AppTextBox _txtPassword;

    public LoginForm()
    {
        Text = "MCT Payroll Attendance — Sign In";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1440, 900);
        MinimumSize = new Size(1080, 720);
        BackColor = AppTheme.Colors.SurfaceCanvas;
        DoubleBuffered = true;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.5f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49.5f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        // Left Hero Panel (Brand Gradient 215 deg)
        var leftPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            CornerRadius = 0,
            BorderThickness = 0f,
            UseBrandGradient = true,
            GradientAngle = 215f,
            Padding = new Padding(44, 0, 32, 0)
        };

        var heroTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        heroTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        heroTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        heroTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heroTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        var heroStack = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        Color heroDark = Color.FromArgb(68, 68, 68);
        var heroFont = AppTheme.Typography.CreateFont(36f, FontStyle.Bold);

        heroStack.Controls.Add(new Label
        {
            Text = "One Workforce.",
            Font = heroFont,
            ForeColor = heroDark,
            AutoSize = true,
            Margin = new Padding(0)
        });

        heroStack.Controls.Add(new Label
        {
            Text = "Multiple Companies.",
            Font = heroFont,
            ForeColor = heroDark,
            AutoSize = true,
            Margin = new Padding(0)
        });

        heroStack.Controls.Add(new Label
        {
            Text = "One Payroll.",
            Font = heroFont,
            ForeColor = AppTheme.Colors.Primary,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        });

        heroStack.Controls.Add(new Label
        {
            Text = "A centralized platform to manage employee information, attendance,\nsalary computations, and payslips across all MCT companies.",
            Font = AppTheme.Typography.CreateFont(12.5f, FontStyle.Regular),
            ForeColor = heroDark,
            AutoSize = true,
            Margin = new Padding(2, 0, 0, 0)
        });

        heroTable.Controls.Add(heroStack, 0, 1);
        leftPanel.Controls.Add(heroTable);

        // Right Login Card Host (centered via TableLayoutPanel)
        var rightHost = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            BackColor = AppTheme.Colors.SurfaceCanvas
        };
        rightHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        rightHost.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 580f));
        rightHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        rightHost.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        rightHost.RowStyles.Add(new RowStyle(SizeType.Absolute, 640f));
        rightHost.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Colors.SurfaceWhite,
            Padding = new Padding(44, 64, 44, 48)
        };

        var cardLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = AppTheme.Colors.SurfaceWhite
        };
        cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));  // Welcome Back!
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));  // Subtitle
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84f));  // Email
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78f));  // Password
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));  // Forgot link
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // Sign In button

        var lblWelcome = new Label
        {
            Text = "Welcome Back!",
            Font = AppTheme.Typography.CreateFont(44f, FontStyle.Bold),
            ForeColor = AppTheme.Colors.Primary,
            UseCompatibleTextRendering = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        var lblSub = new Label
        {
            Text = "Sign in to continue to MCT Payroll Management System",
            Font = AppTheme.Typography.CreateFont(13f, FontStyle.Regular),
            ForeColor = heroDark,
            UseCompatibleTextRendering = true,
            TextAlign = ContentAlignment.TopCenter,
            Dock = DockStyle.Fill
        };

        _txtEmail = new AppTextBox
        {
            ShowLabel = false,
            PlaceholderText = "Email Address",
            LeadingIcon = InputLeadingIcon.Email,
            BoxFillColor = AppTheme.Colors.SurfaceInput,
            DefaultBorderColor = AppTheme.Colors.BorderLoginInput,
            BoxHeight = 49,
            CornerRadius = AppTheme.Radii.InputLarge,
            InputFont = AppTheme.Typography.CreateFont(13f, FontStyle.Regular),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 0)
        };

        _txtPassword = new AppTextBox
        {
            ShowLabel = false,
            PlaceholderText = "Password",
            LeadingIcon = InputLeadingIcon.Lock,
            ShowPasswordToggle = true,
            UseSystemPasswordChar = true,
            BoxFillColor = AppTheme.Colors.SurfaceInput,
            DefaultBorderColor = AppTheme.Colors.BorderLoginInput,
            BoxHeight = 49,
            CornerRadius = AppTheme.Radii.InputLarge,
            InputFont = AppTheme.Typography.CreateFont(13f, FontStyle.Regular),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 0)
        };

        var forgotPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };
        var lnkForgot = new LinkLabel
        {
            Text = "Forgot password?",
            AutoSize = true,
            Font = AppTheme.Typography.CreateFont(10.5f, FontStyle.Regular),
            LinkColor = AppTheme.Colors.PrimaryDeep,
            ActiveLinkColor = AppTheme.Colors.PrimaryFocus,
            LinkBehavior = LinkBehavior.HoverUnderline,
            Dock = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleRight
        };
        lnkForgot.LinkClicked += (_, _) => RunForgotPasswordFlow();
        forgotPanel.Controls.Add(lnkForgot);

        var signInHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 12, 0, 0),
            BackColor = Color.Transparent
        };
        var btnSignIn = new AppButton
        {
            Variant = AppButtonVariant.LoginPrimary,
            Text = "Sign In",
            Font = AppTheme.Typography.CreateFont(14f, FontStyle.Regular),
            Dock = DockStyle.Top,
            Height = 49
        };
        btnSignIn.Click += (_, _) => HandleSignIn();
        signInHost.Controls.Add(btnSignIn);

        cardLayout.Controls.Add(lblWelcome, 0, 0);
        cardLayout.Controls.Add(lblSub, 0, 1);
        cardLayout.Controls.Add(_txtEmail, 0, 2);
        cardLayout.Controls.Add(_txtPassword, 0, 3);
        cardLayout.Controls.Add(forgotPanel, 0, 4);
        cardLayout.Controls.Add(signInHost, 0, 5);

        card.Controls.Add(cardLayout);
        rightHost.Controls.Add(card, 1, 1);

        layout.Controls.Add(leftPanel, 0, 0);
        layout.Controls.Add(rightHost, 1, 0);
        Controls.Add(layout);

        AcceptButton = btnSignIn;
    }

    private void HandleSignIn()
    {
        bool valid = true;
        if (string.IsNullOrWhiteSpace(_txtEmail.Text))
        {
            _txtEmail.ErrorMessage = "Email Address is required.";
            valid = false;
        }
        else
        {
            _txtEmail.ErrorMessage = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_txtPassword.Text))
        {
            _txtPassword.ErrorMessage = "Password is required.";
            valid = false;
        }
        else
        {
            _txtPassword.ErrorMessage = string.Empty;
        }

        if (!valid)
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void RunForgotPasswordFlow()
    {
        while (true)
        {
            using var resetModal = new ResetPasswordModal();
            if (resetModal.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var verifyModal = new VerifyEmailModal(resetModal.EmailAddress);
            var verifyResult = verifyModal.ShowDialog(this);
            if (verifyResult == DialogResult.Retry)
            {
                continue;
            }

            if (verifyResult != DialogResult.OK)
            {
                return;
            }

            using var updatedModal = new PasswordUpdatedModal();
            updatedModal.ShowDialog(this);
            return;
        }
    }
}
