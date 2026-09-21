using Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Multitenancy
{
    public class TenantContext : ITenantContext
    {
        private Guid? _tenantId;

        public Guid TenantId => _tenantId
            ?? throw new InvalidOperationException("Tenant was not resolved for the current request.");

        public void SetTenant(Guid tenantId) => _tenantId = tenantId;
    }
}
