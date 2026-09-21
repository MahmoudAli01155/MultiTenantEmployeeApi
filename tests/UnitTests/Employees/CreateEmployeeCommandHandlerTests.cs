using Application.Common.Exceptions;
using Application.Employees.Commands;
using Domain.Enums;
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
    public class CreateEmployeeCommandHandlerTests : IDisposable
    {
        private readonly TestDatabase _database = new();

        [Fact]
        public async Task Handle_WithValidCommand_CreatesEmployeeForCurrentTenant()
        {
            var (db, tenant) = _database.CreateScope(SeedData.TenantAId);
            var handler = new CreateEmployeeCommandHandler(db, tenant);
            var command = new CreateEmployeeCommand("Ali", "Hassan", "Ali@Test.com", "IT", null, null);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Id.Should().NotBeEmpty();
            result.TenantId.Should().Be(SeedData.TenantAId);
            result.Email.Should().Be("ali@test.com");
            result.Status.Should().Be(EmployeeStatus.Active);

            var (verifyDb, _) = _database.CreateScope(SeedData.TenantAId);
            var saved = await verifyDb.Employees.SingleAsync();
            saved.Id.Should().Be(result.Id);
            saved.CreatedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task Handle_WithDuplicateEmail_ThrowsConflictException()
        {
            var (db, tenant) = _database.CreateScope(SeedData.TenantAId);
            var handler = new CreateEmployeeCommandHandler(db, tenant);
            await handler.Handle(
                new CreateEmployeeCommand("Ali", "Hassan", "ali@test.com", "IT", null, null),
                CancellationToken.None);

            var act = () => handler.Handle(
                new CreateEmployeeCommand("Omar", "Said", "ALI@test.com", "HR", null, null),
                CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
            (await db.Employees.CountAsync()).Should().Be(1);
        }

        public void Dispose() => _database.Dispose();
    }
}
