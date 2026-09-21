using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Employees
{
    public interface IEmployeeInput
    {
        string FirstName { get; }
        string LastName { get; }
        string Email { get; }
        string Department { get; }
        EmployeeStatus? Status { get; }
        JsonElement? CustomData { get; }
    }
}
