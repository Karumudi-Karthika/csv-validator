using Microsoft.AspNetCore.Mvc;

namespace ir_coding_task_csv_validator.Controllers;

[ApiController]
[Route("[controller]")]
public class InstructionController : ControllerBase
{
    [Route("GetInstruction")]
    [HttpGet]
    public IActionResult GetInstruction()
    {
        string instrction = System.IO.File.ReadAllText("Instruction.md");
        return Ok(instrction);
    }
}