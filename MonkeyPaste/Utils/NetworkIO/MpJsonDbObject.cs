using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
	public class MpJsonDbObject {
		public Type DbObjectType { get; set; }
		public string DbObjectJson { get; set; }
		public object DbObject { get; set; }

		public static async Task<MpJsonDbObject> Parse(string jsonDbObjectStr, MpIDbStringToDbObjectTypeConverter typeConverter) {
			if(string.IsNullOrEmpty(jsonDbObjectStr)) {
				return null;
            }
			if(!jsonDbObjectStr.Contains(",")) {
				throw new Exception(@"JsonDbObjectStr must contain a comma to infer type");
            }
			var jsonDbObject = new MpJsonDbObject();
			//seperate by first comma then use reflection on [0] to create type
			//then [1] gets obj json
			int splitIdx = jsonDbObjectStr.IndexOf(',');
			string typeStr = jsonDbObjectStr.Substring(0, splitIdx);
			string jsonStr = jsonDbObjectStr.Substring(splitIdx + 1);

			jsonDbObject.DbObjectType = typeConverter.Convert(typeStr);
			jsonDbObject.DbObjectJson = jsonStr;
			jsonDbObject.DbObject = JsonConvert.DeserializeObject(jsonStr, jsonDbObject.DbObjectType);
			jsonDbObject.DbObject = await (jsonDbObject.DbObject as MpISyncableDbObject).DeserializeDbObject(jsonDbObject.DbObject);
			return jsonDbObject;
		}

		public static string Create(MpISyncableDbObject sdbo) {
			string objTypeStr = sdbo.GetDbObjectType().ToString();
			var objJson = sdbo.SerializeDbObject();
			return string.Format(@"{0},{1}", objTypeStr, objJson);
		}
	}
}
