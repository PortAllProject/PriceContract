using AElf.Types;
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
                    Value = "1"
                };
            }

            var tokenKey = GetTokenKey(input.TokenSymbol, input.TargetTokenSymbol, out var isReverse);
            var priceInfo = State.ExchangeTokenPriceInfo[input.Organization][tokenKey];
            if (!isReverse)
            {
                return priceInfo;
            }

            return new Price
            {
                Timestamp = priceInfo.Timestamp,
                Value = GetPriceReciprocalStr(priceInfo.Value)
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
    }
}