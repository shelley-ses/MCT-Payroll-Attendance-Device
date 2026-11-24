using MCT.PayrollAttendance.Configuration;
using MCT.PayrollAttendance.Controls;

namespace MCT.PayrollAttendance.Tests;

public class ThemeAndControlsTests
{
    [Fact]
    public void AppTheme_ColorsAndTypography_AreConfigured()
    {
        Assert.Equal(Color.FromArgb(77, 162, 255), AppTheme.Colors.PrimarySolid);
        Assert.Equal(Color.FromArgb(203, 65, 84), AppTheme.Colors.ErrorBorder);

        using var font = AppTheme.Typography.Body();
        Assert.NotNull(font);
        Assert.Equal("Afacad", font.FontFamily.Name);
        Assert.True(font.Size > 0);
    }

    [Fact]
    public void AppButton_VariantSetsExpectedDimensionsAndRadius()
    {
        using var pill = new AppButton { Variant = AppButtonVariant.PrimaryPill };
        Assert.Equal(55, pill.Height);
        Assert.Equal(AppTheme.Radii.Pill, pill.CornerRadius);

        using var close = new AppButton { Variant = AppButtonVariant.IconClose };
        Assert.Equal(new Size(36, 36), close.Size);
        Assert.Equal("×", close.Text);
    }

    [Fact]
    public void AppTextBox_TracksErrorState()
    {
        using var input = new AppTextBox();
        Assert.False(input.HasError);

        input.ErrorMessage = "Task Title is Required";
        Assert.True(input.HasError);

        input.ErrorMessage = string.Empty;
        Assert.False(input.HasError);
    }

    [Fact]
    public void OtpCodeInput_FiltersDigitsAndReportsCompletion()
    {
        using var otp = new OtpCodeInput();
        otp.Code = "12a34b567";

        Assert.Equal("123456", otp.Code);
        Assert.True(otp.IsComplete);

        otp.Clear();
        Assert.Equal(string.Empty, otp.Code);
        Assert.False(otp.IsComplete);
    }

    [Fact]
    public void WorkingDaysStrip_UpdatesScheduleArray()
    {
        using var strip = new WorkingDaysStrip();
        strip.SetWorkingDays(true, false, true, false, true, false, false);

        Assert.Equal([true, false, true, false, true, false, false], strip.WorkingDays);
    }

    [Fact]
    public void PaginationBar_ClampsPages()
    {
        using var pagination = new PaginationBar
        {
            TotalPages = 5,
            CurrentPage = 10
        };

        Assert.Equal(5, pagination.CurrentPage);

        pagination.CurrentPage = 0;
        Assert.Equal(1, pagination.CurrentPage);
    }
}
