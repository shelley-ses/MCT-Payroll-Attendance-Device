namespace MCT.PayrollAttendance.Models;

/// <summary>
/// Core employee domain model used across Employee Management, Employee Details Hub, and Profile Pages.
/// </summary>
public sealed record EmployeeRecord(
    string EmployeeId,
    string Initials,
    string FullName,
    string Company,
    string Department,
    string Position,
    string EmploymentType,
    string Status,
    DateTime DateHired,
    string Email,
    string Phone,
    decimal BasicSalary,
    decimal Allowances,
    bool[] WorkingDays);

public sealed record EmployeeFilterCriteria(
    string Query = "",
    string Company = "All Companies",
    string Department = "All Departments",
    string Status = "All Statuses",
    string EmploymentType = "All Types",
    string PositionQuery = "",
    DateTime? HiredFrom = null,
    DateTime? HiredTo = null);

public sealed record EmployeeSummaryStats(
    int TotalEmployees,
    int ActiveEmployees,
    int InactiveEmployees,
    int RecentlyHired);

public static class EmployeeDirectoryService
{
    public static List<EmployeeRecord> CreateDefaultEmployees() =>
    [
        new("EMP-2026-001", "BB", "Bernadette Balagot", "MCT Hotel", "Operations", "Supervisor", "Regular", "Active", new DateTime(2023, 3, 15), "balagotbernadette@gmail.com", "209 555-0104", 28000m, 4000m, [false, true, true, true, true, true, false]),
        new("EMP-2026-002", "JD", "John Doe", "MCT Hotel", "Operations", "Front Desk Lead", "Regular", "Active", new DateTime(2023, 6, 1), "john.doe@mct.com", "209 555-0118", 24000m, 3000m, [false, true, true, true, true, true, false]),
        new("EMP-2026-003", "MS", "Maria Santos", "MCT Corp", "HR", "HR Specialist", "Regular", "Active", new DateTime(2024, 1, 10), "maria.santos@mct.com", "209 555-0142", 30000m, 4500m, [false, true, true, true, true, true, false]),
        new("EMP-2026-004", "AR", "Angelo Reyes", "MCT Tech", "IT", "IT Administrator", "Regular", "Active", new DateTime(2022, 11, 20), "angelo.reyes@mct.com", "209 555-0177", 42000m, 6000m, [false, true, true, true, true, true, true]),
        new("EMP-2026-005", "LC", "Liza Cruz", "MCT Corp", "Finance", "Payroll Officer", "Regular", "Active", new DateTime(2024, 4, 8), "liza.cruz@mct.com", "209 555-0190", 34000m, 5000m, [false, true, true, true, true, false, false]),
        new("EMP-2026-006", "KT", "Kevin Tan", "MCT Logistics", "Operations", "Warehouse Lead", "Regular", "Active", new DateTime(2023, 9, 12), "kevin.tan@mct.com", "209 555-0211", 26500m, 3500m, [false, true, true, true, true, true, true]),
        new("EMP-2026-007", "RG", "Rafael Garcia", "MCT Hotel", "Operations", "Concierge", "Probationary", "Active", DateTime.Today.AddDays(-25), "rafael.garcia@mct.com", "209 555-0234", 19500m, 2000m, [false, true, true, true, true, true, false]),
        new("EMP-2026-008", "CM", "Camille Mendoza", "MCT Corp", "Finance", "Senior Accountant", "Regular", "Active", new DateTime(2023, 2, 14), "camille.mendoza@mct.com", "209 555-0258", 38000m, 5500m, [false, true, true, true, true, true, false]),
        new("EMP-2026-009", "DL", "Daniel Lim", "MCT Tech", "IT", "Systems Engineer", "Probationary", "Active", DateTime.Today.AddDays(-40), "daniel.lim@mct.com", "209 555-0279", 45000m, 6000m, [false, true, true, true, true, true, false]),
        new("EMP-2026-010", "EV", "Elena Villanueva", "MCT Logistics", "Operations", "Fleet Coordinator", "Contractual", "Inactive", new DateTime(2024, 8, 5), "elena.villanueva@mct.com", "209 555-0302", 23000m, 2500m, [true, true, true, true, true, false, false]),
        new("EMP-2026-011", "JR", "Jerome Ramos", "MCT Hotel", "Operations", "Housekeeping Lead", "Regular", "Resigned", new DateTime(2021, 7, 19), "jerome.ramos@mct.com", "209 555-0319", 22500m, 2500m, [false, true, true, true, true, true, true]),
        new("EMP-2026-012", "NP", "Nicole Pascual", "MCT Corp", "HR", "Recruitment Associate", "Probationary", "Active", DateTime.Today.AddDays(-14), "nicole.pascual@mct.com", "209 555-0345", 24000m, 3000m, [false, true, true, true, true, true, false]),
        new("EMP-2026-013", "MR", "Marco Rivera", "MCT Tech", "IT", "QA Analyst", "Contractual", "Terminated", new DateTime(2024, 5, 11), "marco.rivera@mct.com", "209 555-0378", 29000m, 3000m, [false, true, true, true, true, true, false]),
        new("EMP-2026-014", "AT", "Andrea Torres", "MCT Logistics", "Finance", "Billing Specialist", "Regular", "Active", DateTime.Today.AddDays(-62), "andrea.torres@mct.com", "209 555-0399", 27500m, 3500m, [false, true, true, true, true, true, false])
    ];

    public static IReadOnlyList<EmployeeRecord> Filter(IEnumerable<EmployeeRecord> source, EmployeeFilterCriteria criteria)
    {
        string q = criteria.Query.Trim();
        string pos = criteria.PositionQuery.Trim();

        return source.Where(e =>
            (string.IsNullOrEmpty(q) ||
             e.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             e.EmployeeId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             e.Position.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             e.Email.Contains(q, StringComparison.OrdinalIgnoreCase)) &&
            (criteria.Company == "All Companies" || e.Company.Equals(criteria.Company, StringComparison.OrdinalIgnoreCase)) &&
            (criteria.Department == "All Departments" || e.Department.Equals(criteria.Department, StringComparison.OrdinalIgnoreCase)) &&
            (criteria.Status == "All Statuses" || e.Status.Equals(criteria.Status, StringComparison.OrdinalIgnoreCase)) &&
            (criteria.EmploymentType == "All Types" || e.EmploymentType.Equals(criteria.EmploymentType, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(pos) || e.Position.Contains(pos, StringComparison.OrdinalIgnoreCase)) &&
            (!criteria.HiredFrom.HasValue || e.DateHired.Date >= criteria.HiredFrom.Value.Date) &&
            (!criteria.HiredTo.HasValue || e.DateHired.Date <= criteria.HiredTo.Value.Date))
            .ToList();
    }

    public static EmployeeSummaryStats ComputeSummary(IEnumerable<EmployeeRecord> employees, DateTime? referenceDate = null)
    {
        var list = employees.ToList();
        DateTime refDate = (referenceDate ?? DateTime.Today).Date;
        DateTime recentCutoff = refDate.AddDays(-90);

        int total = list.Count;
        int active = list.Count(e => e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
        int inactive = list.Count(e => !e.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
        int recent = list.Count(e => e.DateHired.Date >= recentCutoff && e.DateHired.Date <= refDate);

        return new EmployeeSummaryStats(total, active, inactive, recent);
    }

    public static string ComputeInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "EM";
        }

        string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0].ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }
}
