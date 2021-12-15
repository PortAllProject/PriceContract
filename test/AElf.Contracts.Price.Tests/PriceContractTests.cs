using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Contracts.TestsOracle;
using AElf.CSharp.Core;
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
            AsyncHelper.RunSync(InitializePortToken);
        }

        private async Task InitializePriceContractAsync()
        {
            await PriceContractStub.Initialize.SendAsync(new InitializeInput
            {
                OracleAddress = OracleTestContractAddress,
                Controller = DefaultSender
            });
        }

        private async Task InitializePortToken()
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
        }

        [Fact]
        public async Task Initialize_Repeat_Should_Throw_Exception()
        {
            await Assert.ThrowsAsync<Exception>(InitializePriceContractAsync);
        }

        [Fact]
        public async Task QueryExchangeTokenPrice_QueryId_Should_Be_Logged()
        {
            var queryId = await QueryExchangeTokenPrice("ELF", "LLYP");
            var isQueryIdLogged = await PriceContractStub.CheckQueryIdIfExisted.CallAsync(queryId);
            isQueryIdLogged.Value.ShouldBeTrue();
        }

        [Fact]
        public async Task QuerySwapTokenPrice_QueryId_Should_Be_Logged()
        {
            var queryId = await QuerySwapTokenPrice("ELF", "LLYP");
            var isQueryIdLogged = await PriceContractStub.CheckQueryIdIfExisted.CallAsync(queryId);
            isQueryIdLogged.Value.ShouldBeTrue();
        }

        [Fact]
        public async Task RecordExchangeTokenPrice_Without_Query_Id_Should_Throw_Exception()
        {
            var queryId = Hash.Empty;
            await Assert.ThrowsAsync<Exception>(async () =>
                await RecordExchangeTokenPriceAsync(queryId, "ELF", "LLYP", "1.2345",
                    Timestamp.FromDateTime(DateTime.UtcNow)));
        }

        [Fact]
        public async Task RecordExchangeTokenPrice_Should_Get_Price()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var queryId = await QueryExchangeTokenPrice(token1, token2);
            var price = "1.2345";
            var timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            await RecordExchangeTokenPriceAsync(queryId, token1, token2, price,
                timestamp);
            var priceInfo = await PriceContractStub.GetExchangeTokenPriceInfo.CallAsync(
                new GetExchangeTokenPriceInfoInput
                {
                    Organization = OracleNodes.First(),
                    TokenSymbol = token1,
                    TargetTokenSymbol = token2
                });
            priceInfo.Value.ShouldBe(price);
            priceInfo.Timestamp.ShouldBe(timestamp);
        }
        
        [Fact]
        public async Task RecordSwapTokenPrice_Should_Get_Price()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var queryId = await QuerySwapTokenPrice(token1, token2);
            var price = "1.2345";
            var timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            await RecordSwapTokenPriceAsync(queryId, token1, token2, price,
                timestamp);
            var priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token1,
                    TargetTokenSymbol = token2
                });
            priceInfo.Value.ShouldBe(price);
            priceInfo.Timestamp.ShouldBe(timestamp);
        }

        private async Task<Hash> QueryExchangeTokenPrice(string tokenSymbol, string targetTokenSymbol)
        {
            var result = await PriceContractStub.QueryExchangeTokenPrice.SendAsync(new QueryTokenPriceInput
            {
                DesignatedNodes = {OracleNodes},
                TokenSymbol = tokenSymbol,
                TargetTokenSymbol = targetTokenSymbol
            });
            return result.Output;
        }

        private async Task<Hash> QuerySwapTokenPrice(string tokenSymbol, string targetTokenSymbol)
        {
            var result = await PriceContractStub.QuerySwapTokenPrice.SendAsync(new QueryTokenPriceInput
            {
                DesignatedNodes = {OracleNodes},
                TokenSymbol = tokenSymbol,
                TargetTokenSymbol = targetTokenSymbol
            });
            return result.Output;
        }

        private async Task<IExecutionResult<Empty>> RecordExchangeTokenPriceAsync(Hash queryId, string tokenSymbol, string targetTokenSymbol,
            string price,
            Timestamp timestamp)
        {
            return await OracleContractStub.RecordTokenPrice.SendAsync(new TokenPriceInfo
            {
                CallBackAddress = PriceContractAddress,
                CallBackMethodName = nameof(PriceContractStub.RecordExchangeTokenPrice),
                QueryId = queryId,
                TokenPrice = GenerateTokenPriceInfo(tokenSymbol, targetTokenSymbol, price, timestamp),
                OracleNodes = {OracleNodes}
            });
        }

        private async Task<IExecutionResult<Empty>> RecordSwapTokenPriceAsync(Hash queryId, string tokenSymbol, string targetTokenSymbol,
            string price,
            Timestamp timestamp)
        {
            return await OracleContractStub.RecordTokenPrice.SendAsync(new TokenPriceInfo
            {
                CallBackAddress = PriceContractAddress,
                CallBackMethodName = nameof(PriceContractStub.RecordSwapTokenPrice),
                QueryId = queryId,
                TokenPrice = GenerateTokenPriceInfo(tokenSymbol, targetTokenSymbol, price, timestamp),
                OracleNodes = {OracleNodes}
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