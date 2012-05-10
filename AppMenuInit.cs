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
			if (System.Environment.GetEnvironmentVariable ("UBUNTU_MENUPROXY") != "libappmenu.so")
				return;

			MonoDevelop.Ide.IdeApp.Initialized +=
				delegate
			{
				FixUp ();

				System.Action deferredFixUp =
				delegate
				{
					FixUp ();
					Gtk.Timeout.Add (100, delegate
					{
						FixUp ();
						return false;
					}
					);


				};
				var workbench = MonoDevelop.Ide.IdeApp.Workbench;
				workbench.ActiveDocumentChanged += (_, __) => deferredFixUp ();
				workbench.LayoutChanged += (_, __) => deferredFixUp ();
				workbench.DocumentOpened += (_, __) => deferredFixUp ();
				var workspace = MonoDevelop.Ide.IdeApp.Workspace;
				workspace.SolutionLoaded += (_, __) => deferredFixUp ();
				workspace.FirstWorkspaceItemOpened += (_, __) => deferredFixUp ();
				workspace.WorkspaceItemLoaded += (_, __) => deferredFixUp ();

			};
		}

		void FixUp ()
		{
			var win = (MonoDevelop.Ide.Gui.WorkbenchWindow)MonoDevelop.Ide.IdeApp.Workbench.RootWindow;
			var mbar = (Gtk.MenuBar)win.GetPropertyValue ("TopMenu");

			WalkMenu (mbar);
			
			mbar.Visible = false;
			mbar.Visible = true;
		}

		static System.Reflection.MethodInfo ensurePopulated = null;

		System.Collections.Generic.IEnumerable<T> FindAllChildren<T> (Gtk.Container cont)
		{
			foreach (var ch in cont.AllChildren.OfType<T>())
				yield return ch;
			foreach (var c in cont.AllChildren.OfType<Gtk.Container>())
				foreach (var xch in FindAllChildren<T>(c))
					yield return xch;

		}

		void WalkMenu (Gtk.MenuShell mnu)
		{
			foreach (var item in mnu.AllChildren.OfType<Gtk.MenuItem>())
			{
				try
				{
					if (ensurePopulated == null)
						ensurePopulated = item.Submenu.GetMethodInfo ("EnsurePopulated");
					ensurePopulated.Invoke (item.Submenu, new object[0]);

				} catch (Exception)
				{

				}
				if (item is CommandMenuItem)
				{
					if ((item.TooltipMarkup == null) && (item.Child is Gtk.HBox))
					{

						item.TooltipMarkup = " ";

						var lbls = FindAllChildren<Gtk.Label> (item).ToList ();
						if (lbls.Count > 0)
						{
							lbls [0].AddNotification ("label", (s, e) =>
							{
								var lbl = (Gtk.Label)s;
								string txt = lbl.LabelProp;

								txt = txt.Replace ("_", "");
								string pairtxt = null;
								if (lbl.Data.ContainsKey ("apair"))
								{
									pairtxt = ((Gtk.Label)lbl.Data ["apair"]).Text.Trim ();
									if (pairtxt.Length != 0)
										pairtxt = "    (" + pairtxt + ")";
									else
										pairtxt = null;
								}
								if ((pairtxt != null) && (!txt.Contains (pairtxt)))
									txt = txt + pairtxt;
								if (lbl.LabelProp != txt)
									lbl.LabelProp = txt;
							}
							);

							if (lbls.Count > 1)
								lbls [0].Data ["apair"] = lbls [1];

							lbls [0].Text = lbls [0].Text;
						}

					}
				}


				if (item.Submenu != null)
					WalkMenu ((Gtk.MenuShell)item.Submenu);
			}
		}

	}
}









