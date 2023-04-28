using Integrations.HKTDC.SPCS.Domain;
using Integrations.HKTDC.SPCS.OrderImport.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;
using Ungerboeck.Api.Models.Subjects;
using Ungerboeck.Api.Sdk;
using Attribute = Integrations.HKTDC.SPCS.Domain.Attribute;
using Contract = Integrations.HKTDC.SPCS.Domain.Contract;

namespace Integrations.HKTDC.SPCS.OrderImport.Services
{
    public class OrderService : IOrderService
    {

        private readonly ApiClient _apiClient;
        private readonly ContractOrderImportOptions _contractOrderImportOptions;

        public OrderService(ApiClient apiClient, IOptions<ContractOrderImportOptions> options)
        {
            _apiClient = apiClient;
            _contractOrderImportOptions = options.Value;


        }

        public void ProcessOrderData(Contract contract, bool isAccountOnHold, string accountCode, string billToContact, string salesPersonDepartment,int? sequenceNumber)
        {

            var userDefinedFields = _apiClient.Endpoints.UserDefinedFields.Search(_contractOrderImportOptions.OrganizationCode, $"IssueOpportunityType eq '68'");

            //Ad Source 
            var adSourceField = userDefinedFields.Results.Where(x => x.DatabaseField == "TXT_05").FirstOrDefault();

            //Publication
            var publicationField = userDefinedFields.Results.Where(x => x.DatabaseField == "TXT_02").FirstOrDefault();      

            var publicationValidationList = _apiClient.Endpoints.ValidationEntries.Search(_contractOrderImportOptions.OrganizationCode, $"ValidationTableID eq {publicationField.ValidationList.Value}")?.Results;

            string adSource = GetAdSource(adSourceField.ValidationList.Value, contract.ContractType,contract.AdvertiserCCDID != contract.AgentCCDID, salesPersonDepartment);
           
            //if(contract.ContractType)

            bool isPackageOrder = false;
            if (contract == null) return;
            var CancelOrders = contract.Orders.Where(x => x.ActionCode == Constants.ORDER_ACTION_CODE_CANCELLED).ToList();
            var UpdateOrders = contract.Orders.Where(x => x.ActionCode == Constants.ORDER_ACTION_CODE_UPDATED).ToList();
            var CreateOrders = contract.Orders.Where(x => x.ActionCode == Constants.ORDER_ACTION_CODE_CREATED).ToList();

            if (contract.Orders.Any(x => x.Package != null))
            {
                isPackageOrder = true;
            }
            if (CreateOrders.Any() && !isAccountOnHold)
            {

                foreach (var newOrder in CreateOrders)
                {

                    if (!isPackageOrder)
                    {
                        string ResourceCode = string.Empty;
                        if (newOrder.SellableItem != null)
                        {
                             ResourceCode = newOrder.SellableItem.ItemCode.Replace('S',' ').Trim() + newOrder.PriceTierCode.Substring(newOrder.PriceTierCode.Length - 3);
                        }

                        var myFulfillmentOrder = new FulfillmentOrdersModel
                        {
                            OrganizationCode = _contractOrderImportOptions.OrganizationCode,
                            OrderStatus = _contractOrderImportOptions.OrderStatus.ActiveStatus,
                            Account = accountCode,
                            BillToAccount = billToContact,
                            PriceList = contract.Currency == Constants.CONTRACT_CURRENCY_USD ? _contractOrderImportOptions.PriceList.SellableUSD : _contractOrderImportOptions.PriceList.SellableHKD,
                            Designation = "Event Sales",
                           // Cona = contract.ContractID + "9"
                           // con

                        };

                        Function function = newOrder.SellableItem.Functions.First();
                      
                        myFulfillmentOrder  = AddUserDefinedFields(function.Attributes.ToList(), myFulfillmentOrder,$"{newOrder.OrderId}-{function.Code}");
                        _apiClient.Endpoints.FulfillmentOrders.Add(myFulfillmentOrder);
                    }
                    else
                    {

                    }
                   
                }



            }

            if (UpdateOrders.Any() && !isAccountOnHold)
            {
                foreach (var existingOrder in UpdateOrders)
                {

                }

            }

            if (CancelOrders.Any())
            {
                foreach (var cancelledOrder in CancelOrders)
                {

                }
            }


        }

        private string GetPriceList(string currency)
        {
            return "";

        }

        private string GetAdSource(int? adSourceField ,string contractType,bool isAgency,string personalDepartment)
        {
            var adSourceValidationList = _apiClient.Endpoints.ValidationEntries.Search(_contractOrderImportOptions.OrganizationCode, $"ValidationTableID eq {adSourceField}")?.Results;
            string adSource = string.Empty;
            switch (contractType)
            {
                case Constants.CONTRACT_TYPE_JOINTPACKAGE:
                case Constants.CONTRACT_TYPE_CLUSTER:
                    adSource = adSourceValidationList.Where(x => x.Description.ToLower() == contractType.ToLower()).FirstOrDefault().Code;
                    break;
                case Constants.CONTRACT_TYPE_HOUSEACCOUNT:
                    adSource = adSourceValidationList.Where(x => x.Description.ToLower() == Constants.CONTRACT_ADSOURCE_AGENCY).FirstOrDefault().Code;
                    break;
                case Constants.CONTRACT_TYPE_GENERAL:
                    if (isAgency)
                    {
                        adSource = adSourceValidationList.Where(x => x.Description.ToLower() == Constants.CONTRACT_ADSOURCE_AGENCY).FirstOrDefault().Code;
                    }
                    else
                    {
                        adSource = _apiClient.Endpoints.Departments.Search(_contractOrderImportOptions.OrganizationCode, $"Code eq '{personalDepartment}'").Results.FirstOrDefault().AltDescription1;
                    }
                    break;  
                default:
                    break;
            }

            return adSource;
            
        }

        private FulfillmentOrdersModel AddUserDefinedFields(List<Attribute> attributes,FulfillmentOrdersModel fulfillmentOrder,string orderID)
        {
            var myUserField = new UserFields
            {
                Class = _contractOrderImportOptions.FulfilmentUDFTypeClass,
                Type = _contractOrderImportOptions.FuflfilmentUDFType,
                UserText03 = orderID
                //Use the Opportunity Type code from your user field.  This matches the value stored in Ungerboeck table column CR073_ISSUE_TYPE.              
                //Set the value in the user field property
            };
            attributes.ForEach(x =>
            {
                switch (x.AttributeName)
                {
                    case Constants.ATTRIBUTENAME_PUBCODE:
                        if (!string.IsNullOrWhiteSpace(x.AttributeValue))
                        {
                            myUserField.UserText02 = x.AttributeValue;
                        }
                        break;

                    case Constants.ATTRIBUTENAME_STARTISSUE:
                        if(!string.IsNullOrWhiteSpace(x.AttributeValue))
                        {
                            short year = Convert.ToInt16("20" + x.AttributeValue.Substring(0, 2));
                            short month = Convert.ToInt16(x.AttributeValue.Substring(2, 2));
                            myUserField.UserDateTime02 =  new DateTime(year, month,01);
                        }
                        break;
                    case Constants.ATTRIBUTENAME_STOPISSUE:
                        if (!string.IsNullOrWhiteSpace(x.AttributeValue))
                        {
                            short year = Convert.ToInt16("20" + x.AttributeValue.Substring(0, 2));
                            short month = Convert.ToInt16(x.AttributeValue.Substring(2, 2));
                            myUserField.UserDateTime03 = new DateTime(year, month, 01);
                        }
                        break;
                    case Constants.ATTRIBUTENAME_SIZE:
                        if (!string.IsNullOrWhiteSpace(x.AttributeValue))
                        {
                            myUserField.UserText06 = x.AttributeValue;
                        }
                        break;
                    case Constants.ATTRIBUTENAME_INSTRUCTION:
                        if (!string.IsNullOrWhiteSpace(x.AttributeValue))
                        {
                            myUserField.UserText08 = x.AttributeValue;
                        }
                        break;
                    case Constants.ATTRIBUTENAME_PROJECTODE:
                        if (!string.IsNullOrWhiteSpace(x.AttributeValue))
                        {
                            myUserField.UserText23 = x.AttributeValue;
                        }
                        break;
                    case Constants.ATTRIBUTENAME_FISCALYEAR:
                        if (!string.IsNullOrWhiteSpace(x.AttributeValue))
                        {
                            myUserField.UserText24 = x.AttributeValue;
                        }
                        break;
                    case Constants.ATTRIBUTENAME_NOTES:
                       // myUserField.UserText02 = x.AttributeValue;
                        break;
                    default:
                        break;
                }
            });

            fulfillmentOrder.FulfillmentOrderUserFieldSets = new List<UserFields>
            {
                myUserField
            };

            return fulfillmentOrder;
        }



    }
}
