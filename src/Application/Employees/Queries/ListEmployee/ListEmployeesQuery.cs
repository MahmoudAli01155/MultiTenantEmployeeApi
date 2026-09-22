using Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace Application.Employees.Queries.ListEmployee
{
    public record ListEmployeesQuery : IRequest<PagedResult<EmployeeDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Department { get; init; }
        public EmployeeStatus? Status { get; init; }
    }




}
