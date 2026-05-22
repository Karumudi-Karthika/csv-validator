using ir_coding_task_csv_validator.Models;

namespace ir_coding_task_csv_validator.Services;

public interface IEmployeeValidationService
{
    /// <summary>
    /// Validates a single employee record and returns any warnings/errors.
    /// </summary>
    IReadOnlyList<ValidationMessage> Validate(Employee employee);
}
