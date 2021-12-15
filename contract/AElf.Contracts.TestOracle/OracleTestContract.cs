using AElf.Contracts.Oracle;
using AElf.Contracts.TestsOracle;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.TestOracle
{
    public class OracleTestContract : OracleTestContractContainer.OracleTestContractBase
    {
        public override Hash Query(QueryInput input)
        {
            return Hash.Empty;
        }

        public override Empty RecordTokenPrice(TokenPriceInfo input)
        {
            var callbackInfo = new CallbackInput
            {
                QueryId = input.QueryId,
                OracleNodes = {input.OracleNodes},
                Result = input.TokenPrice.ToByteString()
            };
            Context.SendInline(input.CallBackAddress, input.CallBackMethodName, callbackInfo);
            return new Empty();
        }

        public override StringValue GetOracleTokenSymbol(Empty input)
        {
            return new StringValue
            {
                Value = "PORT"
            };
        }
    }
}