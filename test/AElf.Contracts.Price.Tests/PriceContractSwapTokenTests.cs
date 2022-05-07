using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.TestsOracle;
using AElf.ContractTestKit;
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
        public async Task QuerySwapTokenPrice_Without_Controller_Should_Fail()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var unAuthorizedUser = SampleAccount.Accounts.Skip(1).First().KeyPair;
            var unAuthorizedPriceStub = GetPriceContractStub(unAuthorizedUser);
            var txResult = await unAuthorizedPriceStub.QuerySwapTokenPrice.SendWithExceptionAsync(new QueryTokenPriceInput
            {
                DesignatedNodes = {OracleNodes},
                TokenSymbol = token1,
                TargetTokenSymbol = token2
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("UnAuthorized sender");
        }

        [Fact]
        public async Task RecordSwapTokenPrice_Without_Controller_Should_Fail()
        {
            var txResult = await PriceContractStub.RecordSwapTokenPrice.SendWithExceptionAsync(new CallbackInput());
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("No permission");
        }

        [Fact]
        public async Task RecordSwapTokenPrice_Should_Get_Right_Price()
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
            priceInfo.Value.ShouldBe("1.23450000");
            priceInfo.Timestamp.ShouldBe(timestamp);
            
            priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token2,
                    TargetTokenSymbol = token1
                });
            priceInfo.Value.ShouldBe("0.81004455");
        }
        
        [Fact]
        public async Task GetSwapTokenPriceInfo_With_Same_Token_Should_Return_One()
        {
            var token1 = "ELF";
            var priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token1,
                    TargetTokenSymbol = token1
                });
            priceInfo.Value.ShouldBe("1");
        }
        
        [Fact]
        public async Task RecordSwapTokenPrice_Update_Price_Should_Get_Right_Price()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var price = "1.2345";
            var firstTimestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            var secondTimestamp = Timestamp.FromDateTime(DateTime.UtcNow.AddSeconds(1));
            await AddNewPairWithPriceAsync(token1, token2, price, firstTimestamp);
            var newPrice = "1.25000000";
            await AddNewPairWithPriceAsync(token2, token1, newPrice, secondTimestamp);
            
            var priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token1,
                    TargetTokenSymbol = token2
                });
            priceInfo.Value.ShouldBe("0.8");
            
            priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token2,
                    TargetTokenSymbol = token1
                });
            priceInfo.Value.ShouldBe("1.25000000");
        }

        [Fact]
        public async Task RecordSwapTokenPrice_Without_Invalid_Timestamp_Should_Fail()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var price = "1.2345";
            var secondTimestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            var firstTimestamp = Timestamp.FromDateTime(DateTime.UtcNow.AddSeconds(1));
            await AddNewPairWithPriceAsync(token1, token2, price, firstTimestamp);
            
            var queryId = await QuerySwapTokenPrice(token1, token2);
            var newPrice = "1.23450000";
            var txResult = await OracleContractStub.RecordTokenPrice.SendWithExceptionAsync(new TokenPriceInfo
            {
                CallBackAddress = PriceContractAddress,
                CallBackMethodName = nameof(PriceContractStub.RecordSwapTokenPrice),
                QueryId = queryId,
                TokenPrice = GenerateTokenPriceInfo(token1, token2, newPrice, secondTimestamp),
                OracleNodes = {OracleNodes}
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("Expired data");
        }

        [Fact]
        public async Task RecordSwapTokenPrice_Add_Pair_Indirectly_Should_Get_Right_UnderlyingToken_Price()
        {
            var token1 = "ELF";
            var token2 = "LLYP";
            var queryId = await QuerySwapTokenPrice(token1, token2);
            var price1 = "1.23450000";
            var timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            await RecordSwapTokenPriceAsync(queryId, token1, token2, price1,
                timestamp);
            
            queryId = await QuerySwapTokenPrice(token1, UnderlyingTokenSymbol);
            var price2 = "1.23450000";
            timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            await RecordSwapTokenPriceAsync(queryId, token1, UnderlyingTokenSymbol, price2,
                timestamp);
            
            var priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token2,
                    TargetTokenSymbol = UnderlyingTokenSymbol
                });

            priceInfo.Value.ShouldBe("1.00000000");
            
            priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token1,
                    TargetTokenSymbol = UnderlyingTokenSymbol
                });
            
            priceInfo.Value.ShouldBe(price2);
            
            priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = UnderlyingTokenSymbol,
                    TargetTokenSymbol = token1
                });
            
            priceInfo.Value.ShouldBe("0.81004455");
        }

        //     p1
        // GM —— > LLYP
        // | p2      | p3
        // v    p4   v
        // WJC —— > ZX
        // | p5      | p6 
        // v    p7   v
        // ELF —— > USDT
        [Fact]
        public async Task RecordSwapTokenPrice_Add_Multiple_Pair_Indirectly_Should_Get_Right_UnderlyingToken_Price()
        {
            var token1 = "GM";
            var token2 = "LLYP";
            var token3 = "ZX";
            var token4 = "WJC";
            var token5 = "ELF";
            var token6 = UnderlyingTokenSymbol;
            var price1 = "1.23450000";
            var price2 = "0.156";
            var price3 = "6.7890";
            var price4 = "100.5432";
            var price5 = "12.0132";
            var price6 = "0.5000";
            var price7 = "10.0000";
            await AddNewPairWithPriceAsync(token1, token2, price1);
            await AddNewPairWithPriceAsync(token1, token4, price2);
            await AddNewPairWithPriceAsync(token2, token3, price3);
            await AddNewPairWithPriceAsync(token4, token3, price4);
            await AddNewPairWithPriceAsync(token4, token5, price5);
            await AddNewPairWithPriceAsync(token3, token6, price6);
            await AddNewPairWithPriceAsync(token5, token6, price7);

            var toke1Price = await GetSwapUnderlyingTokenPrice(token1);
            toke1Price.ShouldBe("0");
            var toke2Price = await GetSwapUnderlyingTokenPrice(token2);
            toke2Price.ShouldBe("3.39449996");
            var toke3Price = await GetSwapUnderlyingTokenPrice(token3);
            toke3Price.ShouldBe("0.5000");
            var toke4Price = await GetSwapUnderlyingTokenPrice(token4);
            toke4Price.ShouldBe("50.27161755");
            var toke5Price = await GetSwapUnderlyingTokenPrice(token5);
            toke5Price.ShouldBe("10");

            var batchTokenPrice = await PriceContractStub.BatchGetSwapTokenPriceInfo.CallAsync(
                new GetBatchSwapTokenPriceInfoInput
                {
                    TokenPriceQueryList =
                    {
                        new GetSwapTokenPriceInfoInput
                        {
                            TokenSymbol = token1
                        },
                        new GetSwapTokenPriceInfoInput
                        {
                            TokenSymbol = token2
                        }
                    }
                });
            batchTokenPrice.Value[0].Price.ShouldBe("0");
            batchTokenPrice.Value[1].Price.ShouldBe("3.39449996");
        }

        [Fact]
        public async Task RecordSwapTokenPrice_Update_Trace_Path_Without_Controller_Should_Fail()
        {
            var token1 = "GM";
            var token2 = "LLYP";
            var token3 = "ZX";
            await AddNewPairWithPriceAsync(token1, token2, "1");
            await AddNewPairWithPriceAsync(token1, token3, "1");
            var unAuthorizedUser = SampleAccount.Accounts.Skip(1).First().KeyPair;
            var unAuthorizedPriceStub = GetPriceContractStub(unAuthorizedUser);
            var txResult = await unAuthorizedPriceStub.UpdateSwapTokenTraceInfo.SendWithExceptionAsync(new UpdateSwapTokenTraceInfoInput
            {
                TokenSymbol = token1,
                TargetTokenSymbol = token3
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("not controller");
        }
        
        [Fact]
        public async Task RecordSwapTokenPrice_Update_Trace_Path_With_Invalid_Pair_Should_Fail()
        {
            var token1 = "GM";
            var token2 = "LLYP";
            var token3 = "ZX";
            var token4 = "CWJ";
            await AddNewPairWithPriceAsync(token1, token2, "1");
            await AddNewPairWithPriceAsync(token2, token3, "1");
            await AddNewPairWithPriceAsync(token3, UnderlyingTokenSymbol, "1");
            await AddNewPairWithPriceAsync(token4, UnderlyingTokenSymbol, "1");
            var txResult = await PriceContractStub.UpdateSwapTokenTraceInfo.SendWithExceptionAsync(new UpdateSwapTokenTraceInfoInput
            {
                TokenSymbol = token4,
                TargetTokenSymbol = token2
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain($"Pair {token4}-{token2} does not exist");
        }
        
        [Fact]
        public async Task RecordSwapTokenPrice_Update_Trace_Path_With_Invalid_Path_Set_Should_Fail()
        {
            var token1 = "GM";
            var token2 = "LLYP";
            var token3 = "ZX";
            var token4 = "CWJ";
            await AddNewPairWithPriceAsync(token1, token2, "1");
            await AddNewPairWithPriceAsync(token2, token4, "1");
            await AddNewPairWithPriceAsync(token2, token3, "1");
            await AddNewPairWithPriceAsync(token3, UnderlyingTokenSymbol, "1");
            await AddNewPairWithPriceAsync(token4, UnderlyingTokenSymbol, "1");
            var txResult = await PriceContractStub.UpdateSwapTokenTraceInfo.SendWithExceptionAsync(new UpdateSwapTokenTraceInfoInput
            {
                TokenSymbol = token4,
                TargetTokenSymbol = token2
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("Invalid path set for pair");
        }

        [Fact]
        public async Task RecordSwapTokenPrice_Update_Trace_Path_Should_Get_Right_UnderlyingToken_Price()
        {
            var token2 = "LLYP";
            var token3 = "ZX";
            var token4 = "WJC";
            var token5 = "ELF";
            var token6 = UnderlyingTokenSymbol;
            var price3 = "6.7890";
            var price4 = "100.5432";
            var price5 = "12.0132";
            var price6 = "0.5000";
            var price7 = "10.0000";
            await AddNewPairWithPriceAsync(token2, token3, price3);
            await AddNewPairWithPriceAsync(token4, token3, price4);
            await AddNewPairWithPriceAsync(token4, token5, price5);
            await AddNewPairWithPriceAsync(token3, token6, price6);
            await AddNewPairWithPriceAsync(token5, token6, price7);
            
            var toke4Price = await GetSwapUnderlyingTokenPrice(token4);
            toke4Price.ShouldBe("50.27161755");
            await PriceContractStub.UpdateSwapTokenTraceInfo.SendAsync(new UpdateSwapTokenTraceInfoInput
            {
                TokenSymbol = token4,
                TargetTokenSymbol = token5
            });
            toke4Price = await GetSwapUnderlyingTokenPrice(token4);
            toke4Price.ShouldBe("120.1320");
        }
        
        [Fact]
        public async Task RecordSwapTokenPrice_With_Same_Token_Symbol_Should_Fail()
        {
            var token1 = "GM";
            var txResult = await PriceContractStub.QuerySwapTokenPrice.SendWithExceptionAsync(new QueryTokenPriceInput
            {
                DesignatedNodes = {OracleNodes},
                TokenSymbol = token1,
                TargetTokenSymbol = token1
            });
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("token1: GM, token2: GM are same");
        }

        [Fact]
        public async Task GetSwapTokenPriceInfo_Without_Trace_Underlying_Token_Should_Return_Zero()
        {
            var token1 = "GM";
            var token2 = "LLYP";
            await AddNewPairWithPriceAsync(token1, token2, "1");
            var priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = token1,
                    TargetTokenSymbol = UnderlyingTokenSymbol
                });
            priceInfo.Value.ShouldBe("0");

            var swapTokenInfo = await PriceContractStub.GetSwapTokenInfo.CallAsync(new GetSwapTokenInfoInput
            {
                Token = token1
            });
            swapTokenInfo.TokenList.Count.ShouldBe(1);
            swapTokenInfo.TokenList[0].ShouldBe(token2);
            swapTokenInfo.TracedToken.ShouldBeEmpty();
        }

        [Fact]
        public async Task UpdateAuthorizedSwapTokenPriceQueryUsers_Without_Controller_Should_Fail()
        {
            var unAuthorizedUser = SampleAccount.Accounts.Skip(1).First().KeyPair;
            var unAuthorizedPriceStub = GetPriceContractStub(unAuthorizedUser);
            var newUserList = new AddressList
            {
                Value = {OracleNodes}
            };

            var txResult = await unAuthorizedPriceStub.UpdateAuthorizedSwapTokenPriceQueryUsers.SendWithExceptionAsync(newUserList);
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
            txResult.TransactionResult.Error.ShouldContain("not controller");
        }

        [Fact]
        public async Task UpdateAuthorizedSwapTokenPriceQueryUsers_Test()
        {
            var newUserList = new AddressList
            {
                Value = {OracleNodes}
            };

            await PriceContractStub.UpdateAuthorizedSwapTokenPriceQueryUsers.SendAsync(newUserList);
            var userList = await PriceContractStub.GetAuthorizedSwapTokenPriceQueryUsers.CallAsync(new Empty());
            userList.Value.Count.ShouldBe(newUserList.Value.Count);
            userList.Value.All(x => newUserList.Value.Contains(x)).ShouldBeTrue();
        }

        private async Task AddNewPairWithPriceAsync(string token1, string token2, string price,
            Timestamp timestamp = null)
        {
            var queryId = await QuerySwapTokenPrice(token1, token2);
            timestamp ??= Timestamp.FromDateTime(DateTime.UtcNow);
            await RecordSwapTokenPriceAsync(queryId, token1, token2, price,
                timestamp);
        }

        private async Task<string> GetSwapUnderlyingTokenPrice(string tokenSymbol)
        {
            var priceInfo = await PriceContractStub.GetSwapTokenPriceInfo.CallAsync(
                new GetSwapTokenPriceInfoInput
                {
                    TokenSymbol = tokenSymbol,
                    TargetTokenSymbol = UnderlyingTokenSymbol
                });
            return priceInfo.Value;
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