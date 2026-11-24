# MCT Payroll & Attendance — Frontend Design System (`DESIGN.md`)

> **Source of Truth:** Figma — *MCT Group of Companies* (`Payroll` → `FINAL UI` & `Design System`)  
> **Target Stack:** C# (.NET 9) + Windows Forms (`MCT.PayrollAttendance`)

---

## 1. Color Palette & Semantic Tokens

### 1.1 Brand & Accent Colors
| Token | Figma Value | Solid / Over-White Equivalent | C# WinForms (`Color`) | Usage |
| :--- | :--- | :--- | :--- | :--- |
| `Primary` | `rgba(0, 164, 255, 0.6)` | `#66C8FF` (over `#FFFFFF`) | `Color.FromArgb(153, 0, 164, 255)` / `Color.FromArgb(102, 200, 255)` | Primary buttons, active nav text, working-day pills, active pagination, headings |
| `PrimarySolid` | `#4DA2FF` | `#4DA2FF` | `Color.FromArgb(77, 162, 255)` | Table headers, outline button borders |
| `PrimaryFocus` | `#00A4FF` | `#00A4FF` | `Color.FromArgb(0, 164, 255)` | Focused input border (2px) |
| `PrimaryDeep` | `#247BA4` | `#247BA4` | `Color.FromArgb(36, 123, 164)` | Focused OTP digit border, inline action links (`Resend code`) |
| `PrimarySoft` | `#6CB6DF` | `#6CB6DF` | `Color.FromArgb(108, 182, 223)` | Modal primary CTA buttons (`Send reset link`, `Verify email`, `Back to sign in`) |
| `Accent` | `rgba(0, 41, 64, 0.6)` / `#121037` | `#667F8D` / `#121037` | `Color.FromArgb(18, 16, 55)` | Dark accent, chip text, disabled button base |

### 1.2 Sidebar & Hero Gradient
Used on the **276px App Sidebar** (`Attendance & Timekeeping`, `Employee Management`), **Login Left Panel** (`728×1024`), and **Employee Profile Card** (`346×200`):
- **Stops (60% alpha over white):**
  1. `rgba(108, 182, 223, 0.6)` → `#A6D4EE`
  2. `rgba(181, 219, 239, 0.6)` → `#D3EAF7`
  3. `rgba(254, 254, 255, 0.6)` → `#FFFFFF`
  4. `rgba(246, 250, 253, 0.6)` → `#FAFCFE`
  5. `rgba(181, 219, 239, 0.6)` → `#D3EAF7`
  6. `rgba(108, 182, 223, 0.6)` → `#A6D4EE`
- **WinForms Note:** Paint using `LinearGradientBrush` in `OnPaint` with `ColorBlend` (vertical `180°` for sidebar; diagonal `~196°–215°` for Login & Profile card).

### 1.3 Neutrals & Surfaces
| Token | Hex | C# WinForms | Usage |
| :--- | :--- | :--- | :--- |
| `SurfaceWhite` | `#FFFFFF` | `Color.White` | Main canvas, cards, modals, table rows |
| `SurfaceMuted` | `#F7F8FA` / `#F7F7F8` | `Color.FromArgb(247, 248, 250)` | Table column headers, modal surface, duration chips, close button bg |
| `SurfaceCanvas` | `#F1F2F5` | `Color.FromArgb(241, 242, 245)` | App/Login outer background, button text on `#6CB6DF` |
| `SurfaceInput` | `#F8FBFD` | `Color.FromArgb(248, 251, 253)` | Login input background |
| `SurfaceOffDay` | `#E8EBF0` | `Color.FromArgb(232, 235, 240)` | Off-day circular badges (`S`) |
| `BorderDefault` | `#E3E5EA` / `#E3E2E8` | `Color.FromArgb(227, 229, 234)` | Cards, modal borders, secondary button borders |
| `BorderInput` | `#C9CCD4` / `#A9BCC8` | `Color.FromArgb(201, 204, 212)` | Default input border (`#A9BCC8` on Login) |
| `Divider` | `#EDF0F5` / `#E8EAED` | `Color.FromArgb(237, 240, 245)` | Table row dividers (1–3px), modal header/section dividers |

### 1.4 Text & Status Colors
| Token | Hex | Usage |
| :--- | :--- | :--- |
| `TextPrimary` | `#0B0F2B` / `#2F2E38` / `#35383B` | Headings, employee names, primary values |
| `TextSecondary` | `#526575` / `#737E95` / `#75738C` / `#777C83` | Field labels, table headers, subtitles, metadata |
| `TextMuted` | `#939DAF` / `#969FB3` / `#9CA3AF` | Placeholders, role subtitles, helper text |
| `TextOnPrimaryDark` | `#082F49` | Login Sign In button label |
| `ErrorBorder` | `#CB4154` | Error input border |
| `ErrorText` | `#D94D4D` | Validation error message |
| `StatusLateBg` / `StatusLateFg` | `rgba(255, 66, 66, 0.1)` / `#FF0004` | `Late` status pill |
| `StatusEarlyBg` / `StatusEarlyFg` | `rgba(55, 143, 118, 0.1)` / `#0E7512` | `Early` status pill |
| `StatusActiveBg` / `StatusActiveFg` | `#EFF8F4` / `#438E77` | `Active` status pill |

---

## 2. Typography

- **Design System Spec Font:** `Plus Jakarta Sans` (with `Roboto Mono` for type metadata/tokens)
- **Final UI Screens Font:** `Afacad` (Regular, Medium, SemiBold, Bold)
- **WinForms Recommendation:** Bundle `Plus Jakarta Sans` and/or `Afacad` via `PrivateFontCollection` from `Resources/Fonts/`, with `Segoe UI` as system fallback if a font file is not yet embedded.

| Style Role | Size | Weight | Line Height | Usage |
| :--- | :--- | :--- | :--- | :--- |
| `DisplayXL` | `64–75px` | SemiBold (600) | Tight | Login hero headline (`Welcome Back!`, `One Workforce.`) |
| `PageTitle` | `48px` | SemiBold (600) | `1.4` | Main screen title (`Attendance`, `Employees`) |
| `ClockDisplay` | `40px` | SemiBold (600) | Normal | Live header clock (`10:53 AM`) |
| `Display` | `34–36px` | Bold (700) | `1.25` | Design system display / Employee modal name (`32px`) |
| `ModalTitle` | `28px` | Bold (700) | `36px` | Recovery modal titles (`Forgot your password?`) |
| `Heading` | `22–24px` | Bold / SemiBold | `1.25` | Section titles, `Employee information` (`22px`) |
| `NavItem` | `18–20px` | Medium (500) | Normal | Sidebar navigation items |
| `Body` | `14–16px` | Regular / Medium | `1.25–1.5` | Table cells (`15px`), inputs (`14px`), modal body (`16px`) |
| `Label` | `12px` | SemiBold (600) | Normal | Form field labels, table sub-labels |
| `Overline / Micro` | `11–11.5px` | SemiBold (600) | `16px` | Modal uppercase context (`ACCOUNT RECOVERY`, `EMPLOYEE ATTENDANCE DETAILS`), tracking `1.2px` |
| `Validation` | `10px` | Bold (700) | Normal | Field error message (`Task Title is Required`) |

---

## 3. Layout & Shell Architecture

### 3.1 Desktop Application Shell (`1440 × 1024` baseline)
- **Left Sidebar (`276px` wide, full height):**
  - Gradient background (`180°` vertical).
  - Navigation items (`24px` icon + `20px` Medium label, `~69px` vertical spacing):
    1. `Dashboard`
    2. `Employee Management`
    3. `Attendance & Timekeeping` (active state uses `#66C8FF` text + `#4DA2FF` underline divider)
    4. `Payroll Processing`
    5. `Deductions & Contributions`
    6. `Payslip Management`
    7. `System Admin`
- **Main Content Area (`1164px` wide, `#FFFFFF` background):**
  - **Top Header:** Left-aligned Page Title (`48px`) + Subtitle (`22.5px` `#969FB3`); Right-aligned Live Clock (`40px`), Date (`15px` `#8390A7`), and `MCT Payroll Management System` (`15px` `#75738C`).
  - **Toolbar (`top: 165px`):** Search input (`975×35px`) + `All Companies` filter dropdown button (`145×36px`).
  - **Data Table (`292px` left offset, `1126–1130px` wide):** `#4DA2FF` (`56px` height) or `#F7F8FA` (`72px` height) header, `65px` rows separated by `3px` `#EDF0F5` dividers.
  - **Pagination Footer (`bottom`):** Left `Showing X of Y staff members` (`13px` `#737E95`), Right `Prev` / `Page 1` / `Next` buttons (`34×29px`, `12px` radius).

### 3.2 Login Screen (`1440 × 1024`)
- **Left Panel (`728 × 1024`):** Diagonal light-blue gradient with hero tagline (`One Workforce. Multiple Companies. One Payroll.`) and 2-line system summary.
- **Right Panel (`616 × 711` card on `#F1F2F5`):** White card containing `Welcome Back!`, subtitle, `Email Address` input (`535×49px`), `Password` input (`535×49px` with eye toggle), and `Sign In` button (`536×49px`).

---

## 4. Reusable Components (`Controls/`)

1. **`AppTextBox` (Input Field)**
   - **Height:** `40px` (standard form) or `49px` (Login) or `52px` (Modal).
   - **Corner Radius:** `8px` (standard) / `10px` (Login & Modals).
   - **States:**
     - *Default:* `1px` `#C9CCD4` (or `#A9BCC8` on Login), bg `#FFFFFF` / `#F8FBFD`.
     - *Focused:* `2px` `#00A4FF`, bg `#FFFFFF`.
     - *Error:* `1px` `#CB4154`, helper label `#D94D4D` `10px` Bold below field.
   - **Optional Slots:** Leading icon (`16px` left), trailing eye-toggle icon (`24×24px` right).

2. **`OtpCodeInput` (6-Digit Verification)**
   - 6 square boxes (`60×60px`, `10px` gap, `10px` radius, `#F7F7F8` bg).
   - *Active/Focused box:* `2px` `#247BA4` border; *Inactive:* `1px` `#E3E2E8` border.

3. **`SearchBar` + `FilterDropdownButton`**
   - Search bar with leading search icon and placeholder (`#98A1B5`).
   - Filter button (`145×36px`, `#66C8FF` bg, `10px` radius, white text + chevron-down icon) or outline filter button (`80×38px`, `#FFFFFF` bg, `1px` `#E8EAED` border).

4. **`AvatarBadge`**
   - Circular `43–46px` (table row) or `67–72px` (modal) badge with initials (`BB`, `JD`) in `#666666` SemiBold, optional `15×15px` green online indicator dot.

5. **`StatusPill` & `DurationChip`**
   - **Status Pill (`94×24px` or `70×21px`, `12px` / `10px` radius):**
     - `Late`: bg `rgba(255,66,66,0.1)`, text `#FF0004`, clock icon.
     - `Early`: bg `rgba(55,143,118,0.1)`, text `#0E7512`, clock icon.
     - `Active`: bg `#EFF8F4`, text `#438E77`, check icon.
   - **Overtime / Work Hours Chip (`78–102 × 21–24px`, `3px` radius):** `#F7F8FA` bg, `#35383B` `12.5–15px` Medium text.

6. **`WorkingDaysStrip`**
   - **Compact (Table Row):** 7 circles (`32×32px`, `16px` radius, `6px` gap) for `S M T W T F S`. Working = `#66C8FF` bg + `#FFFFFF` text; Off = `#E8EBF0` bg + `#8993A5` text.
   - **Expanded (Employee Modal):** Rectangular chips (`99×42px`, `10px` radius, `8px` gap) for `Mon`–`Fri` with `#66C8FF` bg and `#667F8D` text.

7. **`PaginationBar`**
   - Prev/Next buttons (`34×29px`, `12px` radius, `#FFFFFF` bg, `2px` `#E9ECF3` border).
   - Active page button (`34×29px`, `12px` radius, `#66C8FF` bg, `#FFFFFF` `24px` SemiBold text).

---

## 5. Button Specifications

| Variant | Size / Height | Radius | Background | Border | Text Color & Style | Usage |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Primary Pill** | `55px` height, full width | `20px` | `#66C8FF` (`0,164,255 @ 60%`) | `1px` `rgba(151,151,151,0.5)` | `#FFFFFF`, `15px` Bold | Design System primary CTA |
| **Primary Standard** | `44–49px` height | `10px` | `#6CB6DF` (Modals) / `#66C8FF` (Login) | None | `#F1F2F5` / `#082F49`, `14–22px` | `Send reset link`, `Verify email`, `Sign In` |
| **Compact Action** | `30–38px` height | `4–6px` | `#66C8FF` | None / `1px` `#E8EAED` | `#FFFFFF`, `11.5–12.5px` | `View Details`, `Export CSV` |
| **Secondary / Outline** | `38–46px` height | `6–10px` | `#F7F7F8` / `#FFFFFF` | `1px` `#E3E2E8` / `#4DA2FF` | `#2F2E38` / `#777C83` / `#66C8FF` | `Cancel`, `Go back`, `Filter`, `Payslips` |
| **Disabled** | `55px` height | `20px` | `rgba(0,41,64,0.6)` @ `15%` opacity | `1px` `rgba(151,151,151,0.5)` | `#000000` @ `15%` opacity, `15px` Bold | Disabled primary actions |
| **Icon / Close** | `36×36px` | `10px` | `#F7F7F8` | None | `#666666` (`×`) | Modal close button |

---

## 6. Modal & Dialog Specifications (`Forms/Modals/`)

All modals share a centered dialog pattern (`ShowDialog(owner)`), crisp `1px` border (`#E3E2E8` or `#E3E3E8`), drop shadow (`0px 16px 20–24px`), and clean section dividers (`1px` `#E3E3E8` / `#E8EAED`).

1. **Password Recovery Modals (`632 × 775`, `#F7F7F8` bg, `64px` horizontal padding, footer `91px` with `#E3E2E8` top border):**
   - **`ResetPasswordModal`:** Overline `ACCOUNT RECOVERY` → Title `Forgot your password?` → Description → `Email address` field → Helper note → Footer (`Cancel` + `Send reset link`).
   - **`VerifyEmailModal`:** Overline `EMAIL VERIFICATION` → Title `Check your inbox` → Description → `OtpCodeInput` (6 digits) → `Didn't receive it? Resend code` → Footer (`Go back` + `Verify email`).
   - **`PasswordUpdatedModal`:** Overline `ACCOUNT SECURITY` → Title `Password updated` → Description → Bordered security info box (`10px` radius, `16px` padding) → Full-width `Back to sign in` button.

2. **Employee Information Preview (`ViewDetailsModal` — `690 × 660`, `#FFFFFF` bg, `20px` radius):**
   - Header (`70px`): `Employee information` (`22px` SemiBold) + `36×36px` Close (`×`) button.
   - Profile block: `72×67px` Avatar (`BB`), Name (`32px` SemiBold), Company (`MCT Hotel` + icon).
   - Contact row: `Email address` + `Contact number` (`20px` Medium).
   - Employment card (`634×76px`, `12px` radius, `#E8EBF0` border): `Company` (`MCT HOTEL`) | vertical divider | `Basic salary` (`₱20,000`).
   - `Working days` card (`634×64px`, `10px` radius): `Mon`–`Fri` chips + `5 working days`.
   - `Attendance` summary row (`634×64px`, `10px` radius): Date range + `See All →` link.
   - Bottom action buttons (`181×42px`, `10px` radius, `#4DA2FF` border): `Payroll & Deductions`, `Payslips`.

3. **Employee Attendance Details (`EmployeeAttendanceModal` — `774 × 691`, `#FFFFFF` bg, `12px` radius):**
   - Header (`72px`): Pill badge `EMPLOYEE ATTENDANCE DETAILS` (`#F7F8FA` bg, `4px` radius) + Close (`×`).
   - Top bordered card (`730×214px`, `14px` radius):
     - Left (`346×200px` gradient card): Avatar + Online dot, `View Details` button, `EMP12345` + copy icon, Name + `Active` pill, Role.
     - Right: Key-value metadata rows (`Company`, `Role`, `Avg. Work Hours` chip, `Avg. Overtime` chip).
   - `Attendance Summary` (`730×74px`, `5px` radius, 3 equal columns): `Total Presents Days`, `Total Absent Days`, `Total Leave Days`.
   - `Attendance Records`: Month navigator (`March 2025` `<` `>`), `Export CSV` + `Filter` buttons, and 6-column table (`Start Date`, `Clock in - Clock Out`, `Overtime`, `Location`, `Status`, `Action`).

---

## 7. WinForms Implementation Guidelines

- **Theme Centralization:** Place all colors, fonts, spacing, and radii in `src/MCT.PayrollAttendance/Configuration/AppTheme.cs` so every `Form` and `UserControl` references tokens—never hardcode hex values in forms.
- **Custom Controls:** Implement rounded corners, custom borders, and gradient panels using `GraphicsPath` + `SmoothingMode.AntiAlias` inside `Controls/` with `DoubleBuffered = true` to prevent flicker.
- **Base Modal Form:** Create `Forms/Modals/BaseModalForm.cs` (`FormBorderStyle = FormBorderStyle.None`, `StartPosition = FormStartPosition.CenterParent`, `1px` border paint, `Esc` to close) and inherit all 5 modals from it.
- **DPI Awareness:** Keep `ApplicationConfiguration.Initialize()` in `Program.cs` (`PerMonitorV2` DPI) and use `TableLayoutPanel` / `FlowLayoutPanel` / `Dock` / `Anchor` instead of brittle fixed pixel positions where content can resize.
