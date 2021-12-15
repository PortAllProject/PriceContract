using System.Collections.Generic;
using AElf.Boilerplate.TestBase;
using AElf.Kernel.SmartContract.Application;
using AElf.Types;

namespace AElf.Contracts.Price.Test.ContractInitializationProviders
{
    public class OracleTestContractInitializationProvider : IContractInitializationProvider
    {
        public List<ContractInitializationMethodCall> GetInitializeMethodList(byte[] contractCode)
        {
            return new List<ContractInitializationMethodCall>();
        }

        public Hash SystemSmartContractName { get; } = OracleTestContractAddressNameProvider.Name;
        public string ContractCodeName { get; } = "AElf.Contracts.OracleTest";
    }
}