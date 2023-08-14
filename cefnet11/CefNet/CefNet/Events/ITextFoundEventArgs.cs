using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet
{
	/// <summary>
	/// Represents find results.
	/// </summary>
	public interface ITextFoundEventArgs
	{
		/// <summary>
		/// Gets a unique incremental identifier for the currently active search.
		/// </summary>
		int ID { get; }

		/// <summary>
		/// Gets the current position in the search results.
		/// </summary>
		int Index { get; }

		/// <summary>
		/// Gets the number of matches currently identified.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Gets the location of where the match was found (in window coordinates).
		/// </summary>
		CefRect SelectionRect { get; }

		/// <summary>
		/// Gets a value which indicates that is the last find notification.
		/// </summary>
		bool FinalUpdate { get; }
	}
}
