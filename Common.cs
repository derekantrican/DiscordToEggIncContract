// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: common.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Aux {

  /// <summary>Holder for reflection information generated from common.proto</summary>
  public static partial class CommonReflection {

    #region Descriptor
    /// <summary>File descriptor for common.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static CommonReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cgxjb21tb24ucHJvdG8SA2F1eCoeCghQbGF0Zm9ybRIHCgNJT1MQARIJCgVE",
            "Uk9JRBACKikKEERldmljZUZvcm1GYWN0b3ISCQoFUEhPTkUQARIKCgZUQUJM",
            "RVQQAiprCglBZE5ldHdvcmsSCgoGVlVOR0xFEAASDgoKQ0hBUlRCT09TVBAB",
            "Eg0KCUFEX0NPTE9OWRACEgwKCEhZUEVSX01YEAMSCQoFVU5JVFkQBBIMCghG",
            "QUNFQk9PSxAFEgwKCEFQUExPVklOEAY="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Aux.Platform), typeof(global::Aux.DeviceFormFactor), typeof(global::Aux.AdNetwork), }, null, null));
    }
    #endregion

  }
  #region Enums
  public enum Platform {
    [pbr::OriginalName("IOS")] Ios = 1,
    [pbr::OriginalName("DROID")] Droid = 2,
  }

  public enum DeviceFormFactor {
    [pbr::OriginalName("PHONE")] Phone = 1,
    [pbr::OriginalName("TABLET")] Tablet = 2,
  }

  public enum AdNetwork {
    [pbr::OriginalName("VUNGLE")] Vungle = 0,
    [pbr::OriginalName("CHARTBOOST")] Chartboost = 1,
    [pbr::OriginalName("AD_COLONY")] AdColony = 2,
    [pbr::OriginalName("HYPER_MX")] HyperMx = 3,
    [pbr::OriginalName("UNITY")] Unity = 4,
    [pbr::OriginalName("FACEBOOK")] Facebook = 5,
    [pbr::OriginalName("APPLOVIN")] Applovin = 6,
  }

  #endregion

}

#endregion Designer generated code