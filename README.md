# CSV Validator API

A .NET Web API that parses and validates employee CSV data.

## How to Run

```bash
cd ir-coding-task-csv-validator-v3
dotnet run
```

Then open http://localhost:5246/swagger to test the API.

## How to Test

```bash
dotnet test
```

## Endpoints

- `POST /Validator/Validate` — Upload `Sample_Data.csv` and `Job_Title_Mappings.csv` to validate employee data

## Validation Rules

- Warning if first or last name is less than 4 characters
- Error if job code is not found in the mappings file
- Warning if salary is not a positive integer
- Error if postcode is missing
- Warning if postcode is not valid for the state

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- xUnit + FluentAssertions (35 unit tests)
