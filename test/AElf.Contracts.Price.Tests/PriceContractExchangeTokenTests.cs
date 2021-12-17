using System;
using System.Linq;
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
        public async Task QueryExchangeTokenPrice_QueryId_Should_Be_Logged()
        {
            var queryId = await QueryExchangeTokenPrice("ELF", "LLYP");
            var isQueryIdLogged = await PriceContractStub.CheckQueryIdIfExisted.CallAsync(queryId);
            isQueryIdLogged.Value.ShouldBeTrue();
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
        
        private async Task RecordExchangeTokenPriceAsync(Hash queryId, string tokenSymbol, string targetTokenSymbol,
            string price,
            Timestamp timestamp)
        {
            await OracleContractStub.RecordTokenPrice.SendAsync(new TokenPriceInfo
            {
                CallBackAddress = PriceContractAddress,
                CallBackMethodName = nameof(PriceContractStub.RecordExchangeTokenPrice),
                QueryId = queryId,
                TokenPrice = GenerateTokenPriceInfo(tokenSymbol, targetTokenSymbol, price, timestamp),
                OracleNodes = {OracleNodes}
            });
        }
    }
}