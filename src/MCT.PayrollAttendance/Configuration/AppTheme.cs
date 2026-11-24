using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MCT.PayrollAttendance.Configuration;

/// <summary>
/// Centralized design tokens for the MCT Payroll Attendance WinForms UI.
/// Derived from docs/DESIGN.md (Figma Design System &amp; FINAL UI).
/// </summary>
public static class AppTheme
{
    public static class Colors
    {
        // Brand & Accent
        public static readonly Color Primary = Color.FromArgb(102, 200, 255);        // rgba(0, 164, 255, 0.6) over white
        public static readonly Color PrimarySolid = Color.FromArgb(77, 162, 255);    // #4DA2FF
        public static readonly Color PrimaryFocus = Color.FromArgb(0, 164, 255);     // #00A4FF
        public static readonly Color PrimaryDeep = Color.FromArgb(36, 123, 164);     // #247BA4
        public static readonly Color PrimarySoft = Color.FromArgb(108, 182, 223);    // #6CB6DF
        public static readonly Color PrimaryHover = Color.FromArgb(84, 184, 245);
        public static readonly Color AccentDark = Color.FromArgb(18, 16, 55);        // #121037
        public static readonly Color AccentChipText = Color.FromArgb(102, 127, 141); // rgba(0, 41, 64, 0.6) over light

        // Surfaces & Neutrals
        public static readonly Color SurfaceWhite = Color.White;                     // #FFFFFF
        public static readonly Color SurfaceMuted = Color.FromArgb(247, 248, 250);   // #F7F8FA
        public static readonly Color SurfaceModal = Color.FromArgb(247, 247, 248);   // #F7F7F8
        public static readonly Color SurfaceCanvas = Color.FromArgb(241, 242, 245);  // #F1F2F5
        public static readonly Color SurfaceInput = Color.FromArgb(248, 251, 253);   // #F8FBFD
        public static readonly Color SurfaceOffDay = Color.FromArgb(232, 235, 240);  // #E8EBF0

        // Borders & Dividers
        public static readonly Color BorderDefault = Color.FromArgb(227, 229, 234);  // #E3E5EA
        public static readonly Color BorderModal = Color.FromArgb(227, 226, 232);    // #E3E2E8
        public static readonly Color BorderInput = Color.FromArgb(201, 204, 212);    // #C9CCD4
        public static readonly Color BorderLoginInput = Color.FromArgb(169, 188, 200); // #A9BCC8
        public static readonly Color BorderSubtle = Color.FromArgb(232, 234, 237);   // #E8EAED
        public static readonly Color Divider = Color.FromArgb(237, 240, 245);        // #EDF0F5
        public static readonly Color BorderPagination = Color.FromArgb(233, 236, 243); // #E9ECF3
        public static readonly Color BorderCheckbox = Color.FromArgb(227, 227, 232);   // #E3E3E8

        // Text
        public static readonly Color TextPrimary = Color.FromArgb(47, 46, 56);       // #2F2E38
        public static readonly Color TextDark = Color.FromArgb(11, 15, 43);          // #0B0F2B
        public static readonly Color TextSecondary = Color.FromArgb(115, 126, 149);  // #737E95
        public static readonly Color TextMuted = Color.FromArgb(147, 157, 175);      // #939DAF
        public static readonly Color TextPlaceholder = Color.FromArgb(152, 161, 181); // #98A1B5
        public static readonly Color TextOnPrimaryDark = Color.FromArgb(8, 47, 73); // #082F49

        // Validation & Status
        public static readonly Color ErrorBorder = Color.FromArgb(203, 65, 84);      // #CB4154
        public static readonly Color ErrorText = Color.FromArgb(217, 77, 77);        // #D94D4D

        public static readonly Color StatusLateBg = Color.FromArgb(255, 236, 236);   // rgba(255, 66, 66, 0.1) over white
        public static readonly Color StatusLateFg = Color.FromArgb(255, 0, 4);       // #FF0004
        public static readonly Color StatusEarlyBg = Color.FromArgb(235, 244, 241);  // rgba(55, 143, 118, 0.1) over white
        public static readonly Color StatusEarlyFg = Color.FromArgb(14, 117, 18);    // #0E7512
        public static readonly Color StatusActiveBg = Color.FromArgb(239, 248, 244); // #EFF8F4
        public static readonly Color StatusActiveFg = Color.FromArgb(67, 142, 119);  // #438E77
        public static readonly Color OnlineDot = Color.FromArgb(34, 197, 94);

        // Gradient Stops (60% alpha blended over white)
        public static readonly Color GradientOuter = Color.FromArgb(166, 212, 238);  // #A6D4EE
        public static readonly Color GradientMid = Color.FromArgb(211, 234, 247);    // #D3EAF7
        public static readonly Color GradientCenter = Color.White;                   // #FFFFFF
    }

    public static class Typography
    {
        public const string PrimaryFontFamily = "Plus Jakarta Sans";
        public const string UiFontFamily = "Afacad";
        public const string FallbackFontFamily = "Segoe UI";

        private static readonly PrivateFontCollection FontCollection = new();
        private static readonly FontFamily? AfacadFamily = LoadEmbeddedAfacad();

        private static FontFamily? LoadEmbeddedAfacad()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                string? resourceName = asm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("Afacad.ttf", StringComparison.OrdinalIgnoreCase));
                if (resourceName is null)
                {
                    return null;
                }

                using Stream? stream = asm.GetManifestResourceStream(resourceName);
                if (stream is null)
                {
                    return null;
                }

                byte[] fontData = new byte[stream.Length];
                stream.ReadExactly(fontData, 0, fontData.Length);

                IntPtr ptr = Marshal.AllocCoTaskMem(fontData.Length);
                try
                {
                    Marshal.Copy(fontData, 0, ptr, fontData.Length);
                    FontCollection.AddMemoryFont(ptr, fontData.Length);
                    return FontCollection.Families.FirstOrDefault();
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
            catch
            {
                return null;
            }
        }

        public static Font CreateFont(float sizePt, FontStyle style = FontStyle.Regular, bool useUiFont = true)
        {
            if (useUiFont && AfacadFamily is not null)
            {
                return new Font(AfacadFamily, sizePt, style, GraphicsUnit.Point);
            }

            string preferred = useUiFont ? UiFontFamily : PrimaryFontFamily;
            try
            {
                return new Font(preferred, sizePt, style, GraphicsUnit.Point);
            }
            catch
            {
                return new Font(FallbackFontFamily, sizePt, style, GraphicsUnit.Point);
            }
        }

        public static Font PageTitle() => CreateFont(32f, FontStyle.Bold);
        public static Font ClockDisplay() => CreateFont(26f, FontStyle.Bold);
        public static Font ModalTitle() => CreateFont(18f, FontStyle.Bold);
        public static Font Heading() => CreateFont(14f, FontStyle.Bold);
        public static Font SubHeading() => CreateFont(12f, FontStyle.Bold);
        public static Font Body() => CreateFont(10.5f, FontStyle.Regular);
        public static Font BodyMedium() => CreateFont(10.5f, FontStyle.Bold);
        public static Font Label() => CreateFont(9f, FontStyle.Bold);
        public static Font Overline() => CreateFont(8.5f, FontStyle.Bold);
        public static Font Validation() => CreateFont(8f, FontStyle.Bold);
    }

    public static class Radii
    {
        public const int Pill = 20;
        public const int Modal = 16;
        public const int Card = 12;
        public const int Input = 8;
        public const int InputLarge = 10;
        public const int Button = 10;
        public const int ButtonCompact = 6;
        public const int Checkbox = 5;
        public const int Chip = 4;
    }

    public static class Spacing
    {
        public const int SidebarWidth = 276;
        public const int HeaderHeight = 124;
        public const int PagePadding = 24;
        public const int SectionGap = 16;
        public const int CardPadding = 20;
        public const int ToolbarHeight = 52;
        public const int TableHeaderHeight = 72;
        public const int TableHeaderCompactHeight = 56;
        public const int TableRowHeight = 65;
        public const int RowDividerThickness = 3;
    }

    /// <summary>
    /// Creates a rounded rectangle GraphicsPath for custom WinForms painting.
    /// </summary>
    public static GraphicsPath CreateRoundedRectPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>
    /// Creates the 6-stop light-blue brand gradient brush used in the sidebar, login hero, and profile card.
    /// </summary>
    public static LinearGradientBrush CreateBrandGradientBrush(Rectangle bounds, float angle = 180f)
    {
        var safeBounds = new Rectangle(
            bounds.X,
            bounds.Y,
            Math.Max(1, bounds.Width),
            Math.Max(1, bounds.Height));

        var brush = new LinearGradientBrush(safeBounds, Colors.GradientOuter, Colors.GradientOuter, angle);
        brush.InterpolationColors = new ColorBlend
        {
            Colors =
            [
                Colors.GradientOuter,
                Colors.GradientMid,
                Colors.GradientCenter,
                Colors.GradientCenter,
                Colors.GradientMid,
                Colors.GradientOuter
            ],
            Positions = [0f, 0.18f, 0.50f, 0.64f, 0.84f, 1f]
        };
        return brush;
    }
}
