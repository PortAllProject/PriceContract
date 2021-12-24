using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Price
{
    public partial class PriceContract
    {
        private void InitializeSwapUnderlyingToken()
        {
            State.SwapTokenTraceInfo[UnderlyingTokenSymbol] = new PriceTraceInfo
            {
                TracedPathWeight = 0
            };
        }

        private void AssertValidToken(string originalSymbol, string targetTokenSymbol)
        {
        }

        private void AddTokenPair(string originalSymbol, string targetTokenSymbol, string price, Timestamp timestamp)
        {
            AssertValidToken(originalSymbol, targetTokenSymbol);
            var originalTokenInfo = State.SwapTokenTraceInfo[originalSymbol] ?? new PriceTraceInfo
            {
                TracedPathWeight = InfinitePathWeight
            };
            Assert(originalTokenInfo.TokenList.All(t => t != targetTokenSymbol),
                $"Pair: {originalSymbol}-{targetTokenSymbol} has been added");
            var targetTokenInfo = State.SwapTokenTraceInfo[targetTokenSymbol] ?? new PriceTraceInfo
            {
                TracedPathWeight = InfinitePathWeight
            };
            originalTokenInfo.TokenList.Add(targetTokenSymbol);

            targetTokenInfo.TokenList.Add(originalSymbol);

            State.SwapTokenTraceInfo[targetTokenSymbol] = targetTokenInfo;
            State.SwapTokenTraceInfo[originalSymbol] = originalTokenInfo;
            var tokenKey = GetTokenKey(originalSymbol, targetTokenSymbol, out var isReverse);
            State.SwapTokenPriceInfo[tokenKey] = new Price
            {
                Value = !isReverse ? price : GetPriceReciprocalStr(price),
                Timestamp = timestamp
            };
            UpdateTokenPriceTraceInfo(originalSymbol, targetTokenSymbol);
        }

        private void UpdateTokenPairPrice(string originalTokenSymbol, string targetTokenSymbol, Price price)
        {
            var tokenKey = GetTokenKey(originalTokenSymbol, targetTokenSymbol, out var isReverse);
            var currentTokenPrice = State.SwapTokenPriceInfo[tokenKey];
            AssertValidTimestamp(originalTokenSymbol, targetTokenSymbol, currentTokenPrice.Timestamp, price.Timestamp);
            var priceValue = isReverse ? GetPriceReciprocalStr(price.Value) : price.Value;
            State.SwapTokenPriceInfo[tokenKey] = new Price
            {
                Value = priceValue,
                Timestamp = price.Timestamp
            };
        }

        private void UpdateTokenPriceTraceInfo(string originalTokenSymbol, string targetSymbol)
        {
            var originalTokenInfo = State.SwapTokenTraceInfo[originalTokenSymbol];
            var targetSymbolTokenInfo = State.SwapTokenTraceInfo[targetSymbol];
            var targetTokenPathWeight = targetSymbolTokenInfo.TracedPathWeight;
            if (originalTokenInfo.TracedPathWeight == targetTokenPathWeight ||
                originalTokenInfo.TracedPathWeight == targetTokenPathWeight + 1)
            {
                return;
            }

            if (originalTokenInfo.TracedPathWeight < targetTokenPathWeight)
            {
                UpdateTokenPriceTraceInfo(targetSymbol, originalTokenSymbol);
                return;
            }

            originalTokenInfo.TracedToken = targetSymbol;
            originalTokenInfo.TracedPathWeight = targetTokenPathWeight + 1;
            State.SwapTokenTraceInfo[originalTokenSymbol] = originalTokenInfo;

            foreach (var neighbourToken in originalTokenInfo.TokenList)
            {
                if (neighbourToken == targetSymbol)
                {
                    continue;
                }

                UpdateTokenPriceTraceInfo(neighbourToken, originalTokenSymbol);
            }
        }

        private void AssignTokenPriceTraceInfo(string originalTokenSymbol, string targetSymbol)
        {
            var originalTokenInfo = State.SwapTokenTraceInfo[originalTokenSymbol];
            Assert(originalTokenInfo.TokenList.Contains(targetSymbol),
                $"Pair {originalTokenSymbol}-{targetSymbol} does not exist");
            originalTokenInfo.TracedToken = targetSymbol;
            State.SwapTokenTraceInfo[originalTokenSymbol] = originalTokenInfo;
            if (originalTokenInfo.TracedPathWeight == InfinitePathWeight)
            {
                return;
            }

            var targetSymbolTokenInfo = State.SwapTokenTraceInfo[targetSymbol];
            Assert(originalTokenInfo.TracedPathWeight == targetSymbolTokenInfo.TracedPathWeight + 1,
                $"Invalid path set for pair {originalTokenSymbol}-{targetSymbol}");
        }

        private decimal TraceSwapTokenPrice(string originalTokenSymbol, string targetTokenSymbol)
        {
            var originalTokenTraceInfo = State.SwapTokenTraceInfo[originalTokenSymbol];
            Assert(originalTokenTraceInfo != null, $"Token:{originalTokenSymbol} does not exist");
            if (originalTokenSymbol == targetTokenSymbol)
            {
                return 1m;
            }

            if (targetTokenSymbol != UnderlyingTokenSymbol)
                return decimal.Round(GetTracedTokenPrice(originalTokenSymbol, targetTokenSymbol), PriceDecimals);

            if (originalTokenTraceInfo.TracedPathWeight == InfinitePathWeight ||
                originalTokenTraceInfo.TracedPathWeight > State.TracePathLimit.Value)
            {
                return 0m;
            }

            var nextTokenSymbol = originalTokenTraceInfo.TracedToken;
            var price = GetTracedTokenPrice(originalTokenSymbol, nextTokenSymbol);
            return decimal.Round(price * TraceSwapTokenPrice(nextTokenSymbol, targetTokenSymbol), PriceDecimals);
        }

        private decimal GetTracedTokenPrice(string originalTokenSymbol, string nextTokenSymbol)
        {
            var tokenKey = GetTokenKey(originalTokenSymbol, nextTokenSymbol, out var isReverse);
            var price = State.SwapTokenPriceInfo[tokenKey];
            return isReverse ? GetPriceReciprocal(price.Value) : decimal.Parse(price.Value);
        }

        private decimal GetPriceReciprocal(string price)
        {
            return 1 / decimal.Parse(price);
        }

        private string GetPriceReciprocalStr(string price)
        {
            return decimal.Round(GetPriceReciprocal(price), PriceDecimals).ToString();
        }
    }
}