using CefNet.CApi;

namespace CefNet
{
	public unsafe partial class CefDictionaryValue
	{
		/// <summary>
		/// Creates a new object that is not owned by any other object.
		/// </summary>
		public CefDictionaryValue()
			: this(CefNativeApi.cef_dictionary_value_create())
		{

		}

		/// <summary>
		/// Gets an array containing the keys in this dictionary.
		/// </summary>
		/// <returns></returns>
		public string[] GetKeys()
		{
			using (var list = new CefStringList())
			{
				if (GetKeys(list) == 0)
					return new string[0];
				return list.ToArray();
			}
		}
	}
}
