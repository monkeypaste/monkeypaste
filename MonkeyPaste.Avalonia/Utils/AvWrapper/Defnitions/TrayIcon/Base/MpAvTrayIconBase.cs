using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Windows.Input;
namespace MonkeyPaste.Avalonia {
    public abstract class MpAvTrayIconBase : MpAvITrayIcon {
        public MpAvTrayIconBase() {
        }

        public string IconPath => @"/Assets/Icons/monkey.ico";

        public string ToolTipText => "Monkey Paste";

        public MpAvMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpAvMenuItemViewModel() {
                    SubItems = new List<MpAvMenuItemViewModel>() {
                        new MpAvMenuItemViewModel() {
                            Header = "Settings"
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test",
                            SubItems = new List<MpAvMenuItemViewModel>() {
                                new MpAvMenuItemViewModel() {
                                    Header = "Sub Test 1"
                                },
                                new MpAvMenuItemViewModel() {
                                    Header = "Sub Test 2"
                                }
                            }
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Exit",
                            Command = ExitCommand
                        }
                    }
                };
            }
        }

        public abstract ContextMenu ContextMenu { get; }



        public ICommand ExitCommand => new MpCommand(
            () => {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                    lifetime.Shutdown();

                }
            });

        public bool Visible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //public event EventHandler<EventArgs>? Click;
        //public event EventHandler<EventArgs>? DoubleClick;
        //public event EventHandler<EventArgs>? RightClick;

        public abstract void Remove();
    }
}

