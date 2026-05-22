using Xunit;
using FluentAssertions;
using ir_coding_task_csv_validator.Models;
using ir_coding_task_csv_validator.Services;

namespace ir_coding_task_csv_validator.Tests;

public class EmployeeValidationServiceTests
{
    private readonly EmployeeValidationService _sut =
        new(new AustralianPostcodeValidationService());

    // ── Helper to build a valid baseline employee ─────────────────────────────

    private static Employee ValidEmployee() => new()
    {
        FirstName   = "Alice",
        LastName    = "Smith",
        DateOfBirth = "1990-01-01",
        Email       = "alice@example.com",
        PhoneNumber = "0400000000",
        CardNumber  = "1",
        State       = "NSW",
        Postcode    = "2000",
        JobCode     = "3",
        JobTitle    = "Operator",   // already resolved
        Salary      = "50000",
        Notes       = "",
    };

    // ── Rule 1: name length ────────────────────────────────────────────────────

    [Theory]
    [InlineData("Al",    "Smith",  "FirstName")]
    [InlineData("Alice", "Li",     "LastName")]
    [InlineData("Al",    "Li",     "FirstName")]   // triggers two warnings
    public void Validate_WhenNameTooShort_ProducesWarning(string first, string last, string expectedField)
    {
        var emp = ValidEmployee() with { FirstName = first, LastName = last };
        var messages = _sut.Validate(emp);
        messages.Should().Contain(m =>
            m.Severity == ValidationSeverity.Warning &&
            m.Field == expectedField);
    }

    [Fact]
    public void Validate_WhenBothNamesTooShort_ProducesTwoWarnings()
    {
        var emp = ValidEmployee() with { FirstName = "Al", LastName = "Li" };
        var messages = _sut.Validate(emp);
        messages.Count(m => m.Severity == ValidationSeverity.Warning && m.Field.Contains("Name"))
                .Should().Be(2);
    }

    [Fact]
    public void Validate_WhenNamesLongEnough_NoNameWarning()
    {
        var messages = _sut.Validate(ValidEmployee());
        messages.Should().NotContain(m => m.Field.Contains("Name"));
    }

    // ── Rule 2: job code in mappings ──────────────────────────────────────────

    [Fact]
    public void Validate_WhenJobCodeNotFound_ProducesError()
    {
        var emp = ValidEmployee() with { JobTitle = null };
        var messages = _sut.Validate(emp);
        messages.Should().Contain(m =>
            m.Severity == ValidationSeverity.Error &&
            m.Field == "JobCode");
    }

    [Fact]
    public void Validate_WhenJobCodeFound_NoError()
    {
        var messages = _sut.Validate(ValidEmployee());
        messages.Should().NotContain(m => m.Field == "JobCode");
    }

    // ── Rule 3: salary must be a positive integer ─────────────────────────────

    [Theory]
    [InlineData("$620.62")]   // decimal
    [InlineData("0")]         // zero
    [InlineData("-5000")]     // negative
    [InlineData("abc")]       // non-numeric
    [InlineData("")]          // empty
    public void Validate_WhenSalaryNotPositiveInteger_ProducesWarning(string salary)
    {
        var emp = ValidEmployee() with { Salary = salary };
        var messages = _sut.Validate(emp);
        messages.Should().Contain(m =>
            m.Severity == ValidationSeverity.Warning &&
            m.Field == "Salary");
    }

    [Theory]
    [InlineData("50000")]
    [InlineData("1")]
    [InlineData("$75000")]   // currency symbol stripped
    public void Validate_WhenSalaryIsPositiveInteger_NoSalaryWarning(string salary)
    {
        var emp = ValidEmployee() with { Salary = salary };
        var messages = _sut.Validate(emp);
        messages.Should().NotContain(m => m.Field == "Salary");
    }

    // ── Rule 4: postcode must exist ───────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenPostcodeMissing_ProducesError(string postcode)
    {
        var emp = ValidEmployee() with { Postcode = postcode };
        var messages = _sut.Validate(emp);
        messages.Should().Contain(m =>
            m.Severity == ValidationSeverity.Error &&
            m.Field == "Postcode");
    }

    // ── Rule 5: postcode valid for state ──────────────────────────────────────

    [Theory]
    [InlineData("NSW", "4473")]   // QLD range in NSW
    [InlineData("VIC", "2000")]   // NSW range in VIC
    [InlineData("QLD", "3000")]   // VIC range in QLD
    public void Validate_WhenPostcodeWrongForState_ProducesWarning(string state, string postcode)
    {
        var emp = ValidEmployee() with { State = state, Postcode = postcode };
        var messages = _sut.Validate(emp);
        messages.Should().Contain(m =>
            m.Severity == ValidationSeverity.Warning &&
            m.Field == "Postcode");
    }

    [Theory]
    [InlineData("NSW", "2000")]
    [InlineData("VIC", "3000")]
    [InlineData("QLD", "4000")]
    [InlineData("SA",  "5000")]
    [InlineData("WA",  "6000")]
    [InlineData("TAS", "7000")]
    [InlineData("NT",  "800")]
    [InlineData("ACT", "2600")]
    public void Validate_WhenPostcodeMatchesState_NoPostcodeWarning(string state, string postcode)
    {
        var emp = ValidEmployee() with { State = state, Postcode = postcode };
        var messages = _sut.Validate(emp);
        messages.Should().NotContain(m =>
            m.Severity == ValidationSeverity.Warning &&
            m.Field == "Postcode");
    }
}
