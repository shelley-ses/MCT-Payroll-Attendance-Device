namespace MCT.PayrollAttendance;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        while (true)
        {
            using var loginForm = new Forms.LoginForm();
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                break;
            }

            using var mainForm = new Forms.MainForm();
            Application.Run(mainForm);

            if (!mainForm.LoggedOut)
            {
                break;
            }
        }
    }
}
