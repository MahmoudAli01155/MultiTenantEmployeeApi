using Application.Common.Exceptions;
using Application.Employees.Commands;
using Application.Employees.Commands.CreateEmployee;
using Application.Employees.Queries;
using Application.Employees.Queries.GetEmployeeById;
using Application.Employees.Queries.ListEmployee;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Helpers;

namespace UnitTests.Employees
{
    public class TenantIsolationTests : IDisposable
    {
        private readonly TestDatabase _database = new();

        [Fact]
        public async Task List_ReturnsOnlyEmployeesOfCurrentTenant()
        {
            await _database.SeedEmployeesAsync(SeedData.TenantAId, 3);
            await _database.SeedEmployeesAsync(SeedData.TenantBId, 2);

            var (dbA, _) = _database.CreateScope(SeedData.TenantAId);
            var (dbB, _) = _database.CreateScope(SeedData.TenantBId);

            var resultA = await new ListEmployeesQueryHandler(dbA)
                .Handle(new ListEmployeesQuery(), CancellationToken.None);
            var resultB = await new ListEmployeesQueryHandler(dbB)
                .Handle(new ListEmployeesQuery(), CancellationToken.None);

            resultA.TotalCount.Should().Be(3);
            resultA.Items.Should().OnlyContain(e => e.TenantId == SeedData.TenantAId);
            resultB.TotalCount.Should().Be(2);
            resultB.Items.Should().OnlyContain(e => e.TenantId == SeedData.TenantBId);
        }

        [Fact]
        public async Task GetById_ForEmployeeOfAnotherTenant_ThrowsNotFoundException()
        {
            var idsB = await _database.SeedEmployeesAsync(SeedData.TenantBId, 1);
            var (dbA, _) = _database.CreateScope(SeedData.TenantAId);
            var (dbB, _) = _database.CreateScope(SeedData.TenantBId);

            var act = () => new GetEmployeeByIdQueryHandler(dbA)
                .Handle(new GetEmployeeByIdQuery(idsB[0]), CancellationToken.None);
            var found = await new GetEmployeeByIdQueryHandler(dbB)
                .Handle(new GetEmployeeByIdQuery(idsB[0]), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
            found.Id.Should().Be(idsB[0]);
        }

        [Fact]
        public async Task Create_WithSameEmailInDifferentTenants_Succeeds()
        {
            var (dbA, tenantA) = _database.CreateScope(SeedData.TenantAId);
            var (dbB, tenantB) = _database.CreateScope(SeedData.TenantBId);
            var command = new CreateEmployeeCommand("Ali", "Hassan", "ali@test.com", "IT", null, null);

            await new CreateEmployeeCommandHandler(dbA, tenantA).Handle(command, CancellationToken.None);
            var createdInB = await new CreateEmployeeCommandHandler(dbB, tenantB).Handle(command, CancellationToken.None);

            createdInB.TenantId.Should().Be(SeedData.TenantBId);
            (await dbA.Employees.CountAsync()).Should().Be(1);
            (await dbB.Employees.CountAsync()).Should().Be(1);
        }

        public void Dispose() => _database.Dispose();
    }
}
