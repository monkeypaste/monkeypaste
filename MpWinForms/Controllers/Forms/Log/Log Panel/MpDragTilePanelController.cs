using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDragTilePanelController : MpController {
        public MpTilePanelController TilePanelController { get; set; }
        private Point _startDragPoint = Point.Empty, _lastMousePoint,_startTilePoint;
        public MpDragTilePanelController(MpController p,MpTilePanelController dragTile,Point startDragPoint) : base(p) {
            _startDragPoint = startDragPoint;
            _lastMousePoint = _startDragPoint;
            TilePanelController = dragTile;

            StartTileDrag();
        }

        public override void Update() {
            throw new NotImplementedException();
        }
        public void StartTileDrag() {
            //tile chooser panel controller
            var tcpc = ((MpLogFormPanelController)Find(typeof(MpLogFormPanelController))).TileChooserPanelController;
            //get tp screen location
            _startTilePoint = tcpc.TileChooserPanel.PointToScreen(TilePanelController.TilePanel.Location);

            //detach tilepanel from chooser
            tcpc.TileChooserPanel.Controls.Remove(TilePanelController.TilePanel);

            // and reproject location overtop the logform
            //log form controller
            var lfc = ((MpLogFormController)Find(typeof(MpLogFormController)));
            lfc.LogForm.Controls.Add(TilePanelController.TilePanel);
            TilePanelController.TilePanel.Location = lfc.LogForm.PointToClient(_startTilePoint);

            TilePanelController.Drag();

            TilePanelController.TilePanel.BringToFront();
            TilePanelController.TilePanel.Invalidate();
        }
        public void ContinueTileDrag(Point mousePos) {
            TilePanelController.TilePanel.Location = MpPointMath.Add(
                TilePanelController.TilePanel.Location,
                MpPointMath.Subtract(mousePos, _lastMousePoint)
            );
            TilePanelController.TilePanel.Invalidate();
            _lastMousePoint = mousePos;
        }
        public void EndTileDrag() {
            ((MpLogFormController)Find(typeof(MpLogFormController))).LogForm.Controls.Remove(TilePanelController.TilePanel);

            //tile chooser panel controller
            var tcpc = ((MpLogFormPanelController)Find(typeof(MpLogFormPanelController))).TileChooserPanelController;
            tcpc.TileChooserPanel.Controls.Add(TilePanelController.TilePanel);
            TilePanelController.TilePanel.Location = tcpc.TileChooserPanel.PointToClient(_startTilePoint);
            TilePanelController.TilePanel.BringToFront();
            TilePanelController.TilePanel.Invalidate();
            TilePanelController = null;
        }
    }
}
