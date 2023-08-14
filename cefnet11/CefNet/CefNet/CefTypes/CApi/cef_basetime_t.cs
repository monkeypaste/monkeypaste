using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet.CApi
{
#pragma warning disable CS1591
	public unsafe partial struct cef_basetime_t : IEquatable<cef_basetime_t>
#pragma warning restore CS1591
	{
		public static cef_basetime_t FromDateTime(DateTime dateTime)
		{
			return new cef_basetime_t { val = dateTime.ToFileTimeUtc() / 10 };
		}

		public DateTime ToDateTime()
		{
			return DateTime.FromFileTime(val * 10);
		}

		public override int GetHashCode()
		{
			return val.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is cef_basetime_t other && other.val == val;
		}

		public bool Equals(cef_basetime_t other)
		{
			return other.val == val;
		}
	}
}
