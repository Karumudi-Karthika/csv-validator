namespace ir_coding_task_csv_validator.Models;

public record ValidationMessage(ValidationSeverity Severity, string Field, string Message);
