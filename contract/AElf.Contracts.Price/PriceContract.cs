using System;
using System.Linq;
using AElf.Contracts.MultiToken;
using AElf.Contracts.Oracle;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Price
{
    public partial class PriceContract : PriceContractContainer.PriceContractBase
    {
        public override Empty Initialize(Address address)
        {
            Assert(State.OracleContract.Value == null, "Already initialized.");
            State.OracleContract.Value = address;
            InitializeSwapUnderlyingToken();
            return new Empty();
        }

        public override Empty RecordSwapTokenPrice(CallbackInput input)
        {
            Assert(Context.Sender == State.OracleContract.Value, "No permission.");
            var tokenPrice = new TokenPrice();
            tokenPrice.MergeFrom(input.Result);
            var originalToken = State.SwapTokenTraceInfo[tokenPrice.TokenSymbol];
            if (originalToken != null &&
                originalToken.TokenList.Any(x => x == tokenPrice.TargetTokenSymbol))
            {
                UpdateTokenPairPrice(tokenPrice.TokenSymbol, tokenPrice.TargetTokenSymbol, tokenPrice.Price);
            }
            else
            {
                AddTokenPair(tokenPrice.TokenSymbol, tokenPrice.TargetTokenSymbol, tokenPrice.Price);
            }
            
            // event
            return new Empty();
        }

        public override Empty RecordExchangeTokenPrice(CallbackInput input)
        {
            Assert(Context.Sender == State.OracleContract.Value, "No permission.");
            var tokenPrice = new TokenPrice();
            tokenPrice.MergeFrom(input.Result);
            var tokenKey = GetTokenKey(tokenPrice.TokenSymbol, tokenPrice.TargetTokenSymbol, out var isReverse);
            var price = isReverse ? GetPriceReciprocalStr(tokenPrice.Price): tokenPrice.Price;
            foreach (var node in input.OracleNodes)
            {
                State.ExchangePriceInfo[node][tokenKey] = price;
            }

            // event
            return new Empty();
        }
        
        private string GetTokenKey(string token1, string token2, out bool isAdjustOrder)
        {
            isAdjustOrder = false;
            var comparision = string.Compare(token1, token2, StringComparison.Ordinal);
            Assert(comparision != 0, $"token1: {token1}, token2: {token2} are same");
            string tokenKey;
            if (comparision < 0)
            {
                isAdjustOrder = true;
                tokenKey = token2 + token1;
            }
            else
            {
                tokenKey = token1 + token2;
            }

            return tokenKey;
        }
    }
}