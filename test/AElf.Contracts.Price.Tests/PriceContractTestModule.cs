using System.Collections.Generic;
using System.IO;
using AElf.Boilerplate.TestBase;
using AElf.Contracts.Price.Test.ContractInitializationProviders;
using AElf.Contracts.TestOracle;
using AElf.ContractTestBase;
using AElf.Kernel.SmartContract.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AElf.Contracts.Price.Test
{
    [DependsOn(typeof(MainChainDAppContractTestModule))]
    public class PriceContractTestModule : MainChainDAppContractTestModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IContractInitializationProvider, OracleTestContractInitializationProvider>();
            context.Services.AddSingleton<IContractInitializationProvider, PriceContractInitializationProvider>();
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            var contractCodeProvider = context.ServiceProvider.GetService<IContractCodeProvider>();
            var contractCodes = new Dictionary<string, byte[]>(contractCodeProvider.Codes)
            {
                {
                    new OracleTestContractInitializationProvider().ContractCodeName,
                    File.ReadAllBytes(typeof(OracleTestContract).Assembly.Location)
                },
                {
                    new PriceContractInitializationProvider().ContractCodeName,
                    File.ReadAllBytes(typeof(PriceContract).Assembly.Location)
                }
            };
            contractCodeProvider.Codes = contractCodes;
        }
    }
}