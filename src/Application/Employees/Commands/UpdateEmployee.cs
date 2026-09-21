using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Employees.Validators;
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

namespace Application.Employees.Commands
{
    public record UpdateEmployeeCommand(
Guid Id,
string FirstName,
string LastName,
string Email,
string Department,
EmployeeStatus? Status,
JsonElement? CustomData) : IRequest<EmployeeDto>, IEmployeeInput;

    public class UpdateEmployeeCommandValidator : EmployeeInputValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeCommandValidator()
        {
            RuleFor(x => x.Status).NotNull();
        }
    }

    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
    {
        private readonly IApplicationDbContext _db;

        public UpdateEmployeeCommandHandler(IApplicationDbContext db) => _db = db;

        public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException($"Employee '{request.Id}' was not found.");

            var email = request.Email.Trim().ToLowerInvariant();

            if (await _db.Employees.AnyAsync(e => e.Email == email && e.Id != request.Id, cancellationToken))
                throw new ConflictException($"An employee with email '{email}' already exists.");

            employee.FirstName = request.FirstName.Trim();
            employee.LastName = request.LastName.Trim();
            employee.Email = email;
            employee.Department = request.Department.Trim();
            employee.Status = request.Status!.Value;
            employee.CustomData = request.CustomData?.GetRawText() ?? "{}";
            employee.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return EmployeeDto.From(employee);
        }
    }
}
