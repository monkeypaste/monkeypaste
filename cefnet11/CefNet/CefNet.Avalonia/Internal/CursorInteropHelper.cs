using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;

using DynamicData;

namespace CefNet.Internal
{
	public static class CursorInteropHelper
	{
		private static readonly Dictionary<CefCursorType, Cursor> _Cursors = new Dictionary<CefCursorType, Cursor>();
		private static readonly Dictionary<StandardCursorType, Cursor> _StdCursors = new Dictionary<StandardCursorType, Cursor>();

		//private static IntPtr GetPlatformHandle(Cursor cursor)
		//{
		//	if (cursor != null)
		//	{
		//		cursor.C
		//		if (cursor.PlatformImpl is IPlatformHandle i)
		//		{
		//			return i.Handle;
		//		}
		//	}
		//	return default;
		//}
		private static CefCursorType GetCefCursorType(Cursor cursor)
		{
			if(cursor != null)
			{
				StandardCursorType av_cursor_type = (StandardCursorType)Enum.GetNames(typeof(StandardCursorType)).IndexOf(cursor.ToString());

				switch(av_cursor_type)
				{
					case StandardCursorType.Arrow:
						return CefCursorType.Pointer;

					case StandardCursorType.Hand:
						return CefCursorType.Hand;

					case StandardCursorType.Ibeam:
						return CefCursorType.Ibeam;

					case StandardCursorType.Help:
						return CefCursorType.Help;

					case StandardCursorType.SizeAll:
						return CefCursorType.Move;

					case StandardCursorType.SizeNorthSouth:
						return CefCursorType.Northsouthresize;

					case StandardCursorType.SizeWestEast:
						return CefCursorType.Eastwestresize;

					case StandardCursorType.Wait:
						return CefCursorType.Wait;

					case StandardCursorType.DragMove:
						return CefCursorType.DndMove;

					case StandardCursorType.DragCopy:
						return CefCursorType.DndCopy;

					case StandardCursorType.DragLink:
						return CefCursorType.DndLink;

					case StandardCursorType.No:
						return CefCursorType.Notallowed;

					case StandardCursorType.None:
						return CefCursorType.None;

				}
			}
			return CefCursorType.Pointer;
		}

		static CursorInteropHelper()
		{
			foreach(StandardCursorType cursorType in Enum.GetValues(typeof(StandardCursorType)))
			{
				var cursor = new Cursor(cursorType);
				var cursor_name = GetCefCursorType(cursor);
				if(cursor_name == default || _Cursors.ContainsKey(cursor_name))
					continue;

				_Cursors.Add(cursor_name, cursor);
				_StdCursors.Add(cursorType, cursor);
			}
		}

		//public static Cursor Create(IntPtr cursorHandle)
		//{
		//	if(_Cursors.TryGetValue(cursorHandle, out Cursor cursor))
		//		return cursor;
		//	return Cursor.Default;
		//}

		public static Cursor Create(CefCursorType ct)
		{
			if(_Cursors.TryGetValue(ct, out Cursor cursor))
				return cursor;
			return Cursor.Default;
		}

		public static Cursor Create(StandardCursorType cursorType)
		{
			Cursor cursor;
			lock(_StdCursors)
			{
				if(!_StdCursors.TryGetValue(cursorType, out cursor))
				{
					cursor = new Cursor(cursorType);
					_StdCursors[cursorType] = cursor;
				}
			}
			return cursor;
		}
	}
}