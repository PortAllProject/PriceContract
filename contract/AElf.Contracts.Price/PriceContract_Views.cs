namespace AElf.Contracts.Price
{
    public partial class PriceContract
    {
        public override Price GetSwapTokenPriceInfo(GetSwapTokenPriceInfoInput input)
        {
            var price = TraceSwapTokenPrice(input.TokenSymbol, input.TargetTokenSymbol);
            return new Price
            {
                Value = price.ToString()
            };
        }

        public override Price GetExchangeTokenPriceInfo(GetExchangeTokenPriceInfoInput input)
        {
            var tokenKey = GetTokenKey(input.TokenSymbol, input.TargetTokenSymbol, out _);
            return State.ExchangePriceInfo[input.Organization][tokenKey];
        }
    }
}