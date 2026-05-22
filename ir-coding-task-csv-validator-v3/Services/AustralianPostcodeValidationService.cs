namespace ir_coding_task_csv_validator.Services;

/// <summary>
/// Validates Australian postcodes against official state/territory ranges.
/// Source: https://auspost.com.au/content/dam/auspost_corp/media/documents/postcode-state-mapping.pdf
/// </summary>
public class AustralianPostcodeValidationService : IPostcodeValidationService
{
    // Each state maps to one or more inclusive [min, max] ranges.
    private static readonly IReadOnlyDictionary<string, (int Min, int Max)[]> StateRanges =
        new Dictionary<string, (int, int)[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["NSW"] = [(1000, 1999), (2000, 2599), (2619, 2899), (2921, 2999)],
            ["ACT"] = [(200,  299),  (2600, 2618), (2900, 2920)],
            ["VIC"] = [(3000, 3999), (8000, 8999)],
            ["QLD"] = [(4000, 4999), (9000, 9999)],
            ["SA"]  = [(5000, 5999)],
            ["WA"]  = [(6000, 6797), (6800, 6999)],
            ["TAS"] = [(7000, 7999)],
            ["NT"]  = [(800,  999)],  // NT postcodes are 4 digits: 0800-0999
        };

    public bool IsValid(string state, string postcode)
    {
        if (!int.TryParse(postcode.Trim(), out int code))
            return false;

        if (!StateRanges.TryGetValue(state.Trim(), out var ranges))
            return false; // Unknown state — let caller decide how to handle

        return ranges.Any(r => code >= r.Min && code <= r.Max);
    }
}
