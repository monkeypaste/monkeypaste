﻿// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------
// Generated by CefGen
// Source: include/capi/cef_task_capi.h
// --------------------------------------------------------------------------------------------﻿
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
// --------------------------------------------------------------------------------------------

#pragma warning disable 0169, 1591, 1573

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using CefNet.WinApi;

namespace CefNet.CApi
{
	/// <summary>
	/// Structure that asynchronously executes tasks on the associated thread. It is
	/// safe to call the functions of this structure on any thread.
	/// CEF maintains multiple internal threads that are used for handling different
	/// types of tasks in different processes. The cef_thread_id_t definitions in
	/// cef_types.h list the common CEF threads. Task runners are also available for
	/// other CEF threads as appropriate (for example, V8 WebWorker threads).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe partial struct cef_task_runner_t
	{
		/// <summary>
		/// Base structure.
		/// </summary>
		public cef_base_ref_counted_t @base;

		/// <summary>
		/// int (*)(_cef_task_runner_t* self, _cef_task_runner_t* that)*
		/// </summary>
		public void* is_same;

		/// <summary>
		/// Returns true (1) if this object is pointing to the same task runner as
		/// |that| object.
		/// </summary>
		[NativeName("is_same")]
		public unsafe int IsSame(cef_task_runner_t* that)
		{
			fixed (cef_task_runner_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_task_runner_t*, cef_task_runner_t*, int>)is_same)(self, that);
			}
		}

		/// <summary>
		/// int (*)(_cef_task_runner_t* self)*
		/// </summary>
		public void* belongs_to_current_thread;

		/// <summary>
		/// Returns true (1) if this task runner belongs to the current thread.
		/// </summary>
		[NativeName("belongs_to_current_thread")]
		public unsafe int BelongsToCurrentThread()
		{
			fixed (cef_task_runner_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_task_runner_t*, int>)belongs_to_current_thread)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_task_runner_t* self, cef_thread_id_t threadId)*
		/// </summary>
		public void* belongs_to_thread;

		/// <summary>
		/// Returns true (1) if this task runner is for the specified CEF thread.
		/// </summary>
		[NativeName("belongs_to_thread")]
		public unsafe int BelongsToThread(CefThreadId threadId)
		{
			fixed (cef_task_runner_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_task_runner_t*, CefThreadId, int>)belongs_to_thread)(self, threadId);
			}
		}

		/// <summary>
		/// int (*)(_cef_task_runner_t* self, _cef_task_t* task)*
		/// </summary>
		public void* post_task;

		/// <summary>
		/// Post a task for execution on the thread associated with this task runner.
		/// Execution will occur asynchronously.
		/// </summary>
		[NativeName("post_task")]
		public unsafe int PostTask(cef_task_t* task)
		{
			fixed (cef_task_runner_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_task_runner_t*, cef_task_t*, int>)post_task)(self, task);
			}
		}

		/// <summary>
		/// int (*)(_cef_task_runner_t* self, _cef_task_t* task, int64 delay_ms)*
		/// </summary>
		public void* post_delayed_task;

		/// <summary>
		/// Post a task for delayed execution on the thread associated with this task
		/// runner. Execution will occur asynchronously. Delayed tasks are not
		/// supported on V8 WebWorker threads and will be executed without the
		/// specified delay.
		/// </summary>
		[NativeName("post_delayed_task")]
		public unsafe int PostDelayedTask(cef_task_t* task, long delay_ms)
		{
			fixed (cef_task_runner_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_task_runner_t*, cef_task_t*, long, int>)post_delayed_task)(self, task, delay_ms);
			}
		}
	}
}
