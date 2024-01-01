using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvActionOutput : MpIActionOutputNode {
        public virtual bool CanExecutionContinue { get; set; } = true;
        public MpAvActionOutput Previous { get; set; }
        public abstract object OutputData { get; }
        public abstract string ActionDescription { get; }

        MpIActionOutputNode MpIActionOutputNode.Previous => Previous;

        object MpIActionOutputNode.Output => OutputData;
        string MpILabelText.LabelText => ActionDescription;

        public MpCopyItem CopyItem { get; set; }
        public override string ToString() {
            return OutputData.ToStringOrDefault();
        }
    }
    public class MpAvTriggerInput : MpAvActionOutput {
        public override object OutputData => CopyItem;
        public override string ActionDescription => "Trigger Activated...";
    }
    public class MpAvMonkeyCopyOutput : MpAvActionOutput {
        public override object OutputData => CopyItem;
        public override string ActionDescription {
            get {
                return $"MonkeyCopy Input was {OutputData}";
            }
        }
    }
    public class MpAvFileSystemTriggerOutput : MpAvActionOutput {
        public override object OutputData { get; }

        public WatcherChangeTypes FileSystemChangeType { get; set; }
        public override string ActionDescription {
            get {
                return $"File system change of type: '{FileSystemChangeType}' occured";
            }
        }
    }
    public class MpAvAnalyzeOutput : MpAvActionOutput {
        public override object OutputData =>
            NewCopyItem == null ? PluginResponse : NewCopyItem;
        public MpAnalyzerPluginResponseFormat PluginResponse { get; set; }
        public MpCopyItem NewCopyItem { get; set; }
        public override string ActionDescription {
            get {
                //return $"Result of analysis of CopyItem({CopyItem.Id},{CopyItem.Title}) was: " + Environment.NewLine + NewCopyItem.ToStringOrDefault();
                return $"Result of analysis: '{OutputData.ToStringOrEmpty()}'";
            }
        }
    }
    public class MpAvClassifyOutput : MpAvActionOutput {
        public override object OutputData => TagId;
        public int TagId { get; set; }
        public override string ActionDescription {
            get {
                if (CopyItem == null) {
                    return "Nothing to classify";
                }
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) Classified to Tag({TagId})";
            }
        }
    }

    public class MpAvAppCommandOutput : MpAvActionOutput {
        public override object OutputData => ShortcutId;
        public int ShortcutId { get; set; }
        public override string ActionDescription {
            get {
                if (CopyItem == null) {
                    return "Nothing to classify";
                }
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) Classified to Tag({ShortcutId})";
            }
        }
    }
    public class MpAvConditionalOutput : MpAvActionOutput {
        public override object OutputData => Matches;
        public override bool CanExecutionContinue => WasConditionMet;
        public List<MpAvConditionalMatch> Matches { get; set; }
        public bool WasConditionMet {
            get {
                bool was_met = Matches != null && Matches.Count > 0;
                if (Flip) {
                    return !was_met;
                }
                return was_met;
            }
        }
        public bool Flip { get; set; }

        public override string ActionDescription {
            get {
                if (Matches == null || Matches.Count == 0) {
                    return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was NOT a match";
                }
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was matched w/ Match Value: {string.Join(Environment.NewLine, Matches)}";
            }
        }
    }
    public class MpAvFileWriterOutput : MpAvActionOutput {
        public string OutputFilesStr { get; set; }
        public override object OutputData => OutputFilesStr;

        public override string ActionDescription {
            get {
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was written to file path: '{OutputFilesStr}'";
            }
        }
    }
    public class MpAvSetClipboardOutput : MpAvActionOutput {
        public override object OutputData => CopyItem;
        public MpPortableDataObject ClipboardDataObject { get; set; }
        public override string ActionDescription {
            get {
                return $"Clipboard set to {ClipboardDataObject}";
            }
        }
    }
}
