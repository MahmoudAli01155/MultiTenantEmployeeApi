using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Employees
{
    public record EmployeeDto(
    Guid Id,
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    EmployeeStatus Status,
    JsonElement CustomData,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
    {
        public static EmployeeDto From(Employee e)
        {
            using var doc = JsonDocument.Parse(e.CustomData);
            return new EmployeeDto(e.Id, e.TenantId, e.FirstName, e.LastName, e.Email,
                e.Department, e.Status, doc.RootElement.Clone(), e.CreatedAt, e.UpdatedAt);
        }
    }
}
