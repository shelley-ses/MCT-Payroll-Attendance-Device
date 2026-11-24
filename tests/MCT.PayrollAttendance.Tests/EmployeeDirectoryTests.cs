using MCT.PayrollAttendance.Models;

namespace MCT.PayrollAttendance.Tests;

public class EmployeeDirectoryTests
{
    [Fact]
    public void ComputeSummary_ReturnsAccurateCounts()
    {
        var refDate = new DateTime(2026, 5, 1);
        var employees = new List<EmployeeRecord>
        {
            new("EMP-1", "AA", "Alice", "MCT Hotel", "Operations", "Lead", "Regular", "Active", refDate.AddDays(-10), "a@mct.com", "111", 25000m, 2000m, new bool[7]),
            new("EMP-2", "BB", "Bob", "MCT Corp", "HR", "Specialist", "Probationary", "Active", refDate.AddDays(-120), "b@mct.com", "222", 25000m, 2000m, new bool[7]),
            new("EMP-3", "CC", "Charlie", "MCT Tech", "IT", "Engineer", "Contractual", "Inactive", refDate.AddDays(-30), "c@mct.com", "333", 25000m, 2000m, new bool[7]),
            new("EMP-4", "DD", "Diana", "MCT Logistics", "Finance", "Analyst", "Regular", "Resigned", refDate.AddDays(-400), "d@mct.com", "444", 25000m, 2000m, new bool[7])
        };

        var summary = EmployeeDirectoryService.ComputeSummary(employees, refDate);

        Assert.Equal(4, summary.TotalEmployees);
        Assert.Equal(2, summary.ActiveEmployees);
        Assert.Equal(2, summary.InactiveEmployees);
        Assert.Equal(2, summary.RecentlyHired);
    }

    [Fact]
    public void Filter_AppliesSearchCompanyDepartmentStatusTypeAndAdvancedCriteria()
    {
        var employees = EmployeeDirectoryService.CreateDefaultEmployees();

        var byId = EmployeeDirectoryService.Filter(employees, new EmployeeFilterCriteria(Query: "EMP-2026-004"));
        Assert.Single(byId);
        Assert.Equal("Angelo Reyes", byId[0].FullName);

        var byCompanyAndDept = EmployeeDirectoryService.Filter(employees, new EmployeeFilterCriteria(
            Company: "MCT Corp",
            Department: "Finance",
            Status: "Active",
            EmploymentType: "Regular"));
        Assert.Equal(2, byCompanyAndDept.Count);

        var byPosition = EmployeeDirectoryService.Filter(employees, new EmployeeFilterCriteria(PositionQuery: "Engineer"));
        Assert.Single(byPosition);
        Assert.Equal("Daniel Lim", byPosition[0].FullName);
    }
}
