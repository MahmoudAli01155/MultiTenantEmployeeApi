using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Employees.Validators
{
    public abstract class EmployeeInputValidator<T> : AbstractValidator<T> where T : IEmployeeInput
    {
        protected EmployeeInputValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
            RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
            RuleFor(x => x.CustomData)
                .Must(v => v is null || v.Value.ValueKind == JsonValueKind.Object)
                .WithMessage("CustomData must be a JSON object.");
        }
    }
}
