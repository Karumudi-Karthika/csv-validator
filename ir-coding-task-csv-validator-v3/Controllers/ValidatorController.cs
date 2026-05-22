using ir_coding_task_csv_validator.Models;
using ir_coding_task_csv_validator.Services;
using Microsoft.AspNetCore.Mvc;

namespace ir_coding_task_csv_validator.Controllers;

[ApiController]
[Route("[controller]")]
public class ValidatorController : ControllerBase
{
    private readonly ILogger<ValidatorController> _logger;
    private readonly ICsvParserService _csvParser;
    private readonly IEmployeeValidationService _validator;

    public ValidatorController(
        ILogger<ValidatorController> logger,
        ICsvParserService csvParser,
        IEmployeeValidationService validator)
    {
        _logger    = logger;
        _csvParser  = csvParser;
        _validator  = validator;
    }

    /// <summary>
    /// Parses the uploaded employee CSV, maps job titles from the supplied
    /// job-title-mappings CSV, validates each row and returns the results.
    /// </summary>
    /// <param name="csvFile">Employee data CSV (Sample_Data.csv).</param>
    /// <param name="jobTitleFile">Job-title mappings CSV (Job_Title_Mappings.csv).</param>
    [Route("Validate")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Validate(IFormFile csvFile, IFormFile jobTitleFile)
    {
        if (csvFile is null || csvFile.Length == 0)
            return BadRequest("Employee data CSV file is required.");

        if (jobTitleFile is null || jobTitleFile.Length == 0)
            return BadRequest("Job title mappings CSV file is required.");

        try
        {
            IReadOnlyDictionary<string, string> jobTitleMappings;
            using (var stream = jobTitleFile.OpenReadStream())
                jobTitleMappings = _csvParser.ParseJobTitleMappings(stream);

            IReadOnlyList<Employee> employees;
            using (var stream = csvFile.OpenReadStream())
                employees = _csvParser.Parse(stream, jobTitleMappings);

            var results = employees
                .Select((employee, index) => new EmployeeValidationResult
                {
                    RowNumber = index + 2, // +2: 1-based + skip header row
                    Employee  = employee,
                    Messages  = _validator.Validate(employee),
                })
                .ToList();

            var response = new ValidationResponse
            {
                TotalRows        = results.Count,
                RowsWithErrors   = results.Count(r => r.HasErrors),
                RowsWithWarnings = results.Count(r => r.HasWarnings),
                Results          = results,
            };

            _logger.LogInformation(
                "Validated {Total} rows: {Errors} error(s), {Warnings} warning(s).",
                response.TotalRows, response.RowsWithErrors, response.RowsWithWarnings);

            return Ok(response);
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(ex, "Invalid CSV data supplied.");
            return BadRequest(ex.Message);
        }
    }
}
