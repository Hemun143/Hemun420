
using Integrations.HKTDC.SPCS.Domain;
using Integrations.HKTDC.SPCS.OrderImport.Configuration;
using Integrations.HKTDC.SPCS.OrderImport.Helpers;
using Integrations.HKTDC.SPCS.OrderImport.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ungerboeck.Api.Models.Subjects;
using Ungerboeck.Api.Sdk;

namespace Integrations.HKTDC.SPCS.OrderImport
{
    public class ContractService : IContractService
    {
        private readonly ApiClient _apiClient;
        private readonly ContractOrderImportOptions _contractOrderImportOptions;
        private readonly IOrderService _orderService;

        public ContractService(ApiClient apiClient, IOptions<ContractOrderImportOptions> options,IOrderService orderService)
        {
            _apiClient = apiClient;
            _contractOrderImportOptions = options.Value;
            _orderService = orderService;

        }

        public void ProcessContractData()
        {
            string contractStatus = "";

          Contract contract = FileHelper.DeseraliseInputfile("");

            string contractAccount = string.Empty;
            string orderAccount = string.Empty;
            string contactContact = string.Empty;
            string billToAccount = string.Empty;
            if(string.IsNullOrWhiteSpace(contract.AgentCCDID))
            {
                contractAccount = contract.AdvertiserCCDID;
                orderAccount = contract.AdvertiserCCDID;
                contactContact = contract.AdvertiserContactID;
            }
            else
            {
                contractAccount = contract.AgentCCDID;
                orderAccount = contract.AgentCCDID;
                contactContact = contract.AgentContactID;
            }

            if (contract.BillToCCDID == contract.AdvertiserCCDID)
                billToAccount = contract.AdvertiserContactID;
            if (contract.BillToCCDID == contract.AgentCCDID)
                billToAccount = contract.AgentContactID;
            if(contract.BillToCCDID != contract.AdvertiserContactID && contract.BillToCCDID != contract.AgentContactID)
            {
                //Last updated contact on the account
            }

            string ubsContractStatus = GetUbsContractStatus(contract.ContractStatus);
            bool isJointPackageContract = !string.IsNullOrWhiteSpace(contract.JointPackageReferenceNumber);
            if (ubsContractStatus == null)
            {
                //throw an exception that the contract status is not defined from the flat file
            }
            //string orderOdataSearch = $"C|69|01|UserText10 eq 'C1565145'"; //C00168551

            var opportunitiesModel1 = _apiClient.Endpoints.Opportunities.Search("01", $"Description eq '{contract.ContractID}'").Results.FirstOrDefault();

            var opportunitiesModel = _apiClient.Endpoints.Opportunities.Search("01", $"Description eq 'C00168551'").Results.FirstOrDefault();


            //var account = _apiClient.Endpoints.Accounts.Search("01", $"AccountCode eq ''"

            var account = _apiClient.Endpoints.Accounts.Search("01", $"Search eq '{contractAccount}' or Search eq '{billToAccount}'").Results;

            var salesAccount = _apiClient.Endpoints.Accounts.Search("01", $"IACVB eq '{contract.SalesCode}'").Results;

            if(salesAccount==null || salesAccount.Count()>1)
            {
                //throw exception that either multiple account found or no account found
            }

            if (account==null && account.Count()==0)
            {
                throw new Exception();
            }

            if (opportunitiesModel != null)
            {
                opportunitiesModel.Status = ubsContractStatus;
                //var datav = _apiClient.Endpoints.Opportunities.Update(opportunitiesModel);
               
            }
            else
            {


                var myOpportunity = new OpportunitiesModel
                {
                    Organization = _contractOrderImportOptions.OrganizationCode,
                    Description = contract.ContractID + "9",
                    Account = account.Where(x=>x.Search == contractAccount).FirstOrDefault().AccountCode,
                    Status = ubsContractStatus,
                    Class = Constants.OPPORTUNITY_CLASS,
                    Type = Constants.OPPORTUNITY_TYPE,
                    //contact should be contractContact because of some invalid data not yet assigned 
                    Contact = account.Where(x => x.Search == billToAccount).FirstOrDefault().AccountCode,
                    Salesperson = salesAccount.FirstOrDefault().AccountCode,
                    UserDateTime01 = contract.ContractSubmissionDate.Value.Date,
                    UserDateTime02 = contract.ContractSignDate.Value.Date,

                    //Need to Check the configuration is mussing for this fields
                    //UserText19 = 
                    //UserText20 =

                    //Need to cross check UserText20 should be configured on Contract or on Order
                    //Currently Configured in Orders but as per the spec it is on Contract

                };

                var opportunityModel = _apiClient.Endpoints.Opportunities.Add(myOpportunity);

                _orderService.ProcessOrderData(contract, false, myOpportunity.Account, billToAccount, salesAccount.FirstOrDefault().PersonnelDepartment, opportunityModel.SequenceNumber);



            }

        }
        private string GetUbsContractStatus(string spcsStatus)
        {
            string status = null;
            switch(spcsStatus)
            {
                case Constants.SPCS_CONTRACT_INPROGRES_STATUS:
                case Constants.SPCS_CONTRACT_Closed_STATUS:
                case Constants.SPCS_CONTRACT_OPEN_STATUS:
                    status = _contractOrderImportOptions.ContractStatus.OpenStatus;
                        break;
                case Constants.SPCS_CONTRACT_CANCELLED_STATUS:
                    status = _contractOrderImportOptions.ContractStatus.CancelledStatus;
                    break;
                default:
                    break;

            }
            return status;
        }





    }
}
