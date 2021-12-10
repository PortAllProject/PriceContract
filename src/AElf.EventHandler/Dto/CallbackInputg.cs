// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: callback_input.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from callback_input.proto</summary>
public static partial class CallbackInputReflection {

  #region Descriptor
  /// <summary>File descriptor for callback_input.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static CallbackInputReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "ChRjYWxsYmFja19pbnB1dC5wcm90bxoPYWVsZi9jb3JlLnByb3RvIj0KDUNh",
          "bGxiYWNrSW5wdXQSHAoIcXVlcnlfaWQYASABKAsyCi5hZWxmLkhhc2gSDgoG",
          "cmVzdWx0GAIgASgMYgZwcm90bzM="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { global::AElf.Types.CoreReflection.Descriptor, },
        new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::CallbackInput), global::CallbackInput.Parser, new[]{ "QueryId", "Result" }, null, null, null, null)
        }));
  }
  #endregion

}
#region Messages
public sealed partial class CallbackInput : pb::IMessage<CallbackInput>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    , pb::IBufferMessage
#endif
{
  private static readonly pb::MessageParser<CallbackInput> _parser = new pb::MessageParser<CallbackInput>(() => new CallbackInput());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<CallbackInput> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::CallbackInputReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CallbackInput() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CallbackInput(CallbackInput other) : this() {
    queryId_ = other.queryId_ != null ? other.queryId_.Clone() : null;
    result_ = other.result_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CallbackInput Clone() {
    return new CallbackInput(this);
  }

  /// <summary>Field number for the "query_id" field.</summary>
  public const int QueryIdFieldNumber = 1;
  private global::AElf.Types.Hash queryId_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public global::AElf.Types.Hash QueryId {
    get { return queryId_; }
    set {
      queryId_ = value;
    }
  }

  /// <summary>Field number for the "result" field.</summary>
  public const int ResultFieldNumber = 2;
  private pb::ByteString result_ = pb::ByteString.Empty;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pb::ByteString Result {
    get { return result_; }
    set {
      result_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as CallbackInput);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(CallbackInput other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (!object.Equals(QueryId, other.QueryId)) return false;
    if (Result != other.Result) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (queryId_ != null) hash ^= QueryId.GetHashCode();
    if (Result.Length != 0) hash ^= Result.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    output.WriteRawMessage(this);
  #else
    if (queryId_ != null) {
      output.WriteRawTag(10);
      output.WriteMessage(QueryId);
    }
    if (Result.Length != 0) {
      output.WriteRawTag(18);
      output.WriteBytes(Result);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
    if (queryId_ != null) {
      output.WriteRawTag(10);
      output.WriteMessage(QueryId);
    }
    if (Result.Length != 0) {
      output.WriteRawTag(18);
      output.WriteBytes(Result);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(ref output);
    }
  }
  #endif

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (queryId_ != null) {
      size += 1 + pb::CodedOutputStream.ComputeMessageSize(QueryId);
    }
    if (Result.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeBytesSize(Result);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(CallbackInput other) {
    if (other == null) {
      return;
    }
    if (other.queryId_ != null) {
      if (queryId_ == null) {
        QueryId = new global::AElf.Types.Hash();
      }
      QueryId.MergeFrom(other.QueryId);
    }
    if (other.Result.Length != 0) {
      Result = other.Result;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    input.ReadRawMessage(this);
  #else
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          if (queryId_ == null) {
            QueryId = new global::AElf.Types.Hash();
          }
          input.ReadMessage(QueryId);
          break;
        }
        case 18: {
          Result = input.ReadBytes();
          break;
        }
      }
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
          break;
        case 10: {
          if (queryId_ == null) {
            QueryId = new global::AElf.Types.Hash();
          }
          input.ReadMessage(QueryId);
          break;
        }
        case 18: {
          Result = input.ReadBytes();
          break;
        }
      }
    }
  }
  #endif

}

#endregion


#endregion Designer generated code
