namespace ir_coding_task_csv_validator.Models;

public record Employee
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string DateOfBirth { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Postcode { get; init; } = string.Empty;
    public string JobCode { get; init; } = string.Empty;

    /// <summary>Resolved from Job_Title_Mappings.csv; null if the code is not found.</summary>
    public string? JobTitle { get; set; }

    public string Salary { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

