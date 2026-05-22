# Assumptions & Design Notes

## Assumptions

### 1. File upload vs. baked-in paths
The skeleton's `Validate` endpoint already accepts two `IFormFile` parameters (`csvFile`, `jobTitleFile`), so I kept that interface.  The caller supplies both files at runtime rather than reading hard-coded paths, which is more appropriate for a production API.

### 2. "Names less than four characters" applies to both first *and* last name
The instruction says "names", which is plural and ambiguous.  I treat it as applying independently to `first name` and `last name`, producing a separate warning for each.

### 3. "Salary is not a positive integer"
The sample data stores salaries like `$620.62` (decimal, dollar-prefixed). I strip a leading `$` before parsing, then check whether the resulting value is a whole number greater than zero.  Any non-parseable string also triggers the warning.

### 4. Australian postcode ranges
Official Australia Post ranges are used.  NT postcodes are stored as 3- or 4-digit strings but parsed numerically (e.g. `0800` → 800).  ACT postcodes overlap with NSW in some
implementations; I follow the Australia Post allocation which gives ACT its own range  (200-299, 2600-2618, 2900-2920) and reserves the adjacent NSW bands accordingly.

### 5. Unknown state
If the `state` value is not one of the eight known codes (NSW, ACT, VIC, QLD, SA, WA, TAS, NT) the postcode is considered invalid for that state, producing a warning.  It does not produce an *error* because the instruction only specifies postcode-related rules, not state validation.

### 6. No external CSV library
Rather than adding a dependency (e.g. CsvHelper), I implemented a minimal RFC 4180-compliant parser.  The sample data contains multi-line quoted `notes` fields, so a line-by-line `string.Split` approach would fail; the parser correctly handles embedded newlines and doubled-quote escaping. The parser is fully unit-tested.

### 7. Response shape
`ValidationResponse` includes a summary (total rows, rows with errors, rows with warnings) and the per-row detail (`EmployeeValidationResult`).  Each result includes the full `Employee` object plus a list of `ValidationMessage` records `(Severity, Field, Message)`.  This gives consumers everything they need to display results or filter by severity.

### 8. DI lifetime
All three services (`CsvParserService`, `AustralianPostcodeValidationService`, `EmployeeValidationService`) are stateless, so they are registered as `Singleton`.
