#if MAC

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using MonoMac.AppKit;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvMacTrayIcon : MpAvTrayIconBase {
        private NSStatusItem _item;
        private NSStatusItem statusBarItem {
            get => _item; set {
                _item = value;
                UpdateMenu();
            }
        }

        public override ContextMenu ContextMenu {
            get {
                return null;
            }
        }

        private void UpdateMenu() {
            if (_item != null) {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                _item.Image = NSImage.FromStream(assets.Open(new Uri(IconPath)));
                _item.ToolTip = this.ToolTipText;
                if (statusBarItem.Menu == null)
                    statusBarItem.Menu = new NSMenu();
                else {
                    statusBarItem.Menu.RemoveAllItems();
                }
                foreach (var x in ContextMenu.Items.Cast<MenuItem>()) {
                    NSMenuItem menuItem = new NSMenuItem(x.Header.ToString());
                    menuItem.Activated += (s, e) => { x.Command.Execute(null); };
                    statusBarItem.Menu.AddItem(menuItem);
                }
                //statusBarItem.DoubleClick += (s, e) => { DoubleClick?.Invoke(this, new EventArgs()); };
            }
        }

        public override void Remove() {
            this.statusBarItem.Dispose();
        }

        public MpAvMacTrayIcon() {
            Dispatcher.UIThread.Post(() => {
                var systemStatusBar = NSStatusBar.SystemStatusBar;
                statusBarItem = systemStatusBar.CreateStatusItem(30);
                statusBarItem.ToolTip = this.ToolTipText;
            }, DispatcherPriority.MaxValue);
        }
    }
}

#endif
