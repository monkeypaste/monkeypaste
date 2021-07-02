using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
	public class MpDbMessage {
		public List<MpJsonDbObject> JsonDbObjects { get; set; } = new List<MpJsonDbObject>();

		public static async Task<MpDbMessage> Parse(string message, MpIDbStringToDbObjectTypeConverter typeConverter, string parseToken = "#$%^") {
			if(string.IsNullOrEmpty(message)) {
				return null;
            }
			var dbMessage = new MpDbMessage();
			//split msg by parseToken then sub elements pass to jsonDbObject
			//and and to to list
			var dbObjects = message.Split(new string[] { parseToken }, StringSplitOptions.RemoveEmptyEntries);
			foreach(var dbo in dbObjects) {
				var jdbo = await MpJsonDbObject.Parse(dbo,typeConverter);
				if(jdbo == null) {
					continue;
                }
				dbMessage.JsonDbObjects.Add(jdbo);
            }
			return dbMessage;
		}

		public static string Create(List<MpISyncableDbObject> dbol, string parseToken = "#$%^") {			
			var sb = new StringBuilder();
			foreach(var oi in dbol) {
				var jsonDbObject = MpJsonDbObject.Create(oi);
				sb.Append(string.Format(@"{0}{1}", jsonDbObject, parseToken));
            }
			return sb.ToString();
        }

	}
}
