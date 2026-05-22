using ir_coding_task_csv_validator.Models;

namespace ir_coding_task_csv_validator.Services;

public interface ICsvParserService
{
    /// <summary>
    /// Parses employee records from a CSV stream and populates JobTitle
    /// from the provided job-title mapping dictionary.
    /// </summary>
    /// <param name="employeeCsv">Stream of the employee data CSV.</param>
    /// <param name="jobTitleMappings">Dictionary keyed by job code (string) → job title.</param>
    /// <returns>Ordered list of parsed employees, in file order.</returns>
    IReadOnlyList<Employee> Parse(Stream employeeCsv, IReadOnlyDictionary<string, string> jobTitleMappings);

    /// <summary>
    /// Parses job-title mappings from a CSV stream.
    /// </summary>
    /// <returns>Dictionary keyed by job code (string) → job title.</returns>
    IReadOnlyDictionary<string, string> ParseJobTitleMappings(Stream jobTitleCsv);
}
