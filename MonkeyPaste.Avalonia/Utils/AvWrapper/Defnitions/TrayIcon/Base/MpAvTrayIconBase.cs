using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MonkeyPaste;
namespace MonkeyPaste.Avalonia {
    public abstract class MpAvTrayIconBase : MpITrayIcon {
        public MpAvTrayIconBase() {
        }

        public string IconPath => @"/Assets/Icons/monkey.ico";

        public string ToolTipText => "Monkey Paste";

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "Settings"
                        },
                        new MpMenuItemViewModel() {
                            Header = "Test",
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = "Sub Test 1"
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Sub Test 2"
                                }
                            }
                        },
                        new MpMenuItemViewModel() {
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

        public event EventHandler<EventArgs>? Click;
        public event EventHandler<EventArgs>? DoubleClick;
        public event EventHandler<EventArgs>? RightClick;

        public abstract void Remove();
    }
}

