using System;
using CefNet.CApi;

namespace CefNet
{
	/// <summary>
	/// Represents an absolute point in coordinated universal time (UTC),
	/// internally represented as microseconds (s/1,000,000) since the Windows epoch
	/// (1601-01-01 00:00:00 UTC).
	/// </summary>
	public struct CefBaseTime
	{
		private readonly long _value;

		public CefBaseTime(cef_basetime_t time)
		{
			_value = time.val;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="time">
		/// A date and time expressed in microseconds (s/1,000,000) since the Windows epoch (1601-01-01 00:00:00 UTC).
		/// </param>
		public CefBaseTime(long time)
		{
			_value = time;
		}

		/// <summary>
		/// The Windows epoch (1601-01-01 00:00:00 UTC).
		/// </summary>
		public static readonly DateTime WindowsEpoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Returns a <see cref="CefBaseTime"/> instance that is set to the date and time of the specified <paramref name="dateTime"/>.
		/// </summary>
		/// <param name="dateTime">The <see cref="DateTime"/> instance.</param>
		/// <returns>The <see cref="CefBaseTime"/> instance.</returns>
		public static CefBaseTime FromDateTime(DateTime dateTime)
		{
			return new CefBaseTime(dateTime.ToFileTimeUtc() / 10);
		}

		/// <summary>
		/// Returns a <see cref="DateTime"/> that is set to the date and time of this <see cref="CefBaseTime"/> instance.
		/// </summary>
		/// <returns>The <see cref="DateTime"/> instance.</returns>
		public DateTime ToDateTime()
		{
			return DateTime.FromFileTimeUtc(_value * 10);
		}

		public static implicit operator CefBaseTime(cef_basetime_t instance)
		{
			return new CefBaseTime(instance);
		}

		public static implicit operator cef_basetime_t(CefBaseTime instance)
		{
			return new cef_basetime_t { val = instance._value };
		}

		public static implicit operator DateTime(CefBaseTime instance)
		{
			try
			{
				return DateTime.FromFileTimeUtc(instance._value * 10);
			}
			catch (ArgumentException)
			{
				return default;
			}
		}

		public static implicit operator CefBaseTime(DateTime dateTime)
		{
			try
			{
				return new CefBaseTime(dateTime.ToFileTimeUtc() / 10);
			}
			catch (ArgumentException)
			{
				return new CefBaseTime(0);
			}
		}
	}
}
