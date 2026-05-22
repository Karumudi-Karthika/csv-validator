namespace ir_coding_task_csv_validator.Models;

public class EmployeeValidationResult
{
    public int RowNumber { get; init; }
    public Employee Employee { get; init; } = null!;
    public IReadOnlyList<ValidationMessage> Messages { get; init; } = [];

    public bool HasErrors => Messages.Any(m => m.Severity == ValidationSeverity.Error);
    public bool HasWarnings => Messages.Any(m => m.Severity == ValidationSeverity.Warning);
}
