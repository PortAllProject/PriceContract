namespace AElf.Contracts.Price
{
    public partial class PriceContract
    {
        public const int InfinitePathWeight = int.MaxValue - 1;
        public const string UnderlyingTokenSymbol = "Usdt";
        public const int PriceDecimals = 8;
        public const int Payment = 10_00000000;
        public const int AggregatorOption = 2;
    }
}