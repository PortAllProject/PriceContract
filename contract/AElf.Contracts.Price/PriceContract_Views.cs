using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Price
{
    public partial class PriceContract
    {
        public override Price GetSwapTokenPriceInfo(GetSwapTokenPriceInfoInput input)
        {
            if (string.IsNullOrEmpty(input.TargetTokenSymbol))
            {
                input.TargetTokenSymbol = UnderlyingTokenSymbol;
            }

            var tokenKey = GetTokenKey(input.TokenSymbol, input.TargetTokenSymbol, out _);
            Timestamp timestamp = null;
            if (State.SwapTokenPriceInfo[tokenKey] != null)
            {
                timestamp = State.SwapTokenPriceInfo[tokenKey].Timestamp;
            }

            var price = TraceSwapTokenPrice(input.TokenSymbol, input.TargetTokenSymbol);
            return new Price
            {
                Value = price.ToString(),
                Timestamp = timestamp
            };
        }

        public override Price GetExchangeTokenPriceInfo(GetExchangeTokenPriceInfoInput input)
        {
            if (string.IsNullOrEmpty(input.TargetTokenSymbol))
            {
                input.TargetTokenSymbol = UnderlyingTokenSymbol;
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

        public override IsQueryIdExisted CheckQueryIdIfExisted(Hash input)
        {
            return new IsQueryIdExisted
            {
                Value = State.QueryIdMap[input]
            };
        }

        public override AuthorizedSwapTokenPriceQueryUsers GetAuthorizedSwapTokenPriceQueryUsers(Empty input)
        {
            return State.AuthorizedSwapTokenPriceQueryUsers.Value;
        }
    }
}