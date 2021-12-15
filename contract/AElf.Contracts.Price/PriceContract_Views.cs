using AElf.Types;

namespace AElf.Contracts.Price
{
    public partial class PriceContract
    {
        public override Price GetSwapTokenPriceInfo(GetSwapTokenPriceInfoInput input)
        {
            var tokenKey = GetTokenKey(input.TokenSymbol, input.TargetTokenSymbol, out _);
            if (State.SwapTokenPriceInfo[tokenKey] != null)
            {
                return State.SwapTokenPriceInfo[tokenKey];
            }
            var price = TraceSwapTokenPrice(input.TokenSymbol, input.TargetTokenSymbol);
            return new Price
            {
                Value = price.ToString()
            };
        }

        public override Price GetExchangeTokenPriceInfo(GetExchangeTokenPriceInfoInput input)
        {
            var tokenKey = GetTokenKey(input.TokenSymbol, input.TargetTokenSymbol, out _);
            return State.ExchangeTokenPriceInfo[input.Organization][tokenKey];
        }

        public override IsQueryIdExisted CheckQueryIdIfExisted(Hash input)
        {
            return new IsQueryIdExisted
            {
                Value = State.QueryIdMap[input]
            };
        }
    }
}