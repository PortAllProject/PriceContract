using System.Collections.Generic;
using System.Linq;
using AElf.Boilerplate.TestBase;
using AElf.Contracts.MultiToken;
using AElf.Contracts.TestsOracle;
using AElf.ContractTestKit;
using AElf.Cryptography.ECDSA;
using AElf.Types;

namespace AElf.Contracts.Price.Test
{
    public class PriceContractTestBase : DAppContractTestBase<PriceContractTestModule>
    {
        protected Address DefaultSender { get; set; }
        internal IList<Address> OracleNodes { get; set; }
        internal OracleTestContractContainer.OracleTestContractStub OracleContractStub { get; set; }
        internal TokenContractContainer.TokenContractStub TokenContractStub { get; set; }

        internal PriceContractContainer.PriceContractStub PriceContractStub { get; set; }
        protected Address OracleTestContractAddress =>
            SystemContractAddresses[OracleTestContractAddressNameProvider.Name];

        protected Address PriceContractAddress =>
            SystemContractAddresses[PriceContractAddressNameProvider.Name];

        protected PriceContractTestBase()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            DefaultSender = SampleAccount.Accounts.First().Address;
            OracleContractStub = GetOracleContractStub(keyPair);
            TokenContractStub = GetTokenContractStub(keyPair);
            PriceContractStub = GetPriceContractStub(keyPair);
            OracleNodes = SampleAccount.Accounts.Take(5).Select(x => x.Address).ToList();
        }

        internal PriceContractContainer.PriceContractStub GetPriceContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<PriceContractContainer.PriceContractStub>(PriceContractAddress, senderKeyPair);
        }
        internal OracleTestContractContainer.OracleTestContractStub GetOracleContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<OracleTestContractContainer.OracleTestContractStub>(OracleTestContractAddress, senderKeyPair);
        }

        internal TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress, senderKeyPair);
        }
    }
}