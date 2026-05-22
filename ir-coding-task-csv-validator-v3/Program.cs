using ir_coding_task_csv_validator.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CSV Validator API", Version = "v1" });
});

// Domain services
builder.Services.AddSingleton<IPostcodeValidationService, AustralianPostcodeValidationService>();
builder.Services.AddSingleton<ICsvParserService, CsvParserService>();
builder.Services.AddSingleton<IEmployeeValidationService, EmployeeValidationService>();

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
