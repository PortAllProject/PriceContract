using System;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Contracts.TestsOracle;
using AElf.Types;
using Shouldly;
using Volo.Abp.Threading;
using Xunit;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Price.Test
{
    public partial class PriceContractTests : PriceContractTestBase
    {
        public PriceContractTests()
        {
            AsyncHelper.RunSync(InitializePriceContractAsync);
            AsyncHelper.RunSync(CreatePortTokenAsync);
        }

        [Fact]
        public async Task Initialize_Repeat_Should_Fail()
        {
            var txResult = await PriceContractStub.Initialize.SendWithExceptionAsync(new InitializeInput
            {
                OracleAddress = OracleTestContractAddress,
                Controller = DefaultSender
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        }

        [Fact]
        public async Task RecordTokenPrice_Without_Query_Id_Should_Fail()
        {
            var queryId = Hash.Empty;
        
            var result = await OracleContractStub.RecordTokenPrice.SendWithExceptionAsync(new TokenPriceInfo
            {
                CallBackAddress = PriceContractAddress,
                CallBackMethodName = nameof(PriceContractStub.RecordExchangeTokenPrice),
                QueryId = queryId,
                TokenPrice = GenerateTokenPriceInfo("ELF", "LLYP", "1.2345", Timestamp.FromDateTime(DateTime.UtcNow)),
                OracleNodes = {OracleNodes}
            });
            result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        }

        private async Task InitializePriceContractAsync()
        {
            await PriceContractStub.Initialize.SendAsync(new InitializeInput
            {
                OracleAddress = OracleTestContractAddress,
                Controller = DefaultSender
            });
        }

        private async Task CreatePortTokenAsync()
        {
            await TokenContractStub.Create.SendAsync(new CreateInput
            {
                Symbol = TokenSymbol,
                TokenName = "Port",
                TotalSupply = 1000_000_000L,
                Decimals = 8,
                Issuer = DefaultSender,
                IsBurnable = true,
                IssueChainId = 0
            });
            await TokenContractStub.Issue.SendAsync(new IssueInput
            {
                Amount = 100_000_000L,
                Symbol = TokenSymbol,
                To = DefaultSender
            });
        }

        private TestsOracle.TokenPrice GenerateTokenPriceInfo(string tokenSymbol, string targetTokenSymbol,
            string price,
            Timestamp timestamp)
        {
            return new TestsOracle.TokenPrice
            {
                Timestamp = timestamp,
                TokenSymbol = tokenSymbol,
                TargetTokenSymbol = targetTokenSymbol,
                Price = price
            };
        }
    }
}