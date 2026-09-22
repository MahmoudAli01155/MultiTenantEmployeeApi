using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ITenantContext _tenant;

        public CreateEmployeeCommandHandler(IApplicationDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _db.Employees.AnyAsync(e => e.Email == email, cancellationToken))
                throw new ConflictException($"An employee with email '{email}' already exists.");

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                TenantId = _tenant.TenantId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Department = request.Department.Trim(),
                Status = request.Status ?? EmployeeStatus.Active,
                CustomData = request.CustomData?.GetRawText() ?? "{}",
                CreatedAt = DateTime.UtcNow
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync(cancellationToken);

            return EmployeeDto.From(employee);
        }
    }
}
