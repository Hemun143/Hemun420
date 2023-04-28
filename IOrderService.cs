using Integrations.HKTDC.SPCS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrations.HKTDC.SPCS.OrderImport.Services
{
    public interface IOrderService
    {
        void ProcessOrderData(Contract contract,bool isAccountOnHold, string accountCode, string billToContact, string salesPersonDepartment, int? sequenceNumber);
    }
}
