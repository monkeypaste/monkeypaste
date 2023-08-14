using System;
using System.Runtime.CompilerServices;

namespace CefNet.Internal
{
	public partial class WebViewGlue
	{
		public void CreateOrDestroyCommandGlue()
		{
			if (AvoidOnChromeCommand())
			{
				this.CommandGlue = null;
			}
			else if (this.AudioGlue is null)
			{
				this.CommandGlue = new CefCommandHandlerGlue(this);
			}
		}

		/// <summary>
		/// Returns value indicating that call to OnChromeCommand can be avoided.
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal extern bool AvoidOnChromeCommand();

		/// <summary>
		/// Called to execute a Chrome command triggered via menu selection or keyboard
		/// shortcut. Values for |command_id| can be found in the cef_command_ids.h
		/// file. |disposition| provides information about the intended command target.
		/// Return 
		/// </summary>
		/// <param name="commandId">
		/// The command identifier. Values for <paramref name="commandId"/> can be found
		/// in the cef_command_ids.h file.
		/// </param>
		/// <returns>
		/// True if the command was handled or False for the default implementation. 
		/// </returns>
		/// <remarks>
		/// For context menu commands this will be called after <see cref="OnContextMenuCommand"/>.
		/// Only used with the Chrome runtime.
		/// </remarks>
		internal protected virtual bool OnChromeCommand(CefBrowser browser, int commandId, CefWindowOpenDisposition disposition)
		{
			return false;
		}
	}
}
