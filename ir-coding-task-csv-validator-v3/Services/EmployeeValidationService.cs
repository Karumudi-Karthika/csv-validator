using ir_coding_task_csv_validator.Models;

namespace ir_coding_task_csv_validator.Services;

/// <summary>
/// Applies the five validation rules specified in Instruction.md:
///
///   1. Warning  – first name or last name shorter than 4 characters.
///   2. Error    – job code not found in the mappings file.
///   3. Warning  – salary is not a positive integer.
///   4. Error    – postcode is absent.
///   5. Warning  – postcode does not match the state's expected ranges.
/// </summary>
public class EmployeeValidationService : IEmployeeValidationService
{
    private const int MinNameLength = 4;

    private readonly IPostcodeValidationService _postcodeValidation;

    public EmployeeValidationService(IPostcodeValidationService postcodeValidation)
    {
        _postcodeValidation = postcodeValidation;
    }

    public IReadOnlyList<ValidationMessage> Validate(Employee employee)
    {
        var messages = new List<ValidationMessage>();

        ValidateNames(employee, messages);
        ValidateJobCode(employee, messages);
        ValidateSalary(employee, messages);
        ValidatePostcode(employee, messages);

        return messages;
    }

    // ── Rule 1: name length ────────────────────────────────────────────────────

    private static void ValidateNames(Employee employee, List<ValidationMessage> messages)
    {
        if (employee.FirstName.Length < MinNameLength)
            messages.Add(new ValidationMessage(
                ValidationSeverity.Warning,
                nameof(employee.FirstName),
                $"First name '{employee.FirstName}' is less than {MinNameLength} characters."));

        if (employee.LastName.Length < MinNameLength)
            messages.Add(new ValidationMessage(
                ValidationSeverity.Warning,
                nameof(employee.LastName),
                $"Last name '{employee.LastName}' is less than {MinNameLength} characters."));
    }

    // ── Rule 2: job code must be in the mappings ───────────────────────────────

    private static void ValidateJobCode(Employee employee, List<ValidationMessage> messages)
    {
        if (employee.JobTitle is null)
            messages.Add(new ValidationMessage(
                ValidationSeverity.Error,
                nameof(employee.JobCode),
                $"Job code '{employee.JobCode}' was not found in the job-title mappings."));
    }

    // ── Rule 3: salary must be a positive integer ──────────────────────────────

    private static void ValidateSalary(Employee employee, List<ValidationMessage> messages)
    {
        // Strip common currency symbols / whitespace before parsing
        string raw = employee.Salary.TrimStart('$').Trim();

        bool isPositiveInteger =
            long.TryParse(raw, out long value) && value > 0;

        if (!isPositiveInteger)
            messages.Add(new ValidationMessage(
                ValidationSeverity.Warning,
                nameof(employee.Salary),
                $"Salary '{employee.Salary}' is not a positive integer."));
    }

    // ── Rules 4 & 5: postcode presence and state validity ────────────────────

    private void ValidatePostcode(Employee employee, List<ValidationMessage> messages)
    {
        if (string.IsNullOrWhiteSpace(employee.Postcode))
        {
            messages.Add(new ValidationMessage(
                ValidationSeverity.Error,
                nameof(employee.Postcode),
                "Postcode is missing."));
            return; // No point checking validity if there is no postcode
        }

        if (!_postcodeValidation.IsValid(employee.State, employee.Postcode))
            messages.Add(new ValidationMessage(
                ValidationSeverity.Warning,
                nameof(employee.Postcode),
                $"Postcode '{employee.Postcode}' is not valid for state '{employee.State}'."));
    }
}
