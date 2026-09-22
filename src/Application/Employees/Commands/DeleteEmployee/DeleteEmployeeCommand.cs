using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Employees.Commands.DeleteEmployee
{
    public record DeleteEmployeeCommand(Guid Id) : IRequest;

}
