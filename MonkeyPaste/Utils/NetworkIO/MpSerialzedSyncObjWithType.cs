using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
	public class MpSerialzedSyncObjWithType {
		public Type ObjType { get; set; }
		public string ObjStr { get; set; } = string.Empty;

		public static string ConvertTo(MpISyncableDbObject sdbo) {
			string objTypeStr = sdbo.GetDbObjectType().ToString();
			var objJson = sdbo.SerializeDbObject();
			return string.Format(@"{0},{1}", objTypeStr, objJson);
		}

		public static MpSerialzedSyncObjWithType ConvertFrom(string serializedSyncObjStr,MpIStringToSyncObjectTypeConverter converter) {
			if(string.IsNullOrEmpty(serializedSyncObjStr)) {
				return null;
            }
			if(!serializedSyncObjStr.Contains(",")) {
				throw new Exception(@"JsonDbObjectStr must contain a comma to infer type");
            }
			var serializedSyncObject = new MpSerialzedSyncObjWithType();
			//seperate by first comma then use reflection on [0] to create type
			//then [1] gets obj json
			int splitIdx = serializedSyncObjStr.IndexOf(',');
			string typeStr = serializedSyncObjStr.Substring(0, splitIdx);
			serializedSyncObject.ObjType = converter.Convert(typeStr);
			serializedSyncObject.ObjStr = serializedSyncObjStr.Substring(splitIdx + 1);
			
			return serializedSyncObject;
		}
	}
}
