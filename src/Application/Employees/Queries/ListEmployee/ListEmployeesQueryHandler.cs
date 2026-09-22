using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Employees.Queries.ListEmployee
{
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
