namespace AElf.Contracts.Price
{
    public partial class PriceContract
    {
        private const int InfinitePathWeight = int.MaxValue - 1;
        private const string DefaultUnderlyingTokenSymbol = "USDT";
        private const int PriceDecimals = 8;
        private const int Payment = 10_000_000;
        private const int AggregatorOption = 2;
        private const int MaxTracePathLimit = 3;
    }
}