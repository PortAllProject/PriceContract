using System;
using System.Collections.Generic;
using System.Linq;
using AElf.Contracts.MultiToken;
using AElf.Contracts.Oracle;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Price
{
    public partial class PriceContract : PriceContractContainer.PriceContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            Assert(State.OracleContract.Value == null, "Already initialized.");
            State.OracleContract.Value = input.OracleAddress;
            if (State.TokenContract.Value == null)
            {
                State.TokenContract.Value =
                    Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            }

            var authorizedUsers = input.AuthorizedUsers.Any()
                ? input.AuthorizedUsers
                : new RepeatedField<Address> {input.Controller};
            State.AuthorizedSwapTokenPriceQueryUsers.Value = new AuthorizedSwapTokenPriceQueryUsers
            {
                List = {authorizedUsers}
            };
            InitializeSwapUnderlyingToken();
            State.Controller.Value = input.Controller;
            State.TracePathLimit.Value = input.TracePathLimit > 0 ? input.TracePathLimit : 2;
            return new Empty();
        }

        public override Hash QuerySwapTokenPrice(QueryTokenPriceInput input)
        {
            var authorizedUsers = State.AuthorizedSwapTokenPriceQueryUsers.Value.List;
            Assert(authorizedUsers.Contains(Context.Sender), $"UnAuthorized sender {Context.Sender}");
            const string title = "TokenSwapPrice";
            var options = new List<string> {$"{input.TokenSymbol}-{input.TargetTokenSymbol}"};
            var queryId = CreateQuery(input, title, options, nameof(RecordSwapTokenPrice));
            State.QueryIdMap[queryId] = true;
            return queryId;
        }

        public override Hash QueryExchangeTokenPrice(QueryTokenPriceInput input)
        {
            const string title = "ExchangeTokenPrice";
            var options = new List<string> {$"{input.TokenSymbol}-{input.TargetTokenSymbol}"};
            var queryId = CreateQuery(input, title, options, nameof(RecordExchangeTokenPrice));
            State.QueryIdMap[queryId] = true;
            return queryId;
        }

        public override Empty RecordSwapTokenPrice(CallbackInput input)
        {
            Assert(Context.Sender == State.OracleContract.Value, "No permission.");
            Assert(State.QueryIdMap[input.QueryId], $"Query ID:{input.QueryId} does not exist");
            State.QueryIdMap.Remove(input.QueryId);
            var tokenPrice = new TokenPrice();
            tokenPrice.MergeFrom(input.Result);
            var originalToken = State.SwapTokenTraceInfo[tokenPrice.TokenSymbol];
            if (originalToken != null &&
                originalToken.TokenList.Any(x => x == tokenPrice.TargetTokenSymbol))
            {
                UpdateTokenPairPrice(tokenPrice.TokenSymbol, tokenPrice.TargetTokenSymbol, new Price
                {
                    Value = tokenPrice.Price,
                    Timestamp = tokenPrice.Timestamp
                });
            }
            else
            {
                AddTokenPair(tokenPrice.TokenSymbol, tokenPrice.TargetTokenSymbol, tokenPrice.Price,
                    tokenPrice.Timestamp);
            }

            Context.Fire(new NewestSwapPriceUpdated
            {
                TokenSymbol = tokenPrice.TokenSymbol,
                TargetTokenSymbol = tokenPrice.TargetTokenSymbol,
                Price = tokenPrice.Price,
                Timestamp = tokenPrice.Timestamp,
                QueryId = input.QueryId,
            });
            return new Empty();
        }

        public override Empty RecordExchangeTokenPrice(CallbackInput input)
        {
            Assert(Context.Sender == State.OracleContract.Value, "No permission.");
            Assert(State.QueryIdMap[input.QueryId], $"Query ID:{input.QueryId} does not exist");
            State.QueryIdMap.Remove(input.QueryId);
            var tokenPrice = new TokenPrice();
            tokenPrice.MergeFrom(input.Result);
            var tokenKey = GetTokenKey(tokenPrice.TokenSymbol, tokenPrice.TargetTokenSymbol, out var isReverse);
            var price = isReverse ? GetPriceReciprocalStr(tokenPrice.Price) : tokenPrice.Price;
            foreach (var node in input.OracleNodes)
            {
                var currentPriceInfo = State.ExchangeTokenPriceInfo[node][tokenKey];
                if (currentPriceInfo != null)
                {
                    AssertValidTimestamp(tokenPrice.TokenSymbol, tokenPrice.TargetTokenSymbol,
                        currentPriceInfo.Timestamp, tokenPrice.Timestamp);
                }

                State.ExchangeTokenPriceInfo[node][tokenKey] = new Price
                {
                    Value = price,
                    Timestamp = tokenPrice.Timestamp
                };
            }

            Context.Fire(new NewestExchangePriceUpdated
            {
                TokenSymbol = tokenPrice.TokenSymbol,
                TargetTokenSymbol = tokenPrice.TargetTokenSymbol,
                Price = tokenPrice.Price,
                Timestamp = tokenPrice.Timestamp,
                PriceSupplier = new OrganizationList
                {
                    NodeList = {input.OracleNodes}
                },
                QueryId = input.QueryId
            });
            return new Empty();
        }

        public override Empty UpdateSwapTokenTraceInfo(UpdateSwapTokenTraceInfoInput input)
        {
            Assert(State.Controller.Value == Context.Sender, $"Invalid sender: {Context.Sender}");
            AssignTokenPriceTraceInfo(input.TokenSymbol, input.TargetTokenSymbol);
            return new Empty();
        }

        public override Empty UpdateAuthorizedSwapTokenPriceQueryUsers(AuthorizedSwapTokenPriceQueryUsers input)
        {
            Assert(State.Controller.Value == Context.Sender, $"Invalid sender: {Context.Sender}");
            State.AuthorizedSwapTokenPriceQueryUsers.Value = input;
            return new Empty();
        }

        public override Empty ChangeOracle(ChangeOracleInput input)
        {
            Assert(State.Controller.Value == Context.Sender, $"Invalid sender: {Context.Sender}");
            State.OracleContract.Value = input.Oracle;
            return new Empty();
        }

        public override Empty ChangeTracePathLimit(ChangeTracePathLimitInput input)
        {
            Assert(State.Controller.Value == Context.Sender, $"Invalid sender: {Context.Sender}");
            Assert(input.NewPathLimit > 0 && input.NewPathLimit <= MaxTracePathLimit,
                $"Invalid input: {input.NewPathLimit}, trace path limit should be greater than 0 and less than {MaxTracePathLimit}");
            State.TracePathLimit.Value = input.NewPathLimit;
            return new Empty();
        }

        private Hash CreateQuery(QueryTokenPriceInput input, string title, IEnumerable<string> options, string callback)
        {
            State.TokenContract.Approve.Send(new ApproveInput
            {
                Spender = State.OracleContract.Value,
                Amount = Payment,
                Symbol = State.OracleContract.GetOracleTokenSymbol.Call(new Empty()).Value
            });

            var queryInput = new QueryInput
            {
                AggregatorContractAddress = input.AggregatorContractAddress,
                QueryInfo = new QueryInfo
                {
                    Title = title,
                    Options = {options}
                },
                CallbackInfo = new CallbackInfo
                {
                    ContractAddress = Context.Self,
                    MethodName = callback
                },
                DesignatedNodeList = new AddressList {Value = {input.DesignatedNodes}},
                Payment = Payment,
                AggregateThreshold = input.AggregateThreshold,
                AggregateOption = AggregatorOption
            };
            State.OracleContract.Query.Send(queryInput);

            var queryIdFromHash = HashHelper.ComputeFrom(queryInput);
            var queryId = Context.GenerateId(State.OracleContract.Value, queryIdFromHash);
            return queryId;
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
        
        private void AssertValidTimestamp(string token1, string token2, Timestamp currentTimestamp, Timestamp newTimestamp)
        {
            Assert(currentTimestamp < newTimestamp,
                $"Expired data for pair {token1}:{token2}. Data timestamp in contract: {currentTimestamp}; record data timestamp: {newTimestamp}");
        }
    }
}