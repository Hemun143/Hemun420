using AutoMapper;
using HKTDC.DAL.Models;
using Integrations.HKTDC.SPCS.Domain;
using Integrations.HKTDC.SPCS.Mapping;
using Integrations.HKTDC.SPCS.ResourceImport.Configuration;
using Integrations.HKTDC.SPCS.ResourceImport.Helper;
using Integrations.HKTDC.SPCS.ResourceImport.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ungerboeck.Api.Models.Authorization;
using Ungerboeck.Api.Models.Subjects;
using Ungerboeck.Api.Sdk;

// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

// Resources = Products
// first off look at auth

// todo: Get the validation table entries for 61 (not hardcoded, config)
// from the validation table entries, in the future we'll be able to determine the order form

// keep what you can in the SellableItemService if it fits
// Refer to the test app for examples

// todo: Needs to be able to parse a file.
// input parameter 'folder path' eg c:\myimport
// sellable-item-1.json = SellableItem
// for each file in this directory  
//    parse it, deserialize it as either a SellableItem or a Package
//    confirm the mapping from the specification, map the sellable item to a resource
//    check if the resource exists, if so update, else create
//    once the resource is created/updated, check the pricelist for the item (including the order form determined from the above list)
//    if not on the price list, add it.

// 1. call the Validation API  for 61 
// 2. Get the result of API 
// 3. Grab the JSON file from the path 
// 4. Numerical portion of TierCode and ItemCode -> Determines ResourceCode in UB of the product being created
// 4.1 If exits update otherwise create a new one.
// 4.2 Also create "Resources User Defined fields" (but they will be created by Custom API)
// 5. Determine the order form from the result of the #2. Catg matches desc1 and Type matches desc 2. Desc 5 id Order form 
// 6. Once order form is there, Create PriceList for relevant US and HK.

using IHost host = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
{
    services.Configure<Jwt>(
    context.Configuration.GetSection(nameof(Jwt)));
    services.Configure<ResourceImportOptions>(
    context.Configuration.GetSection(nameof(ResourceImportOptions)));
    services.AddScoped<IResourceService, ResourceService>();
    services.AddScoped<ApiHelper>();
    services.AddSingleton(options => options.GetRequiredService<IOptions<Jwt>>().Value);
    services.AddSingleton<ApiClient>();
    services.AddAutoMapper(typeof(ResourceProfile).Assembly);
}).Build();

    using var serviceScope = host.Services.CreateScope();
    var resource = serviceScope.ServiceProvider.GetRequiredService<IResourceService>();
    var resourceImportOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<ResourceImportOptions>>().Value;
    var result = resource.GetValidationSettings();
    foreach (var item in result)
    {
    Console.WriteLine(JsonConvert.SerializeObject(item));
    Console.WriteLine("\n");
    }
    var resouceFileData = resource.GetResourceFileContent();
    var mapper = serviceScope.ServiceProvider.GetRequiredService<IMapper>();

//Save Resource for Package data
    if (resouceFileData.Packages != null && resouceFileData.Packages.Any())
    {
        foreach (var package in resouceFileData.Packages)
    {
        var resourceUDF = mapper.Map<Package, ResourceUDF>(package);
        var resourceModelForPackage = mapper.Map<Package, ResourcesModel>(package);

        // Check if resource exists
        if (resourceUDF != null)
        {
            var isSuccess = await resource.CreateResourceUDF(resourceUDF, package.PackageCode); // UDF
        }

        if (resourceModelForPackage != null)
        {
            var resourceModel = resource.CreateResource(resourceModelForPackage); // Resource 
        }

        if (package.PriceTiers != null && package.PriceTiers.Any())
        {
            var priceModelList = new List<PriceListModel>();

            foreach (var priceTier in package.PriceTiers)
            {
                var priceModel = new PriceListModel();

                if (priceTier.USD != null)
                {
                    priceModel.OrganizationCode = resourceImportOptions.OrganizationCode; // get it from config 
                    priceModel.Currency = "USD";
                    priceModel.EndDate = priceTier.EffectiveEnd;
                    priceModel.StartDate = priceTier.EffectiveStart;
                    priceModel.Code = resourceImportOptions.PackageUSD;
                    priceModelList.Add(priceModel);
                }

                if (priceTier.HKD != null)
                {
                    priceModel.OrganizationCode = resourceImportOptions.OrganizationCode;
                    priceModel.Currency = "HKD";
                    priceModel.EndDate = priceTier.EffectiveEnd;
                    priceModel.StartDate = priceTier.EffectiveStart;
                    priceModel.Code = resourceImportOptions.PackageHKD;
                    priceModelList.Add(priceModel);
                }
            }
            resource.CreatePriceList(priceModelList);
        }
    }
    }

//Save Resource for SellableItem data
    if (resouceFileData.SellableItems != null && resouceFileData.SellableItems.Any())
    {   
         foreach (var sellableItem in resouceFileData.SellableItems)
    {
        var resourceUDF = mapper.Map<SellableItem, ResourceUDF>(sellableItem);
        var resourceModelForSellableItem = mapper.Map<SellableItem, ResourcesModel>(sellableItem);
        if (resourceUDF != null)
        {
            var isSuccess = await resource.CreateResourceUDF(resourceUDF, sellableItem.ItemCode);
        }
        if (resourceModelForSellableItem != null)
        {
            var resourceModel = resource.CreateResource(resourceModelForSellableItem);
        }
        if (sellableItem.PriceTiers != null && sellableItem.PriceTiers.Any())
        {
            var priceModelList = new List<PriceListModel>();
            foreach (var priceTier in sellableItem.PriceTiers)
            {
                var priceModel = new PriceListModel();

                if (priceTier.USD != null)
                {
                    priceModel.Currency = "USD";
                    priceModel.EndDate = priceTier.EffectiveEnd;
                    priceModel.StartDate = priceTier.EffectiveStart;
                    priceModel.Code = resourceImportOptions.SellableUSD;

                    priceModelList.Add(priceModel);
                }
                if (priceTier.HKD != null)
                {
                    priceModel.Currency = "HKD";
                    priceModel.EndDate = priceTier.EffectiveEnd;
                    priceModel.StartDate = priceTier.EffectiveStart;
                    priceModel.Code = resourceImportOptions.SellableHKD;

                    priceModelList.Add(priceModel);
                }
            }
            resource.CreatePriceList(priceModelList);
        }
    }
    }

    Console.WriteLine("Serialized Package data.......");
    Console.WriteLine(JsonConvert.SerializeObject(resouceFileData.Packages));
    Console.WriteLine("\n");
    Console.WriteLine("Serialized SellebleItem data.......");
    Console.WriteLine(JsonConvert.SerializeObject(resouceFileData.SellableItems));

