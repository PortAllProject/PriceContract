﻿using AElf.Contracts.MultiToken;
using AElf.Contracts.Oracle;
using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace AElf.Contracts.Price
{
    public class PriceContractState : ContractState
    {
        internal OracleContractContainer.OracleContractReferenceState OracleContract { get; set; }
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }

        public SingletonState<Address> Controller { get; set; }
        public MappedState<Hash, bool> QueryIdMap { get; set; }
        public MappedState<string, PriceTraceInfo> SwapTokenTraceInfo { get; set; }
        public MappedState<string, Price> SwapTokenPriceInfo { get; set; }

        public MappedState<Address, string, Price> ExchangePriceInfo { get; set; }
        // public MappedState<Hash, Hash> OrganizationMap { get; set; }
        // public MappedState<Hash, string, string> ExchangePriceInfo { get; set; }
    }
}