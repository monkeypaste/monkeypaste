﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public class MpAvActiveApplicationChangedTrigger : 
        MpAvTriggerActionViewModelBase {
        #region Constants

        public const string EXCLUDED_APPS_PARAM_ID = "ExcludedAppsParam";
        public const string POLLING_INTERVAL_MS_PARAM_ID = "PollingIntervalMs";

        public const int DEFAULT_POLLING_INTERVAL_MS = 300;
        #endregion

        #region MpIParameterHost Overrides

        private MpTriggerPluginFormat _actionComponentFormat;
        public override MpTriggerPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    List<MpPluginParameterValueFormat> ignoredVals =
                        OperatingSystem.IsWindows() ?
                        new[] { "csrss", "dwm", "mwc" }.Select(x => new MpPluginParameterValueFormat() {
                            label = x,
                            value = x,
                            isDefault = true
                        }).ToList() : null;

                    _actionComponentFormat = new MpTriggerPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Ignored Process Names",
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = EXCLUDED_APPS_PARAM_ID,
                                description = "Process names (wihtout extension or folder) will be ignored. Some system processes cannot be queried and will slow ",
                                values = ignoredVals
                            },
                            new MpParameterFormat() {
                                label="Polling Interval (ms)",
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Integer,
                                paramId = POLLING_INTERVAL_MS_PARAM_ID,
                                isRequired = true,
                                minimum = 0,
                                maximum = 5000,
                                values = new[] {
                                    new MpPluginParameterValueFormat() {
                                        value = DEFAULT_POLLING_INTERVAL_MS.ToString(),
                                        isDefault = true
                                    }
                                }.ToList()
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Properties

        #region Model

        public IEnumerable<string> IgnoredProcesses {
            get {
                if (ArgLookup.TryGetValue(EXCLUDED_APPS_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal.ToListFromCsv(param_vm.CsvProps);
                }
                return new List<string>();
            }
        }
        public int PollingIntervalMs {
            get {
                if(ArgLookup.TryGetValue(POLLING_INTERVAL_MS_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return DEFAULT_POLLING_INTERVAL_MS;
            }
            set {
                if (PollingIntervalMs != value) {
                    ArgLookup[POLLING_INTERVAL_MS_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PollingIntervalMs));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvActiveApplicationChangedTrigger(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Protected Methods
        protected override void EnableTrigger() {
            MpPlatformWrapper.Services.ProcessWatcher.RegisterActionComponent(this);
            if (MpPlatformWrapper.Services.ProcessWatcher.IsWatching) {
                return;
            }

        }

        protected override void DisableTrigger() {
            MpPlatformWrapper.Services.ProcessWatcher.UnregisterActionComponent(this);

            if (!MpPlatformWrapper.Services.ProcessWatcher.IsWatching) {
                return;
            }
            MpPlatformWrapper.Services.ProcessWatcher.StopWatcher();
        }

        public override async Task PerformActionAsync(object arg) {
            if (!base.CanPerformAction(arg)) {
                return;
            }
            if(arg is MpPortableProcessInfo ppi) {
                await base.PerformActionAsync(
                        new MpAvActiveAppChangedTriggerOutput() {
                            Previous = null,
                            ProcessInfo = ppi
                        });
            }
        }

        #endregion
    }
}