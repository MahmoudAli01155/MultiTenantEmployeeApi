using Application.Common.Models;
using Application.Employees;
using Application.Employees.Commands;
using Application.Employees.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly ISender _sender;

        public EmployeesController(ISender sender) => _sender = sender;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command, CancellationToken ct)
        {
            var employee = await _sender.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ApiResponse<EmployeeDto>.Ok(employee));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ListEmployeesQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            var pagination = new PaginationMeta
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
            return Ok(ApiResponse<IReadOnlyList<EmployeeDto>>.Ok(result.Items, pagination));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var employee = await _sender.Send(new GetEmployeeByIdQuery(id), ct);
            return Ok(ApiResponse<EmployeeDto>.Ok(employee));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeCommand command, CancellationToken ct)
        {
            var employee = await _sender.Send(command with { Id = id }, ct);
            return Ok(ApiResponse<EmployeeDto>.Ok(employee));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _sender.Send(new DeleteEmployeeCommand(id), ct);
            return NoContent();
        }
    }
}
