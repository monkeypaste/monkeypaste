using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;

using System;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpShortcut : MpDbModelBase {
        public const int MIN_USER_SHORTCUT_TYPE = 101;

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpShortcutId")]
        public override int Id { get; set; } = 0;

        [Column("MpShortcutGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string CommandParameter { get; set; } = null;

        public string KeyString { get; set; } = string.Empty;

        public string RouteTypeName { get; set; }

        public string ShortcutTypeName { get; set; }

        public int RoutingDelayMs { get; set; } = 100;

        [Column("b_IsReadOnly")]
        public int IsReadOnlyVal { get; set; } = 0;

        [Column("b_IsInternalOnly")]
        public int IsInternalOnlyVal { get; set; } = 0;

        #endregion

        #region Fk Models

        #endregion

        #region Properties

        [Ignore]
        public Guid ShortcutGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [Ignore]
        public bool IsInternalOnly {
            get => IsInternalOnlyVal == 1;
            set => IsInternalOnlyVal = value ? 1 : 0;
        }

        [Ignore]
        public bool IsReadOnly {
            get => IsReadOnlyVal == 1;
            set => IsReadOnlyVal = value ? 1 : 0;
        }

        [Ignore]
        public MpShortcutType ShortcutType {
            get {
                return ShortcutTypeName.ToEnum<MpShortcutType>();
            }
            set {
                ShortcutTypeName = value.ToString();
            }
        }

        [Ignore]
        public MpRoutingType RoutingType {
            get {
                return RouteTypeName.ToEnum<MpRoutingType>();
            }
            set {
                RouteTypeName = value.ToString();
            }
        }


        #endregion

        #region Static Methods

        public static async Task<MpShortcut> CreateAsync(
            //string shortcutLabel = "",
            string keyString = "",
            MpRoutingType routeType = MpRoutingType.Passive,
            MpShortcutType shortcutType = MpShortcutType.None,
            string commandParameter = null,
            string guid = "",
            bool isReadOnly = false,
            bool isInternalOnly = false) {
            if (shortcutType == MpShortcutType.None) {
                throw new Exception("Needs type");
            }
            if (string.IsNullOrEmpty(keyString)) {
                throw new Exception("Needs keystring");
            }

            guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;

            var dupShortcut = await MpDataModelProvider.GetShortcutAsync(shortcutType.ToString(), commandParameter);
            if (dupShortcut != null) {
                MpConsole.WriteLine($"Updating keyString from '{dupShortcut.KeyString}' to '{keyString}'");
                MpConsole.WriteLine($"Updating routing type from '{dupShortcut.RoutingType}' to '{routeType}'");

                dupShortcut = await MpDataModelProvider.GetItemAsync<MpShortcut>(dupShortcut.Id);
                dupShortcut.KeyString = keyString;
                dupShortcut.RoutingType = routeType;

                await dupShortcut.WriteToDatabaseAsync();
                return dupShortcut;
            }
            var newShortcut = new MpShortcut() {
                ShortcutGuid = System.Guid.Parse(guid),
                KeyString = keyString,
                RoutingType = routeType,
                ShortcutType = shortcutType,
                CommandParameter = commandParameter,
                IsReadOnly = isReadOnly,
                IsInternalOnly = isInternalOnly
            };

            await newShortcut.WriteToDatabaseAsync();

            return newShortcut;
        }

        #endregion

        #region Public Methods
        public MpShortcut() { }


        public override string ToString() {
            return $"Shortcut Id:{Id} Name: '{ShortcutType}' Gesture: '{KeyString}'";
        }

        public override async Task WriteToDatabaseAsync() {
            if (IsReadOnly && Id > 0) {
                // Should be caught before here
                MpDebug.Break();
                return;
            }
            await base.WriteToDatabaseAsync();
        }

        public override async Task DeleteFromDatabaseAsync() {
            //if (IsReadOnly) {
            //    // Should be caught before here
            //    MpDebug.Break();
            //    return;
            //}
            await base.DeleteFromDatabaseAsync();
        }
        #endregion

    }

}
