using Integrations.HKTDC.Webforms.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ungerboeck.Api.Sdk;

namespace Integrations.HKTDC.Webforms.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApiClient _apiClient;
        private readonly HKTDCContext _context;

        public PaymentService(ApiClient apiClient, HKTDCContext context)
        {
            _apiClient = apiClient;
            _context = context;
        }

        public BillToAccount GetBillToAccount(string orgCode, string ccdId)
        {
            var accounts = _apiClient.Endpoints.Accounts.Search(orgCode, $"Search eq '{ccdId}'").Results;

            if (accounts.Any())
            {
                var account = accounts.First();

                return new BillToAccount() { AccountCode = account.AccountCode, CCDId = account.Search, Company = account.Company };
            }

            return null;
        }

        public IEnumerable<TransactionType> GetTransactionTypes(string orgCode)
        {
            var transactionTypes = _apiClient.Endpoints.ReceivableTransactionTypes.Search(orgCode, $"Status eq 'A'").Results;

            if (transactionTypes.Any())
            {
                var transactions = transactionTypes.Select(x => new TransactionType { Type = x.Type, Source = x.Source, Method = x.Method, Description = x.Description, Currency = x.Currency });

                return transactions;
            }

            return null;
        }

        public IEnumerable<Order> GetOrders(string orgId, string accountCode)
        {       
            var result = _context.Orders.FromSqlRaw(@"exec GetOrderByContract @OrgID,@AccountCode",
                new SqlParameter("@OrgID", orgId), new SqlParameter("@AccountCode", accountCode)).ToList();

            return result;
        }
    }
}
