﻿// --------------------------------------------------------------------------------------------
// Copyright (c) 2019 The CefNet Authors. All rights reserved.
// Licensed under the MIT license.
// See the licence file in the project root for full license information.
// --------------------------------------------------------------------------------------------
// Generated by CefGen
// Source: include/capi/cef_v8_capi.h
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
	/// Structure representing a V8 value handle. V8 handles can only be accessed
	/// from the thread on which they are created. Valid threads for creating a V8
	/// handle include the render process main thread (TID_RENDERER) and WebWorker
	/// threads. A task runner for posting tasks on the associated thread can be
	/// retrieved via the cef_v8context_t::get_task_runner() function.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe partial struct cef_v8value_t
	{
		/// <summary>
		/// Base structure.
		/// </summary>
		public cef_base_ref_counted_t @base;

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_valid;

		/// <summary>
		/// Returns true (1) if the underlying handle is valid and it can be accessed
		/// on the current thread. Do not call any other functions if this function
		/// returns false (0).
		/// </summary>
		[NativeName("is_valid")]
		public unsafe int IsValid()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_valid)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_undefined;

		/// <summary>
		/// True if the value type is undefined.
		/// </summary>
		[NativeName("is_undefined")]
		public unsafe int IsUndefined()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_undefined)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_null;

		/// <summary>
		/// True if the value type is null.
		/// </summary>
		[NativeName("is_null")]
		public unsafe int IsNull()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_null)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_bool;

		/// <summary>
		/// True if the value type is bool.
		/// </summary>
		[NativeName("is_bool")]
		public unsafe int IsBool()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_bool)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_int;

		/// <summary>
		/// True if the value type is int.
		/// </summary>
		[NativeName("is_int")]
		public unsafe int IsInt()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_int)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_uint;

		/// <summary>
		/// True if the value type is unsigned int.
		/// </summary>
		[NativeName("is_uint")]
		public unsafe int IsUInt()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_uint)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_double;

		/// <summary>
		/// True if the value type is double.
		/// </summary>
		[NativeName("is_double")]
		public unsafe int IsDouble()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_double)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_date;

		/// <summary>
		/// True if the value type is Date.
		/// </summary>
		[NativeName("is_date")]
		public unsafe int IsDate()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_date)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_string;

		/// <summary>
		/// True if the value type is string.
		/// </summary>
		[NativeName("is_string")]
		public unsafe int IsString()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_string)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_object;

		/// <summary>
		/// True if the value type is object.
		/// </summary>
		[NativeName("is_object")]
		public unsafe int IsObject()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_object)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_array;

		/// <summary>
		/// True if the value type is array.
		/// </summary>
		[NativeName("is_array")]
		public unsafe int IsArray()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_array)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_array_buffer;

		/// <summary>
		/// True if the value type is an ArrayBuffer.
		/// </summary>
		[NativeName("is_array_buffer")]
		public unsafe int IsArrayBuffer()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_array_buffer)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_function;

		/// <summary>
		/// True if the value type is function.
		/// </summary>
		[NativeName("is_function")]
		public unsafe int IsFunction()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_function)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, _cef_v8value_t* that)*
		/// </summary>
		public void* is_same;

		/// <summary>
		/// Returns true (1) if this object is pointing to the same handle as |that|
		/// object.
		/// </summary>
		[NativeName("is_same")]
		public unsafe int IsSame(cef_v8value_t* that)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_v8value_t*, int>)is_same)(self, that);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_bool_value;

		/// <summary>
		/// Return a bool value.
		/// </summary>
		[NativeName("get_bool_value")]
		public unsafe int GetBoolValue()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)get_bool_value)(self);
			}
		}

		/// <summary>
		/// int32 (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_int_value;

		/// <summary>
		/// Return an int value.
		/// </summary>
		[NativeName("get_int_value")]
		public unsafe int GetIntValue()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)get_int_value)(self);
			}
		}

		/// <summary>
		/// uint32 (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_uint_value;

		/// <summary>
		/// Return an unsigned int value.
		/// </summary>
		[NativeName("get_uint_value")]
		public unsafe uint GetUIntValue()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, uint>)get_uint_value)(self);
			}
		}

		/// <summary>
		/// double (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_double_value;

		/// <summary>
		/// Return a double value.
		/// </summary>
		[NativeName("get_double_value")]
		public unsafe double GetDoubleValue()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, double>)get_double_value)(self);
			}
		}

		/// <summary>
		/// cef_basetime_t (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_date_value;

		/// <summary>
		/// Return a Date value.
		/// </summary>
		[NativeName("get_date_value")]
		public unsafe cef_basetime_t GetDateValue()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_basetime_t>)get_date_value)(self);
			}
		}

		/// <summary>
		/// cef_string_userfree_t (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_string_value;

		/// <summary>
		/// Return a string value.
		/// The resulting string must be freed by calling cef_string_userfree_free().
		/// </summary>
		[NativeName("get_string_value")]
		public unsafe cef_string_userfree_t GetStringValue()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_userfree_t>)get_string_value)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* is_user_created;

		/// <summary>
		/// Returns true (1) if this is a user created object.
		/// </summary>
		[NativeName("is_user_created")]
		public unsafe int IsUserCreated()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)is_user_created)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* has_exception;

		/// <summary>
		/// Returns true (1) if the last function call resulted in an exception. This
		/// attribute exists only in the scope of the current CEF value object.
		/// </summary>
		[NativeName("has_exception")]
		public unsafe int HasException()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)has_exception)(self);
			}
		}

		/// <summary>
		/// _cef_v8exception_t* (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_exception;

		/// <summary>
		/// Returns the exception resulting from the last function call. This
		/// attribute exists only in the scope of the current CEF value object.
		/// </summary>
		[NativeName("get_exception")]
		public unsafe cef_v8exception_t* GetException()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_v8exception_t*>)get_exception)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* clear_exception;

		/// <summary>
		/// Clears the last exception and returns true (1) on success.
		/// </summary>
		[NativeName("clear_exception")]
		public unsafe int ClearException()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)clear_exception)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* will_rethrow_exceptions;

		/// <summary>
		/// Returns true (1) if this object will re-throw future exceptions. This
		/// attribute exists only in the scope of the current CEF value object.
		/// </summary>
		[NativeName("will_rethrow_exceptions")]
		public unsafe int WillRethrowExceptions()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)will_rethrow_exceptions)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, int rethrow)*
		/// </summary>
		public void* set_rethrow_exceptions;

		/// <summary>
		/// Set whether this object will re-throw future exceptions. By default
		/// exceptions are not re-thrown. If a exception is re-thrown the current
		/// context should not be accessed again until after the exception has been
		/// caught and not re-thrown. Returns true (1) on success. This attribute
		/// exists only in the scope of the current CEF value object.
		/// </summary>
		[NativeName("set_rethrow_exceptions")]
		public unsafe int SetRethrowExceptions(int rethrow)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int, int>)set_rethrow_exceptions)(self, rethrow);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, const cef_string_t* key)*
		/// </summary>
		public void* has_value_bykey;

		/// <summary>
		/// Returns true (1) if the object has a value with the specified identifier.
		/// </summary>
		[NativeName("has_value_bykey")]
		public unsafe int HasValueByKey([Immutable]cef_string_t* key)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_t*, int>)has_value_bykey)(self, key);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, int index)*
		/// </summary>
		public void* has_value_byindex;

		/// <summary>
		/// Returns true (1) if the object has a value with the specified identifier.
		/// </summary>
		[NativeName("has_value_byindex")]
		public unsafe int HasValueByIndex(int index)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int, int>)has_value_byindex)(self, index);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, const cef_string_t* key)*
		/// </summary>
		public void* delete_value_bykey;

		/// <summary>
		/// Deletes the value with the specified identifier and returns true (1) on
		/// success. Returns false (0) if this function is called incorrectly or an
		/// exception is thrown. For read-only and don&apos;t-delete values this function
		/// will return true (1) even though deletion failed.
		/// </summary>
		[NativeName("delete_value_bykey")]
		public unsafe int DeleteValueByKey([Immutable]cef_string_t* key)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_t*, int>)delete_value_bykey)(self, key);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, int index)*
		/// </summary>
		public void* delete_value_byindex;

		/// <summary>
		/// Deletes the value with the specified identifier and returns true (1) on
		/// success. Returns false (0) if this function is called incorrectly,
		/// deletion fails or an exception is thrown. For read-only and don&apos;t-delete
		/// values this function will return true (1) even though deletion failed.
		/// </summary>
		[NativeName("delete_value_byindex")]
		public unsafe int DeleteValueByIndex(int index)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int, int>)delete_value_byindex)(self, index);
			}
		}

		/// <summary>
		/// _cef_v8value_t* (*)(_cef_v8value_t* self, const cef_string_t* key)*
		/// </summary>
		public void* get_value_bykey;

		/// <summary>
		/// Returns the value with the specified identifier on success. Returns NULL
		/// if this function is called incorrectly or an exception is thrown.
		/// </summary>
		[NativeName("get_value_bykey")]
		public unsafe cef_v8value_t* GetValueByKey([Immutable]cef_string_t* key)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_t*, cef_v8value_t*>)get_value_bykey)(self, key);
			}
		}

		/// <summary>
		/// _cef_v8value_t* (*)(_cef_v8value_t* self, int index)*
		/// </summary>
		public void* get_value_byindex;

		/// <summary>
		/// Returns the value with the specified identifier on success. Returns NULL
		/// if this function is called incorrectly or an exception is thrown.
		/// </summary>
		[NativeName("get_value_byindex")]
		public unsafe cef_v8value_t* GetValueByIndex(int index)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int, cef_v8value_t*>)get_value_byindex)(self, index);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, const cef_string_t* key, _cef_v8value_t* value, cef_v8_propertyattribute_t attribute)*
		/// </summary>
		public void* set_value_bykey;

		/// <summary>
		/// Associates a value with the specified identifier and returns true (1) on
		/// success. Returns false (0) if this function is called incorrectly or an
		/// exception is thrown. For read-only values this function will return true
		/// (1) even though assignment failed.
		/// </summary>
		[NativeName("set_value_bykey")]
		public unsafe int SetValueByKey([Immutable]cef_string_t* key, cef_v8value_t* value, CefV8PropertyAttribute attribute)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_t*, cef_v8value_t*, CefV8PropertyAttribute, int>)set_value_bykey)(self, key, value, attribute);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, int index, _cef_v8value_t* value)*
		/// </summary>
		public void* set_value_byindex;

		/// <summary>
		/// Associates a value with the specified identifier and returns true (1) on
		/// success. Returns false (0) if this function is called incorrectly or an
		/// exception is thrown. For read-only values this function will return true
		/// (1) even though assignment failed.
		/// </summary>
		[NativeName("set_value_byindex")]
		public unsafe int SetValueByIndex(int index, cef_v8value_t* value)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int, cef_v8value_t*, int>)set_value_byindex)(self, index, value);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, const cef_string_t* key, cef_v8_accesscontrol_t settings, cef_v8_propertyattribute_t attribute)*
		/// </summary>
		public void* set_value_byaccessor;

		/// <summary>
		/// Registers an identifier and returns true (1) on success. Access to the
		/// identifier will be forwarded to the cef_v8accessor_t instance passed to
		/// cef_v8value_t::cef_v8value_create_object(). Returns false (0) if this
		/// function is called incorrectly or an exception is thrown. For read-only
		/// values this function will return true (1) even though assignment failed.
		/// </summary>
		[NativeName("set_value_byaccessor")]
		public unsafe int SetValueByAccessor([Immutable]cef_string_t* key, CefV8AccessControl settings, CefV8PropertyAttribute attribute)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_t*, CefV8AccessControl, CefV8PropertyAttribute, int>)set_value_byaccessor)(self, key, settings, attribute);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, cef_string_list_t keys)*
		/// </summary>
		public void* get_keys;

		/// <summary>
		/// Read the keys for the object&apos;s values into the specified vector. Integer-
		/// based keys will also be returned as strings.
		/// </summary>
		[NativeName("get_keys")]
		public unsafe int GetKeys(cef_string_list_t keys)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_list_t, int>)get_keys)(self, keys);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, _cef_base_ref_counted_t* user_data)*
		/// </summary>
		public void* set_user_data;

		/// <summary>
		/// Sets the user data for this object and returns true (1) on success.
		/// Returns false (0) if this function is called incorrectly. This function
		/// can only be called on user created objects.
		/// </summary>
		[NativeName("set_user_data")]
		public unsafe int SetUserData(cef_base_ref_counted_t* user_data)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_base_ref_counted_t*, int>)set_user_data)(self, user_data);
			}
		}

		/// <summary>
		/// _cef_base_ref_counted_t* (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_user_data;

		/// <summary>
		/// Returns the user data, if any, assigned to this object.
		/// </summary>
		[NativeName("get_user_data")]
		public unsafe cef_base_ref_counted_t* GetUserData()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_base_ref_counted_t*>)get_user_data)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_externally_allocated_memory;

		/// <summary>
		/// Returns the amount of externally allocated memory registered for the
		/// object.
		/// </summary>
		[NativeName("get_externally_allocated_memory")]
		public unsafe int GetExternallyAllocatedMemory()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)get_externally_allocated_memory)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self, int change_in_bytes)*
		/// </summary>
		public void* adjust_externally_allocated_memory;

		/// <summary>
		/// Adjusts the amount of registered external memory for the object. Used to
		/// give V8 an indication of the amount of externally allocated memory that is
		/// kept alive by JavaScript objects. V8 uses this information to decide when
		/// to perform global garbage collection. Each cef_v8value_t tracks the amount
		/// of external memory associated with it and automatically decreases the
		/// global total by the appropriate amount on its destruction.
		/// |change_in_bytes| specifies the number of bytes to adjust by. This
		/// function returns the number of bytes associated with the object after the
		/// adjustment. This function can only be called on user created objects.
		/// </summary>
		[NativeName("adjust_externally_allocated_memory")]
		public unsafe int AdjustExternallyAllocatedMemory(int change_in_bytes)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int, int>)adjust_externally_allocated_memory)(self, change_in_bytes);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_array_length;

		/// <summary>
		/// Returns the number of elements in the array.
		/// </summary>
		[NativeName("get_array_length")]
		public unsafe int GetArrayLength()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)get_array_length)(self);
			}
		}

		/// <summary>
		/// _cef_v8array_buffer_release_callback_t* (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_array_buffer_release_callback;

		/// <summary>
		/// Returns the ReleaseCallback object associated with the ArrayBuffer or NULL
		/// if the ArrayBuffer was not created with CreateArrayBuffer.
		/// </summary>
		[NativeName("get_array_buffer_release_callback")]
		public unsafe cef_v8array_buffer_release_callback_t* GetArrayBufferReleaseCallback()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_v8array_buffer_release_callback_t*>)get_array_buffer_release_callback)(self);
			}
		}

		/// <summary>
		/// int (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* neuter_array_buffer;

		/// <summary>
		/// Prevent the ArrayBuffer from using it&apos;s memory block by setting the length
		/// to zero. This operation cannot be undone. If the ArrayBuffer was created
		/// with CreateArrayBuffer then
		/// cef_v8array_buffer_release_callback_t::ReleaseBuffer will be called to
		/// release the underlying buffer.
		/// </summary>
		[NativeName("neuter_array_buffer")]
		public unsafe int NeuterArrayBuffer()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, int>)neuter_array_buffer)(self);
			}
		}

		/// <summary>
		/// cef_string_userfree_t (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_function_name;

		/// <summary>
		/// Returns the function name.
		/// The resulting string must be freed by calling cef_string_userfree_free().
		/// </summary>
		[NativeName("get_function_name")]
		public unsafe cef_string_userfree_t GetFunctionName()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_string_userfree_t>)get_function_name)(self);
			}
		}

		/// <summary>
		/// _cef_v8handler_t* (*)(_cef_v8value_t* self)*
		/// </summary>
		public void* get_function_handler;

		/// <summary>
		/// Returns the function handler or NULL if not a CEF-created function.
		/// </summary>
		[NativeName("get_function_handler")]
		public unsafe cef_v8handler_t* GetFunctionHandler()
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_v8handler_t*>)get_function_handler)(self);
			}
		}

		/// <summary>
		/// _cef_v8value_t* (*)(_cef_v8value_t* self, _cef_v8value_t* object, size_t argumentsCount, const _cef_v8value_t** arguments)*
		/// </summary>
		public void* execute_function;

		/// <summary>
		/// Execute the function using the current V8 context. This function should
		/// only be called from within the scope of a cef_v8handler_t or
		/// cef_v8accessor_t callback, or in combination with calling enter() and
		/// exit() on a stored cef_v8context_t reference. |object| is the receiver
		/// (&apos;this&apos; object) of the function. If |object| is NULL the current context&apos;s
		/// global object will be used. |arguments| is the list of arguments that will
		/// be passed to the function. Returns the function return value on success.
		/// Returns NULL if this function is called incorrectly or an exception is
		/// thrown.
		/// </summary>
		[NativeName("execute_function")]
		public unsafe cef_v8value_t* ExecuteFunction(cef_v8value_t* @object, UIntPtr argumentsCount, [Immutable]cef_v8value_t** arguments)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_v8value_t*, UIntPtr, cef_v8value_t**, cef_v8value_t*>)execute_function)(self, @object, argumentsCount, arguments);
			}
		}

		/// <summary>
		/// _cef_v8value_t* (*)(_cef_v8value_t* self, _cef_v8context_t* context, _cef_v8value_t* object, size_t argumentsCount, const _cef_v8value_t** arguments)*
		/// </summary>
		public void* execute_function_with_context;

		/// <summary>
		/// Execute the function using the specified V8 context. |object| is the
		/// receiver (&apos;this&apos; object) of the function. If |object| is NULL the
		/// specified context&apos;s global object will be used. |arguments| is the list of
		/// arguments that will be passed to the function. Returns the function return
		/// value on success. Returns NULL if this function is called incorrectly or
		/// an exception is thrown.
		/// </summary>
		[NativeName("execute_function_with_context")]
		public unsafe cef_v8value_t* ExecuteFunctionWithContext(cef_v8context_t* context, cef_v8value_t* @object, UIntPtr argumentsCount, [Immutable]cef_v8value_t** arguments)
		{
			fixed (cef_v8value_t* self = &this)
			{
				return ((delegate* unmanaged[Stdcall]<cef_v8value_t*, cef_v8context_t*, cef_v8value_t*, UIntPtr, cef_v8value_t**, cef_v8value_t*>)execute_function_with_context)(self, context, @object, argumentsCount, arguments);
			}
		}
	}
}
