using Application.Employees.Validators;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Employees.Commands.UpdateEmployeeCommand
{
    public class UpdateEmployeeCommandValidator : EmployeeInputValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeCommandValidator()
        {
            RuleFor(x => x.Status).NotNull();
        }
    }
}
