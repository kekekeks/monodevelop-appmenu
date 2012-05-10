// 
// AppMenuInit.cs
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
using System.Linq;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.AppMenu
{
	public class AppMenuInit:CommandHandler
	{
		protected override void Run ()
		{
			//if(System.Environment.GetEnvironmentVariable("UBUNTU_MENUPROXY")!="libappmenu.so")
			//	return;


			MonoDevelop.Ide.IdeApp.Initialized+=
				delegate
			{
				FixUp();

				MonoDevelop.Ide.IdeApp.Workbench.ActiveDocumentChanged+= 
				delegate {
					LiteFixUp();
				};
				var vbox=(Gtk.VBox) MonoDevelop.Ide.IdeApp.Workbench.RootWindow.Child;
				Gtk.Timeout.Add(2000, delegate
				{
					LiteFixUp();
					return true;
				});
			};
		}

		void LiteFixUp ()
		{
			var win=(MonoDevelop.Ide.Gui.WorkbenchWindow) MonoDevelop.Ide.IdeApp.Workbench.RootWindow;
			var mbar=(Gtk.MenuBar)win.GetPropertyValue("TopMenu");

			WalkMenu(mbar);
			
					mbar.Visible=false;
					mbar.Visible=true;
		}

		void FixUp()
		{
			foreach(var cmd in Ide.IdeApp.CommandService.GetCommands())
			{
				cmd.Text=cmd.Text.Replace("_", "");

				if(cmd.AccelKey!=null)
				{
					var xtxt="  ("+cmd.AccelKey.Replace("Control", "Ctrl")+")";
					if(!cmd.Text.Contains(xtxt))
						cmd.Text+=xtxt;

				}
			}

			System.Threading.ThreadPool.QueueUserWorkItem(delegate
			{
				Gtk.Application.Invoke(delegate
				{
					LiteFixUp();


				});

			});
		}


		static System.Reflection.FieldInfo overrideLabel=null;
		static System.Reflection.FieldInfo lastCmdInfo = null;
		void WalkMenu (Gtk.MenuShell mnu)
		{
			foreach(var item in mnu.AllChildren.OfType<Gtk.MenuItem>())
			{
				try
				{
					item.Submenu.CallMethod("EnsurePopulated", new object[0]);

				}
				catch(Exception)
				{

				}
				if(item is CommandMenuItem)
				{

					if(overrideLabel==null)
						overrideLabel=item.GetType().GetFieldAccessor("overrideLabel");
					if(lastCmdInfo==null)
						lastCmdInfo=item.GetType().GetFieldAccessor("lastCmdInfo");
					var value=(string)overrideLabel.GetValue(item);
					if(value==null)
						item.TooltipMarkup=" ";

					if(item.TooltipMarkup!=null)
					{
						value=((CommandInfo)lastCmdInfo.GetValue(item)).Text;
					}
					overrideLabel.SetValue(item, value.Replace("_",""));



				}
				item.AllChildren.OfType<Gtk.Label>().Each(l => l.LabelProp=l.LabelProp.Replace("_", ""));
				if(item.Submenu!=null)
					WalkMenu((Gtk.MenuShell)item.Submenu);
			}
		}
	}
}

