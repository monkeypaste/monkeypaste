using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet
{
	/// <summary>
	/// Represents find results.
	/// </summary>
	public sealed class TextFoundEventArgs : EventArgs, ITextFoundEventArgs
	{
		public TextFoundEventArgs(int identifier, int count, CefRect selectionRect, int index, bool finalUpdate)
		{
			this.ID = identifier;
			this.Count = count;
			this.SelectionRect = selectionRect;
			this.Index = index;
			this.FinalUpdate = finalUpdate;
		}

		/// <inheritdoc />
		public int ID { get; }

		/// <inheritdoc />
		public int Index { get; }

		/// <inheritdoc />
		public int Count { get; }

		/// <inheritdoc />
		public CefRect SelectionRect { get; }

		/// <inheritdoc />
		public bool FinalUpdate { get; }

	}
}
