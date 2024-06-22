using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpTriggerDesignerSettings :
        MpDbModelBase {
        

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTriggerDesignerSettingsId")]
        public override int Id { get; set; }

        [Column("MpTriggerDesignerSettingsGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpActionId")]
        public int ActionId { get; set; } = 0;

        public double TranslateOffsetX { get; set; }
        public double TranslateOffsetY { get; set; }
        public double ZoomFactor { get; set; } = 1;

        [Column("b_IsGridVisible")]
        public int IsGridVisibleValue { get; set; } = 1;

        #endregion

        #region Properties

        [Ignore]
        public Guid TriggerDesignerSettingsGuid {
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
        public bool IsGridVisible {
            get => IsGridVisibleValue == 1;
            set => IsGridVisibleValue = value ? 1 : 0;
        }

        #endregion

        public static async Task<MpTriggerDesignerSettings> CreateAsync(
            string guid = "",
            int actionId = 0,
            double x = 0,
            double y = 0,
            double zoomFactor = 1,
            bool showGrid = true,
            bool suppressWrite = false) {
            if(actionId <= 0) {
                throw new Exception($"Trigger props must have action id");
            }

            var dupCheck = await MpDataModelProvider.GetTriggerDesignerSettingsByActionId(actionId);
            if (dupCheck != null) {
                dupCheck.TranslateOffsetX = x;
                dupCheck.TranslateOffsetY = y;
                dupCheck.ZoomFactor = zoomFactor;
                dupCheck.IsGridVisible = showGrid;
                await dupCheck.WriteToDatabaseAsync();
                return dupCheck;
            }

            var mr = new MpTriggerDesignerSettings() {
                TriggerDesignerSettingsGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                ActionId = actionId,
                TranslateOffsetX = x,
                TranslateOffsetY = y,
                ZoomFactor = zoomFactor,
                IsGridVisible = showGrid
            };

            if (!suppressWrite) {
                await mr.WriteToDatabaseAsync();
            }
            return mr;
        }

        public MpTriggerDesignerSettings() { }

        public override string ToString() {
            return $"TriggerDesignerSettings Id: {Id} ActionId: {ActionId} X: {TranslateOffsetX} Y: {TranslateOffsetY} Scale: {ZoomFactor} ShowGrid: {IsGridVisible}";
        }

    }
}
