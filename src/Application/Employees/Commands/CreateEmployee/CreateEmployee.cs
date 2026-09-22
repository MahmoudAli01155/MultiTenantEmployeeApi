using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Employees.Validators;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
