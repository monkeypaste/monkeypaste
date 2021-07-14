using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
	public class MpDbMessage {
		public const string ParseToken = "#$%^";
		public List<MpSerialzedSyncObjWithType> DbObjects { get; set; } = new List<MpSerialzedSyncObjWithType>();

		public static MpDbMessage Parse(string message, MpIStringToSyncObjectTypeConverter converter) {
			if(string.IsNullOrEmpty(message)) {
				return null;
            }
			var dbMessage = new MpDbMessage();
			//split msg by parseToken then sub elements pass to jsonDbObject
			//and and to to list
			var dbObjects = message.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
			foreach(var dbo in dbObjects) {
				var jdbo = MpSerialzedSyncObjWithType.ConvertFrom(dbo,converter);
				if(jdbo == null) {
					continue;
                }
				dbMessage.DbObjects.Add(jdbo);
            }
			return dbMessage;
		}

		public static string Create(List<MpISyncableDbObject> dbol) {			
			var sb = new StringBuilder();
			foreach(var oi in dbol) {
				var jsonDbObject = MpSerialzedSyncObjWithType.ConvertTo(oi);
				sb.Append(string.Format(@"{0}{1}", jsonDbObject, ParseToken));
            }
			return sb.ToString();
        }

	}
}
