using Domain.Enums;
using MediatR;
using System.Text.Json;

namespace Application.Employees.Commands.CreateEmployee
{
    public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string Department,
    EmployeeStatus? Status,
    JsonElement? CustomData) : IRequest<EmployeeDto>, IEmployeeInput;


}
