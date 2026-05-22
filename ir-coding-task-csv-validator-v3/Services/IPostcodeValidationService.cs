namespace ir_coding_task_csv_validator.Services;

public interface IPostcodeValidationService
{
    /// <summary>
    /// Returns true when <paramref name="postcode"/> falls within the
    /// accepted range(s) for <paramref name="state"/>.
    /// </summary>
    bool IsValid(string state, string postcode);
}
