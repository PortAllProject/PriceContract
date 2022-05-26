using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.Types;
using Awaken.Contracts.Swap;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Price
{
    public partial class PriceContract
    {
        public override Price GetSwapTokenPriceInfo(GetSwapTokenPriceInfoInput input)
        {
            AssertValidToken(input.TokenSymbol);
            if (string.IsNullOrEmpty(input.TargetTokenSymbol))
            {
                input.TargetTokenSymbol = State.UnderlyingTokenSymbol.Value;
            }

            if (input.TargetTokenSymbol == input.TokenSymbol)
            {
                return new Price
                {
                    Value = "1"
                };
            }

            var tokenKey = GetTokenKey(input.TokenSymbol, input.TargetTokenSymbol, out _);
            Timestamp timestamp = null;
            if (State.SwapTokenPriceInfo[tokenKey] != null)
            {
                timestamp = State.SwapTokenPriceInfo[tokenKey].Timestamp;
            }

            var limitPath = State.TracePathLimit.Value;
            var price = TraceSwapTokenPrice(input.TokenSymbol, input.TargetTokenSymbol, ref limitPath);
            return new Price
            {
                Value = price.ToString(),
                Timestamp = timestamp
            };
        }

        public override TokenPriceList BatchGetSwapTokenPriceInfo(GetBatchSwapTokenPriceInfoInput input)
        {
            var result = new TokenPriceList();
            foreach (var tokenPriceQuery in input.TokenPriceQueryList)
            {
                var priceInfo = GetSwapTokenPriceInfo(tokenPriceQuery);
                result.Value.Add(new TokenPrice
                {
                    TokenSymbol = tokenPriceQuery.TokenSymbol,
                    TargetTokenSymbol = tokenPriceQuery.TargetTokenSymbol,
                    Price = priceInfo.Value,
                    Timestamp = priceInfo.Timestamp
                });
            }

            return result;
        }

        public override Price GetExchangeTokenPriceInfo(GetExchangeTokenPriceInfoInput input)
        {
            AssertValidToken(input.TokenSymbol);
            if (string.IsNullOrEmpty(input.TargetTokenSymbol))
            {
                input.TargetTokenSymbol = State.UnderlyingTokenSymbol.Value;
            }

            if (input.TargetTokenSymbol == input.TokenSymbol)
            {
                return new Price
                {
                    Value = Mantissa.ToString()
                };
            }

            var reservesInput = new GetReservesInput
            {
                SymbolPair = { $"{input.TokenSymbol}-{input.TargetTokenSymbol}" }
            }.ToByteString();
            var tokenReserves = Context.Call<GetReservesOutput>(Context.Self, State.TokenSwapAddress.Value,
                nameof(AwakenSwapContractContainer.AwakenSwapContractReferenceState.GetReserves), reservesInput);
            Assert(tokenReserves.Results.Count == 1,
                $"Token Pair does not exist: {input.TokenSymbol}-{input.TargetTokenSymbol}");
            var tokenPair = tokenReserves.Results[0];
            var tokenADecimals = State.TokenContract.GetTokenInfo.Call(new GetTokenInfoInput
            {
                Symbol = tokenPair.SymbolA
            }).Decimals;
            var tokenBDecimals = State.TokenContract.GetTokenInfo.Call(new GetTokenInfoInput
            {
                Symbol = tokenPair.SymbolB
            }).Decimals;
            if (input.TargetTokenSymbol == tokenPair.SymbolA)
            {
                return new Price
                {
                    Timestamp = new Timestamp(),
                    Value = GetMantissaPrice(tokenPair.ReserveB, tokenPair.ReserveA, tokenBDecimals, tokenADecimals)
                };
            }
            
            return new Price
            {
                Timestamp = new Timestamp(),
                Value = GetMantissaPrice(tokenPair.ReserveA, tokenPair.ReserveB, tokenADecimals, tokenBDecimals)
            };
        }

        public override TokenPriceList BatchGetExchangeTokenPriceInfo(GetBatchExchangeTokenPriceInfoInput input)
        {
            var result = new TokenPriceList();
            foreach (var tokenPriceQuery in input.TokenPriceQueryList)
            {
                var priceInfo = GetExchangeTokenPriceInfo(tokenPriceQuery);
                result.Value.Add(new TokenPrice
                {
                    TokenSymbol = tokenPriceQuery.TokenSymbol,
                    TargetTokenSymbol = tokenPriceQuery.TargetTokenSymbol,
                    Price = priceInfo.Value,
                    Timestamp = priceInfo.Timestamp
                });
            }

            return result;
        }

        public override IsQueryIdExisted CheckQueryIdIfExisted(Hash input)
        {
            return new IsQueryIdExisted
            {
                Value = State.QueryIdMap[input]
            };
        }

        public override AddressList GetAuthorizedSwapTokenPriceQueryUsers(Empty input)
        {
            return State.AuthorizedSwapTokenPriceInquirers.Value;
        }

        public override TracePathLimit GetTracePathLimit(Empty input)
        {
            return new TracePathLimit
            {
                PathLimit = State.TracePathLimit.Value
            };
        }

        public override UnderlyingToken GetUnderlyingToken(Empty input)
        {
            return new UnderlyingToken
            {
                Value = State.UnderlyingTokenSymbol.Value
            };
        }

        public override Address GetTokenSwapAddress(Empty input)
        {
            return State.TokenSwapAddress.Value;
        }

        public override PriceTraceInfo GetSwapTokenInfo(GetSwapTokenInfoInput input)
        {
            return State.SwapTokenTraceInfo[input.Token];
        }

        public override Address GetController(Empty input)
        {
            return State.Controller.Value;
        }

        public override Address GetOracle(Empty input)
        {
            return State.OracleContract.Value;
        }

        public override QueryFee GetQueryFee(Empty input)
        {
            return new QueryFee
            {
                Fee = State.QueryFee.Value
            };
        }

        public static string GetMantissaPrice(BigIntValue tokenReserve, BigIntValue targetTokenReserve, int tokenDecimals,
            int reserveTokenDecimals)
        {
            var price = tokenReserve.Mul(GetDecimalMultiplier(reserveTokenDecimals)).Mul(Mantissa)
                .Div(GetDecimalMultiplier(tokenDecimals)).Div(targetTokenReserve);
            return price.Value;
        }

        public static long GetDecimalMultiplier(int decimals)
        {
            long multiplier = 1;
            while (decimals -- > 0)
            {
                multiplier *= 10;
            }

            return multiplier;
        }
    }
}