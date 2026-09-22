using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Employees.Queries.ListEmployee
{
    public class ListEmployeesQueryValidator : AbstractValidator<ListEmployeesQuery>
    {
        public ListEmployeesQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        }
    }
}
