using Integrations.HKTDC.Webforms.Model;

namespace Integrations.HKTDC.Webforms.Services
{
    public interface IPaymentService
    {
        BillToAccount GetBillToAccount(string orgCode, string ccdId);
        IEnumerable<TransactionType> GetTransactionTypes(string orgCode);
        IEnumerable<Order> GetOrders(string orgId, string accountCode);
    }
}
