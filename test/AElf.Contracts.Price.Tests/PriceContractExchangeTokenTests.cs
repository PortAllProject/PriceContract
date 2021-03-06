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
        public async Task RecordExchangeTokenPrice_Without_Controller_Should_Fail()
        {
            var txResult = await PriceContractStub.RecordExchangeTokenPrice.SendWithExceptionAsync(new CallbackInput());
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("No permission");
        }

        [Fact]
        public async Task RecordExchangeTokenPrice_Should_Get_Right_Price()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var queryId = await QueryExchangeTokenPrice(token1, token2);
            var price = "1.23450000";
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
            
            priceInfo = await PriceContractStub.GetExchangeTokenPriceInfo.CallAsync(
                new GetExchangeTokenPriceInfoInput
                {
                    Organization = OracleNodes.First(),
                    TokenSymbol = token2,
                    TargetTokenSymbol = token1
                });
            priceInfo.Value.ShouldBe("0.81004455");
        }

        [Fact]
        public async Task GetBatchExchangeTokenPriceInfo_Should_Get_Right_Price()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var token3 = "LYY";
            var queryId1 = await QueryExchangeTokenPrice(token1, token2);
            var queryId2 = await QueryExchangeTokenPrice(token2, token3);
            var price1 = "1";
            var price2 = "2";
            var timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            await RecordExchangeTokenPriceAsync(queryId1, token1, token2, price1,
                timestamp);
            await RecordExchangeTokenPriceAsync(queryId2, token2, token3, price2,
                timestamp);
            var organization = OracleNodes.First();
            var batchTokenPrice = await PriceContractStub.BatchGetExchangeTokenPriceInfo.CallAsync(
                new GetBatchExchangeTokenPriceInfoInput
                {
                    TokenPriceQueryList =
                    {
                        new GetExchangeTokenPriceInfoInput
                        {
                            Organization = organization,
                            TokenSymbol = token1,
                            TargetTokenSymbol = token2
                        },
                        new GetExchangeTokenPriceInfoInput
                        {
                            Organization = organization,
                            TokenSymbol = token2,
                            TargetTokenSymbol = token3
                        }
                    }
                });
            batchTokenPrice.Value[0].Price.ShouldBe(price1);
            batchTokenPrice.Value[1].Price.ShouldBe(price2);
        }

        [Fact]
        public async Task RecordExchangeTokenPrice_With_Invalid_Timestamp_Should_Fail()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var queryId = await QueryExchangeTokenPrice(token1, token2);
            var price = "1.23450000";
            var now = DateTime.UtcNow;
            var firstTimestamp = Timestamp.FromDateTime(now.AddSeconds(10));
            var secondTimestamp = Timestamp.FromDateTime(now);
            await RecordExchangeTokenPriceAsync(queryId, token1, token2, price,
                firstTimestamp);
            queryId = await QueryExchangeTokenPrice(token1, token2);
            var txResult = await OracleContractStub.RecordTokenPrice.SendWithExceptionAsync(new TokenPriceInfo
            {
                CallBackAddress = PriceContractAddress,
                CallBackMethodName = nameof(PriceContractStub.RecordExchangeTokenPrice),
                QueryId = queryId,
                TokenPrice = GenerateTokenPriceInfo(token1, token2, price, secondTimestamp),
                OracleNodes = {OracleNodes}
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("Expired data");
        }
        
        [Fact]
        public async Task RecordExchangeTokenPrice_Add_Reciprocal_Price_Should_Get_Right_Price()
        {
            var token1 = "LLYP";
            var token2 = "ELF";
            var queryId = await QueryExchangeTokenPrice(token1, token2);
            var price = "0.81004455";
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
            
            priceInfo = await PriceContractStub.GetExchangeTokenPriceInfo.CallAsync(
                new GetExchangeTokenPriceInfoInput
                {
                    Organization = OracleNodes.First(),
                    TokenSymbol = token2,
                    TargetTokenSymbol = token1
                });
            priceInfo.Value.ShouldBe("1.23450000");
        }
        
        [Fact]
        public async Task GetExchangeTokenPriceInfo_With_Same_Token_Should_Return_One()
        {
            var token1 = "ELF";
            var priceInfo = await PriceContractStub.GetExchangeTokenPriceInfo.CallAsync(
                new GetExchangeTokenPriceInfoInput
                {
                    TokenSymbol = token1,
                    TargetTokenSymbol = token1
                });
            priceInfo.Value.ShouldBe("1");
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