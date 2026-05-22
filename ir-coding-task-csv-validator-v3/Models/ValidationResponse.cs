namespace ir_coding_task_csv_validator.Models;

public class ValidationResponse
{
    public int TotalRows { get; init; }
    public int RowsWithErrors { get; init; }
    public int RowsWithWarnings { get; init; }
    public IReadOnlyList<EmployeeValidationResult> Results { get; init; } = [];
}
