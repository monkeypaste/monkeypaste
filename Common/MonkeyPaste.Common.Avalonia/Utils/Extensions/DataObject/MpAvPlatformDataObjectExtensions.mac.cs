using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using MonoMac.AppKit;
using MonoMac.CoreText;
using MonoMac.Foundation;
//using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static partial class MpAvPlatformDataObjectExtensions {
        public static void LogDataObject(this IDataObject ido) {
            var actual_formats = ido.GetAllDataFormats();
            foreach (string af in actual_formats) {
                if (!ido.TryGetData(af, out string dataStr)) {
                    continue;
                }
                object data = ido.Get(af);
                MpConsole.WriteLine($"Format Name: '{af}'", true);
                MpConsole.WriteLine($"Format Type:'{data.GetType()}'");
                if (af == MpPortableDataFormats.MacUrl2 &&
                    data is byte[] bytes) {

                    MpConsole.WriteLine("Format Data (UNICODE):");
                    MpConsole.WriteLine(bytes.ToDecodedString(enc: Encoding.Unicode));

                    NSData arch_data = NSData.FromArray(bytes);
                    NSPropertyListFormat plist_format = NSPropertyListFormat.Binary;

                    NSObject dict_obj =
                        NSPropertyListSerialization.PropertyListWithData(
                            arch_data,
                            NSPropertyListReadOptions.Immutable,
                            ref plist_format,
                            out NSError err);

                    NSArray items = NSArray.FromNSObjects(dict_obj);
                }
                MpConsole.WriteLine("Format Data:");
                MpConsole.WriteLine(dataStr);
            }
        }
        public static async Task FinalizePlatformDataObjectAsync(this IDataObject ido) {
            // NOTE this is a little funky but this does nothing on mac since dnd is part of clipboard
            await Task.Delay(1);

            return;
        }

        public static async Task WriteToPasteboardAsync(this IDataObject ido, bool isDnd) {
            await Task.Delay(1);
            NSPasteboard pb = isDnd ? NSPasteboard.FromName(NSPasteboard.NSDragPasteboardName) : NSPasteboard.GeneralPasteboard;

            pb.ClearContents();


            var formats = ido.GetAllDataFormats().ToList();
            foreach (string format in formats) {
                switch (format) {
                    case MpPortableDataFormats.Files: {
                            // from https://stackoverflow.com/a/5843278/105028
                            if (!ido.TryGetData(format, out IEnumerable<string> fpl) ||
                                !fpl.Any()) {
                                continue;
                            }
                            NSMutableArray nsarr = new NSMutableArray();
                            foreach (var fp in fpl) {
                                NSUrl url = NSUrl.FromFilename(fp);
                                nsarr.Add(url);
                                pb.SetDataForType(NSData.FromUrl(url), MpPortableDataFormats.MacFiles1);
                            }
                            //pb.WriteObjects(new NSImage[] { null });

                            //pb.DeclareTypes(new[] { MpPortableDataFormats.MacFiles1, MpPortableDataFormats.MacFiles2, MpPortableDataFormats.Text }, null);
                            //NSData data = NSKeyedArchiver.ArchivedDataWithRootObject(nsarr);
                            //pb.SetDataForType(data, MpPortableDataFormats.MacFiles1);
                            //pb.SetDataForType(data, MpPortableDataFormats.MacFiles2);

                            //pb.SetDataForType(NSData.FromUrl(NSUrl.FromFilename(fpl.FirstOrDefault())), MpPortableDataFormats.MacFiles1);
                            //pb.SetDataForType(NSData.FromUrl(NSUrl.FromFilename(fpl.FirstOrDefault())), MpPortableDataFormats.MacFiles2);

                            pb.SetDataForType(NSData.FromString(string.Join(Environment.NewLine, fpl.Select(x => x.ToFileSystemUriFromPath()))), MpPortableDataFormats.Text);
                            break;
                        }
                    case MpPortableDataFormats.Image: {
                            // from https://stackoverflow.com/a/18124824/105028
                            if (!ido.TryGetData(format, out string imgBase64)) {
                                continue;
                            }
                            //NSUrl nsurl = NSUrl.FromString(imgBase64.ToBase64ImageUrl());
                            //NSData nsdata = NSData.FromUrl(nsurl);
                            //NSImage nsimg = new NSImage(nsdata);
                            //NSArray arr = NSArray.FromNSObjects(nsimg);
                            //ido.Set(format, arr);
                            ido.Set(format, imgBase64.ToBytesFromBase64String());
                            break;
                        }
                    default: {
                            if (!ido.TryGetData(format, out string dataStr)) {
                                continue;
                            }
                            //NSString nsstr = new NSString(dataStr);
                            //NSArray arr = NSArray.FromNSObjects(nsstr);
                            //ido.Set(format, arr);
                            ido.Set(format, dataStr);
                            break;
                        }

                }
            }
        }
    }

    /*
    final class TidiFile : NSObject, Codable {

    var url : URL
    var createdDateAttribute : Date
    var modifiedDateAttribute : Date
    var fileSizeAttribute: Int

    init(url: URL, createdDateAttribute: Date, modifiedDateAttribute: Date, fileSizeAttribute: Int) {
        self.url = url
        self.createdDateAttribute = createdDateAttribute
        self.modifiedDateAttribute = modifiedDateAttribute
        self.fileSizeAttribute = fileSizeAttribute
    }

    convenience init?(pasteboardPropertyList propertyList: Any, ofType type: NSPasteboard.PasteboardType) {
        guard let data = propertyList as? Data,
            let tidi = try? PropertyListDecoder().decode(TidiFile.self, from: data) else { return nil }
        self.init(url: tidi.url, createdDateAttribute: tidi.createdDateAttribute, modifiedDateAttribute: tidi.modifiedDateAttribute, fileSizeAttribute: tidi.fileSizeAttribute)
    }

}

extension TidiFile : NSPasteboardWriting, NSPasteboardReading
{

    public func writingOptions(forType type: NSPasteboard.PasteboardType, pasteboard: NSPasteboard) -> NSPasteboard.WritingOptions {
        return .promised
    }

    public func writableTypes(for pasteboard: NSPasteboard) -> [NSPasteboard.PasteboardType] {
        return [.tidiFile]
    }

    public func pasteboardPropertyList(forType type: NSPasteboard.PasteboardType) -> Any? {
        if type == .tidiFile {
            return try? PropertyListEncoder().encode(self)
        }
        return nil
    }

    public static func readableTypes(for pasteboard: NSPasteboard) -> [NSPasteboard.PasteboardType] {
        return [.tidiFile]
    }

    public static func readingOptions(forType type: NSPasteboard.PasteboardType, pasteboard: NSPasteboard) -> NSPasteboard.ReadingOptions {
        return .asData
    }

}
    */
    //public class MpCustomPasteBoardItem : NSPasteboardReading {
    //    public List<string> ReadableTypes { get; } = new List<string>();
    //    public override string[] GetReadableTypesForPasteboard(NSPasteboard pasteboard) {
    //        return ReadableTypes.ToArray();
    //    }

    //    public override NSPasteboardReadingOptions GetReadingOptionsForType(string type, NSPasteboard pasteboard) {
    //        throw new NotImplementedException();
    //    }

    //    public override NSObject InitWithPasteboardPropertyList(NSObject propertyList, string type) {
    //        NSPropertyListSerialization.
    //    }
    //}
}