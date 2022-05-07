using AElf.Contracts.MultiToken;
using AElf.Contracts.Oracle;
using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Price
{
    public class PriceContractState : ContractState
    {
        internal OracleContractContainer.OracleContractReferenceState OracleContract { get; set; }
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }
        public SingletonState<Address> Controller { get; set; }
        public Int32State TracePathLimit { get; set; }
        public Int64State QueryFee { get; set; }
        public SingletonState<AddressList> AuthorizedSwapTokenPriceInquirers { get; set; }
        public MappedState<Hash, bool> QueryIdMap { get; set; }
        public MappedState<string, PriceTraceInfo> SwapTokenTraceInfo { get; set; }
        public MappedState<string, Price> SwapTokenPriceInfo { get; set; }
        public MappedState<Address, string, Price> ExchangeTokenPriceInfo { get; set; }
        public StringState UnderlyingTokenSymbol { get; set; }
        public SingletonState<Address> TokenSwapAddress { get; set; }
    }
}