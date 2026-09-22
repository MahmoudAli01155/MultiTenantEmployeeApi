using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
