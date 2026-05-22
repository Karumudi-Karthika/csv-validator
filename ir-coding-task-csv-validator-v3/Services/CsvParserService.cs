using ir_coding_task_csv_validator.Models;

namespace ir_coding_task_csv_validator.Services;

/// <summary>
/// RFC 4180-compliant CSV parser that handles quoted fields containing
/// embedded commas and newlines.
/// </summary>
public class CsvParserService : ICsvParserService
{
    // ── Column name constants (lower-cased for case-insensitive header matching) ─
    private const string ColFirstName   = "first name";
    private const string ColLastName    = "last name";
    private const string ColDateOfBirth = "date of birth";
    private const string ColEmail       = "email";
    private const string ColPhone       = "phone number";
    private const string ColCard        = "card number";
    private const string ColState       = "state";
    private const string ColPostcode    = "postcode";
    private const string ColJobCode     = "job_code";
    private const string ColSalary      = "salary";
    private const string ColNotes       = "notes";

    private const string MappingColCode  = "job code";
    private const string MappingColTitle = "job title";

    // ─────────────────────────────────────────────────────────────────────────────

    public IReadOnlyDictionary<string, string> ParseJobTitleMappings(Stream jobTitleCsv)
    {
        var rows = ReadAllRows(jobTitleCsv);
        if (rows.Count < 2)
            return new Dictionary<string, string>();

        var headers = NormaliseHeaders(rows[0]);

        int codeIdx  = RequireColumn(headers, MappingColCode);
        int titleIdx = RequireColumn(headers, MappingColTitle);

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows.Skip(1))
        {
            if (row.Count <= Math.Max(codeIdx, titleIdx)) continue;
            var code  = row[codeIdx].Trim();
            var title = row[titleIdx].Trim();
            if (!string.IsNullOrEmpty(code))
                dict[code] = title;
        }
        return dict;
    }

    public IReadOnlyList<Employee> Parse(
        Stream employeeCsv,
        IReadOnlyDictionary<string, string> jobTitleMappings)
    {
        var rows = ReadAllRows(employeeCsv);
        if (rows.Count < 2)
            return [];

        var headers = NormaliseHeaders(rows[0]);

        int idxFirstName   = RequireColumn(headers, ColFirstName);
        int idxLastName    = RequireColumn(headers, ColLastName);
        int idxDateOfBirth = RequireColumn(headers, ColDateOfBirth);
        int idxEmail       = RequireColumn(headers, ColEmail);
        int idxPhone       = RequireColumn(headers, ColPhone);
        int idxCard        = RequireColumn(headers, ColCard);
        int idxState       = RequireColumn(headers, ColState);
        int idxPostcode    = RequireColumn(headers, ColPostcode);
        int idxJobCode     = RequireColumn(headers, ColJobCode);
        int idxSalary      = RequireColumn(headers, ColSalary);
        int idxNotes       = RequireColumn(headers, ColNotes);

        var employees = new List<Employee>(rows.Count - 1);
        foreach (var row in rows.Skip(1))
        {
            string jobCode = GetField(row, idxJobCode);
            jobTitleMappings.TryGetValue(jobCode, out string? jobTitle);

            employees.Add(new Employee
            {
                FirstName   = GetField(row, idxFirstName),
                LastName    = GetField(row, idxLastName),
                DateOfBirth = GetField(row, idxDateOfBirth),
                Email       = GetField(row, idxEmail),
                PhoneNumber = GetField(row, idxPhone),
                CardNumber  = GetField(row, idxCard),
                State       = GetField(row, idxState),
                Postcode    = GetField(row, idxPostcode),
                JobCode     = jobCode,
                JobTitle    = jobTitle,
                Salary      = GetField(row, idxSalary),
                Notes       = GetField(row, idxNotes),
            });
        }
        return employees;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static string GetField(List<string> row, int index) =>
        index < row.Count ? row[index] : string.Empty;

    private static List<string> NormaliseHeaders(List<string> header) =>
        header.Select(h => h.Trim().ToLowerInvariant()).ToList();

    private static int RequireColumn(List<string> headers, string name)
    {
        int idx = headers.IndexOf(name);
        if (idx < 0)
            throw new InvalidDataException($"Required column '{name}' not found in CSV header.");
        return idx;
    }

    /// <summary>
    /// Reads every logical CSV row (handling multi-line quoted fields) from
    /// <paramref name="stream"/> and returns each row as a list of field strings.
    /// </summary>
    private static List<List<string>> ReadAllRows(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        string? rawContent = reader.ReadToEnd();

        return ParseCsv(rawContent);
    }

    /// <summary>
    /// Minimal but correct RFC 4180 parser.  Handles:
    ///   - quoted fields (doubled-quote escaping)
    ///   - embedded commas within quotes
    ///   - embedded CR/LF within quotes
    ///   - trailing newlines
    /// </summary>
    public static List<List<string>> ParseCsv(string content)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var field = new System.Text.StringBuilder();
        bool inQuotes = false;
        int i = 0;

        while (i < content.Length)
        {
            char c = content[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Peek for doubled-quote escape
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                    }
                    else
                    {
                        inQuotes = false;
                        i++;
                    }
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                    i++;
                }
                else if (c == ',')
                {
                    currentRow.Add(field.ToString());
                    field.Clear();
                    i++;
                }
                else if (c == '\r')
                {
                    // Consume optional following \n
                    currentRow.Add(field.ToString());
                    field.Clear();
                    rows.Add(currentRow);
                    currentRow = [];
                    i++;
                    if (i < content.Length && content[i] == '\n') i++;
                }
                else if (c == '\n')
                {
                    currentRow.Add(field.ToString());
                    field.Clear();
                    rows.Add(currentRow);
                    currentRow = [];
                    i++;
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
        }

        // Flush last field / row
        if (field.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(field.ToString());
            rows.Add(currentRow);
        }

        // Remove completely empty trailing rows (e.g., trailing newline in file)
        while (rows.Count > 0 && rows[^1].All(string.IsNullOrEmpty))
            rows.RemoveAt(rows.Count - 1);

        return rows;
    }
}
