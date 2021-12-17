using System;
using System.Threading.Tasks;
using AElf.Contracts.TestsOracle;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.Price.Test
{
    public partial class PriceContractTests
    {
        [Fact]
        public async Task QuerySwapTokenPrice_QueryId_Should_Be_Logged()
        {
            var queryId = await QuerySwapTokenPrice("ELF", "LLYP");
            var isQueryIdLogged = await PriceContractStub.CheckQueryIdIfExisted.CallAsync(queryId);
            isQueryIdLogged.Value.ShouldBeTrue();
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

        private async Task RecordSwapTokenPriceAsync(Hash queryId, string tokenSymbol, string targetTokenSymbol,
            string price,
            Timestamp timestamp)
        {
            await OracleContractStub.RecordTokenPrice.SendAsync(new TokenPriceInfo
            {
                CallBackAddress = PriceContractAddress,
                CallBackMethodName = nameof(PriceContractStub.RecordSwapTokenPrice),
                QueryId = queryId,
                TokenPrice = GenerateTokenPriceInfo(tokenSymbol, targetTokenSymbol, price, timestamp),
                OracleNodes = {OracleNodes}
            });
        }
    }
}