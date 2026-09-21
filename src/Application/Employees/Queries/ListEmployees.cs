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

namespace Application.Employees.Queries
{
    public record ListEmployeesQuery : IRequest<PagedResult<EmployeeDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Department { get; init; }
        public EmployeeStatus? Status { get; init; }
    }

    public class ListEmployeesQueryValidator : AbstractValidator<ListEmployeesQuery>
    {
        public ListEmployeesQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        }
    }

    public class ListEmployeesQueryHandler : IRequestHandler<ListEmployeesQuery, PagedResult<EmployeeDto>>
    {
        private readonly IApplicationDbContext _db;

        public ListEmployeesQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<PagedResult<EmployeeDto>> Handle(ListEmployeesQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Employees.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Department))
                query = query.Where(e => e.Department == request.Department);

            if (request.Status.HasValue)
                query = query.Where(e => e.Status == request.Status.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var employees = await query
                .OrderBy(e => e.CreatedAt).ThenBy(e => e.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<EmployeeDto>(
                employees.Select(EmployeeDto.From).ToList(),
                request.Page,
                request.PageSize,
                totalCount);
        }
    }
}
