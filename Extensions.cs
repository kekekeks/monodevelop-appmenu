// 
// Extensions.cs
//  
// Author:
//       Nikita Tsukanov <keks9n@gmail.com>
// 
// Copyright (c) 2012 Nikita Tsukanov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using System;

using BindingFlags=System.Reflection.BindingFlags;
using System.Linq;
namespace MonoDevelop.AppMenu
{
	internal static class Extensions
	{
		public static object GetPropertyValue(this object o, string property)
		{
			return o.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
				.Where(p=>p.Name==property).First().GetValue(o, new object[0]);
		}

		public static object CallMethod (this object o, string method, object[] args)
		{
			return o.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
				.Where(m=>m.Name==method).First().Invoke(o, args);
		}
		public static System.Reflection.FieldInfo GetFieldAccessor (this Type t, string field)
		{
			return t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
				.Where(m=>m.Name==field).First();
		}
		public static void Each <T> (this System.Collections.Generic.IEnumerable<T> col, Action<T> act)
		{
			foreach(var o in col)
				act(o);
		}

	}
}

