using MediatR;

namespace Application.Employees.Queries.GetEmployeeById
{
    public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;

}
