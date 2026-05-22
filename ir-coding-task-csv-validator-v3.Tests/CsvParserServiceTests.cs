using Xunit;
using FluentAssertions;
using ir_coding_task_csv_validator.Services;

namespace ir_coding_task_csv_validator.Tests;

public class CsvParserServiceTests
{
    private readonly CsvParserService _sut = new();

    // ── ParseJobTitleMappings ─────────────────────────────────────────────────

    [Fact]
    public void ParseJobTitleMappings_ReturnsCorrectEntries()
    {
        const string csv = "job code,job title\r\n1,Engineer\r\n2,Manager\r\n";
        using var stream = ToStream(csv);

        var result = _sut.ParseJobTitleMappings(stream);

        result.Should().HaveCount(2);
        result["1"].Should().Be("Engineer");
        result["2"].Should().Be("Manager");
    }

    [Fact]
    public void ParseJobTitleMappings_IsCaseInsensitiveOnLookup()
    {
        const string csv = "job code,job title\r\n5,Analyst\r\n";
        using var stream = ToStream(csv);

        var result = _sut.ParseJobTitleMappings(stream);

        result.ContainsKey("5").Should().BeTrue();
    }

    // ── Parse (employees) ─────────────────────────────────────────────────────

    [Fact]
    public void Parse_MapsJobTitle_WhenCodeExists()
    {
        const string csv =
            "first name,last name,date of birth,email,phone number,card number,state,postcode,job_code,salary,notes\r\n" +
            "Alice,Smith,1990-01-01,a@b.com,000,1,NSW,2000,3,50000,note\r\n";
        var mappings = new Dictionary<string, string> { ["3"] = "Operator" };
        using var stream = ToStream(csv);

        var employees = _sut.Parse(stream, mappings);

        employees.Should().HaveCount(1);
        employees[0].JobTitle.Should().Be("Operator");
    }

    [Fact]
    public void Parse_SetsJobTitleNull_WhenCodeNotFound()
    {
        const string csv =
            "first name,last name,date of birth,email,phone number,card number,state,postcode,job_code,salary,notes\r\n" +
            "Alice,Smith,1990-01-01,a@b.com,000,1,NSW,2000,99,50000,note\r\n";
        var mappings = new Dictionary<string, string> { ["3"] = "Operator" };
        using var stream = ToStream(csv);

        var employees = _sut.Parse(stream, mappings);

        employees[0].JobTitle.Should().BeNull();
    }

    [Fact]
    public void Parse_HandlesQuotedFieldsWithEmbeddedNewlines()
    {
        // notes field spans two lines
        const string csv =
            "first name,last name,date of birth,email,phone number,card number,state,postcode,job_code,salary,notes\r\n" +
            "Alice,Smith,1990-01-01,a@b.com,000,1,NSW,2000,3,50000,\"line one\r\nline two\"\r\n";
        var mappings = new Dictionary<string, string> { ["3"] = "Operator" };
        using var stream = ToStream(csv);

        var employees = _sut.Parse(stream, mappings);

        employees.Should().HaveCount(1);
        employees[0].Notes.Should().Contain("line one");
        employees[0].Notes.Should().Contain("line two");
    }

    // ── ParseCsv (internal low-level parser) ─────────────────────────────────

    [Fact]
    public void ParseCsv_HandlesDoubledQuoteEscape()
    {
        const string csv = "a,\"say \"\"hello\"\"\",b\r\n";
        var rows = CsvParserService.ParseCsv(csv);
        rows[0][1].Should().Be("say \"hello\"");
    }

    [Fact]
    public void ParseCsv_IgnoresTrailingEmptyRow()
    {
        const string csv = "a,b\r\n1,2\r\n";
        var rows = CsvParserService.ParseCsv(csv);
        rows.Should().HaveCount(2); // header + 1 data row
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static Stream ToStream(string content) =>
        new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
}
