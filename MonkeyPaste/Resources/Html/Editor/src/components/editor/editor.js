
var isLoaded = false;

var isShowingTemplateContextMenu = false;
var isShowingTemplateColorPaletteMenu = false;
var isShowingTemplateToolbarMenu = false;
var isShowingEditorToolbar = true;

var IgnoreTextChange = false;
var IgnoreSelectionChange = false;

var LastSelection = { index: 0, length: 0 };
var LastSelectedHtml;
var SelectedHtml;

////////////////////////////////////////////////////////////////////////////////
/*  These var's can be parsed and replaced with values from app */
var envName = '';
var isPastingTemplate = false;
var fontFamilys = null;
var fontSizes = [];

var defaultFontSize = '12px';
var defaultFontFamily = 'Arial';
var indentSize = 5;
////////////////////////////////////////////////////////////////////////////////

function setWpfEnv() {
    envName = 'wpf';
}

function init(reqMsgStr) { 
    if (reqMsgStr == null) {
        let sample1 = '<p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">//rtbvm.HasViewChanged = true;</span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">rtbvm.OnPropertyChanged(</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">nameof</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">(rtbvm.CurrentSize)); </span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> cilv =  </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">this</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.GetVisualAncestor<</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(43,145,175);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpContentListView</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">>(); </span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">cilv.UpdateAdorner();</span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><br copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> rtbl = cilv.GetVisualDescendents<</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(43,145,175);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">RichTextBox</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">>(); </span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">double</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> totalHeight = rtbl.Sum(x => x.ActualHeight) +</span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(43,145,175);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpMeasurements</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.Instance.ClipTileEditToolbarHeight + </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(43,145,175);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpMeasurements</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.Instance.ClipTileDetailHeight; </span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> ctcv =  </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">this</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">.GetVisualAncestor<</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(43,145,175);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">MpClipTileContainerView</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">>(); </span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">ctcv.ExpandBehavior.Resize(totalHeight); </span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,255);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">var</span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"> sv = cilv.ContentListBox.GetScrollViewer(); </span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6">sv.InvalidateScrollInfo();</span></p><p class="ql-align-left" copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"><br copyitemguid="8a26a5ad-66eb-43b4-b4e6-4fa4005ebed6"></p>';
        let sample2 = '<p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><br copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//public async Task FillAllTemplates() {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//    bool hasExpanded = false;</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//    foreach (var rtbvm in SubSelectedContentItems) {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//        if (rtbvm.HasTokens) {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            rtbvm.IsSelected = true;</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            rtbvm.IsPastingTemplate = true;</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            if (!hasExpanded) {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                //tile will be shrunk in on completed of hide window</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                MainWindowViewModel.ExpandClipTile(HostClipTileViewModel);</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                if (!MpClipTrayViewModel.Instance.IsPastingHotKey) {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                    PasteTemplateToolbarViewModel.IsBusy = true;</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                }</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                hasExpanded = true;</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            }</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            PasteTemplateToolbarViewModel.SetSubItem(rtbvm);</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            await Application.Current.Dispatcher.BeginInvoke((Action)(() => {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                while (!PasteTemplateToolbarViewModel.HaveAllSubItemTemplatesBeenVisited) {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                    System.Threading.Thread.Sleep(100);</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//                }</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            }), DispatcherPriority.Background);</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><br copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            //await Task.Run(() => {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            //    while (!HostClipTileViewModel.PasteTemplateToolbarViewModel.HaveAllSubItemTemplatesBeenVisited) {</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            //        System.Threading.Thread.Sleep(100);</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            //    }</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            //    //TemplateRichText is set in PasteTemplateCommand</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            //});</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//            rtbvm.TemplateHyperlinkCollectionViewModel.ClearSelection();</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//        }</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><br copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//    }</span></p><p class="ql-align-left" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938"><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,0,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">        </span><span class="ql-font-consolas" style="font-size: 12.6666666666667px; color: rgb(0,128,0);" copyitemguid="707c5722-f7a7-4459-b8c3-6a1179a59938">//}</span></p>';

        reqMsg = {
            envName: 'web',
            isPasteRequest: false,
            isReadOnlyEnabled: false,
            itemEncodedHtmlData: sample1 + sample2,
            usedTextTemplates: []
        }
    } else {
        let reqMsgStr_decoded = atob(reqMsgStr);
        reqMsg = JSON.parse(reqMsgStr_decoded);
    }

    loadQuill(reqMsg);

    if (reqMsg.envName == 'web') {
        //for testing in browser
    } else {
        if (reqMsg.isReadOnlyEnabled) {
            hideToolbar();
            enableReadOnly();
        } else {
            showToolbar();
            disableReadOnly();
        }

        //hideScrollbars();
        //disableScrolling();
    }


    isLoaded = true;
    return "GREAT!";
}

function loadQuill(reqMsg) {
    //if (isLoaded) {
    //    return;
    //}

    Quill.register("modules/htmlEditButton", htmlEditButton);
    Quill.register({ 'modules/better-table': quillBetterTable }, true);

    registerContentGuidAttribute();
    registerTemplateSpan();

    // Append the CSS stylesheet to the page
    var node = document.createElement('style');
    node.innerHTML = registerFontStyles(reqMsg.envName);
    document.body.appendChild(node);

    quill = new Quill(
        '#editor', {
            //debug: true,
        placeholder: '',
        theme: 'snow',
        modules: {
            table: false,
            toolbar: registerToolbar(reqMsg.envName),
            htmlEditButton: {
                syntax: true,
            },
            'better-table': {
                operationMenu: {
                    items: {
                        unmergeCells: {
                            text: 'Unmerge cells'
                        }
                    },
                    color: {
                        colors: ['green', 'red', 'yellow', 'blue', 'white'],
                        text: 'Background Colors:'
                    }
                }
            },
            keyboard: {
                bindings: quillBetterTable.keyboardBindings
            }
        }
    });

    quill.root.setAttribute("spellcheck", "false");

    initTableToolbarButton();

    window.addEventListener('click', (e) => {
        if (e.path.find(x => x.classList && x.classList.contains('edit-template-toolbar')) != null ||
            e.path.find(x => x.classList && x.classList.contains('paste-template-toolbar')) != null ||
            e.path.find(x => x.classList && x.classList.contains('context-menu-option')) != null || 
            e.path.find(x => x.classList && x.classList.contains('ql-toolbar')) != null) {
            //ignore clicks within template toolbars
            return;
        }
        if (e.path.find(x => x.classList && x.classList.contains('ql-template-embed-blot')) == null) {
            hideAllContextMenus();
            hideEditTemplateToolbar();
            hidePasteTemplateToolbar();
            clearTemplateFocus();
        }
    });

    quill.on('selection-change', function (range, oldRange, source) {
        //LastSelectedHtml = SelectedHtml;
        //SelectedHtml = getSelectedHtml();

        if (range) {
            refreshFontSizePicker();
            refreshFontFamilyPicker();

            if (range.length == 0) {
                log('User cursor is on', range.index);

            } else {
                var text = quill.getText(range.index, range.length);
                log('User has highlighted', text);
            }

            refreshTemplatesAfterSelectionChange();

            //if (IgnoreSelectionChange) {
            //    IgnoreSelectionChange = false;
            //    return;
            //}
            //let templateDocIdxLookup = getTemplateElementsWithDocIdx();

            //for (var i = 0; i < templateDocIdxLookup.length; i++) {
            //    let tdil = templateDocIdxLookup[i];
            //    if (parseInt(tdil[0]) == parseInt(range.index)) {
            //        range.index++;
            //        IgnoreSelectionChange = true;
            //        quill.setSelection(range, Quill.sources.SILENT);
            //    }
            //}
        } else {
            log('Cursor not in the editor');
        }
        if (!range && oldRange) {
            //blur occured
            //quill.setSelection(oldRange);
        }   
    });

    quill.on('text-change', function (delta, oldDelta, source) {
        if (!isLoaded) {
            return;
        }
        if (IgnoreTextChange) {
            IgnoreTextChange = false;
            return;
        }
        let srange = quill.getSelection();
        if (!srange) {
            return;
        }
        let idx = 0;
        if (PasteNode) {
            let retargetedNode = retargetContentItemDomNode(PasteNode);
            let contentBlot = getContentItemFromDomNode(retargetedNode);
            IgnoreTextChange = true;
            for (var i = 0; i < delta.ops.length; i++) {
                let op = delta.ops[i];

                if (op.retain) {
                    idx += op.retain;
                }
                if (op.insert) {

                    let insertRange = { index: idx, length: op.insert.length };
                    if (contentBlot.copyItemGuid) {
                        IgnoreTextChange = true;
                        quill.formatText(insertRange, 'copyItemGuid', contentBlot.copyItemGuid);
                        IgnoreTextChange = true;
                        quill.formatText(insertRange, 'copyItemSourceGuid', contentBlot.copyItemSourceGuid);
                    } else {
                        IgnoreTextChange = true;
                        quill.formatText(insertRange, 'fromUser', '');
                    }
                    idx += op.insert.length;
                }
            }
            IgnoreTextChange = false;
            PasteNode = null;
        } else {
            IgnoreTextChange = true;
            for (var i = 0; i < delta.ops.length; i++) {
                let op = delta.ops[i];
                if (op.retain) {
                    idx += op.retain;
                }
                if (op.insert) {
                    let insertRange = { index: idx, length: op.insert.length };
                    IgnoreTextChange = true;
                    quill.formatText(insertRange, 'fromUser', '');
                    idx += op.insert.length;
                }
            };
            IgnoreTextChange = false;
        }


        //idx = 0;

        //for (var i = 0; i < delta.ops.length; i++) {
        //    let op = delta.ops[i];

        //    if (op.retain) {
        //        idx += op.retain;
        //    }
        //    if (op.delete) {
        //        let templateDocIdxLookup = getTemplateElementsWithDocIdx();
        //        if (templateDocIdxLookup.filter(x => x[0] == srange.index - 1) != null) {
        //            log('here')
        //        }
        //        if (templateDocIdxLookup.filter(x => x[0] == srange.index) != null) {
        //            IgnoreTextChange = true;
        //            quill.insertText(srange.index, ' ', Quill.sources.SILENT);
        //        }
        //        if (templateDocIdxLookup.filter(x => x[0] == srange.index + 1) != null) {
        //            log('or or here')
        //        }
        //    }
        //}

        //initContentRangeListeners();
        if (oldDelta) {
            //log('old:');
            //log(JSON.stringify(oldDelta));
        }
        if (delta) {
            //log('new:');
            //log(JSON.stringify(delta));
        }
    });

    initClipboard();

    initContent(reqMsg.itemEncodedHtmlData);

    initTemplates(reqMsg.usedTextTemplates, reqMsg.isPasteRequest);

    initDragDrop();

    refreshFontSizePicker();
    refreshFontFamilyPicker();
}

function registerTables() {
    var tableOptions = [];
    var maxRows = 7;
    var maxCols = 7;

    for (let r = 1; r <= maxRows; r++) {
        for (let c = 1; c <= maxCols; c++) {
            tableOptions.push('newtable_' + r + '_' + c);
        }
    }
    return tableOptions;
}

function registerToolbar(envName) {
    let sizes = registerFontSizes();
    let fonts = registerFontFamilys(envName);

    var toolbar = {
        container: [
            //[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
            [{ 'size': sizes }],               // font sizes
            [{ 'font': fonts.whitelist }],
            ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
            ['blockquote', 'code-block'],

            // [{ 'header': 1 }, { 'header': 2 }],               // custom button values
            [{ 'list': 'ordered' }, { 'list': 'bullet' }, {'list': 'check'}],
            [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
            [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
            [{ 'direction': 'rtl' }],                         // text direction

            // [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
            ['link', 'image', 'video', 'formula'],
            [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
            [{ 'align': [] }],
            // ['clean'],                                         // remove formatting button
            // ['templatebutton'],
            [{ 'Table-Input': registerTables() }]
        ],
        handlers: {
            'Table-Input': () => { return; }
        }
    };

    return toolbar;
}


function setText(text) {
    quill.setText(text + '\n');
}

function setHtml(html) {
    quill.root.innerHTML = html;
}

function getContentsAsJson() {
    return JSON.stringify(quill.getContents());
}

function setContents(jsonStr) {
    quill.setContents(JSON.parse(jsonStr));
}

function getText() {
    //var text = quill.getText(0, quill.getLength() - 1);
    //return text;
    var text = quill.root.innerText;
    return text;
}

function getSelectedText() {
    var selection = quill.getSelection();
    return quill.getText(selection.index, selection.length);
}

function getHtml() {
    //document.getElementsByClassName
    //var val = document.getElementsByClassName("ql-editor")[0].innerHTML;
    clearTemplateFocus();
    var val = quill.root.innerHTML;
    return unescape(val);
}

function getEncodedHtml() {
    resetTemplates();
    var result = encodeTemplates();
    return result;
}

function getSelectedHtml(maxLength) {
    maxLength = maxLength == null ? Number.MAX_SAFE_INTEGER : maxLength;

    var selection = quill.getSelection();
    if (selection == null) {
        return '';
    }
    if (!selection.hasOwnProperty('length') || selection.length == 0) {
        selection.length = 1;
    }
    if (selection.length > maxLength) {
        selection.length = maxLength;
    }
    var selectedContent = quill.getContents(selection.index, selection.length);
    var tempContainer = document.createElement('div')
    var tempQuill = new Quill(tempContainer);

    tempQuill.setContents(selectedContent);
    let result = tempContainer.querySelector('.ql-editor').innerHTML;
    tempContainer.remove();
    return result;
}

function getSelectedHtml2() {
    var selection = window.getSelection();
    if (selection.rangeCount > 0) {
        var range = selection.getRangeAt(0);
        var docFrag = range.cloneContents();

        let docFragStr = domSerializer.serializeToString(docFrag);

        const xmlnAttribute = ' xmlns="http://www.w3.org/1999/xhtml"';
        const regEx = new RegExp(xmlnAttribute, 'g');
        docFragStr = docFragStr.replace(regEx, '');
        return docFragStr;
    }
    return '';
}

function isEditorLoaded() {
    return isLoaded;
}

function hideToolbar() {
    isShowingEditorToolbar = false;
    $(".ql-toolbar").css("display", "none");
    moveEditorTop(0);
}

function showToolbar() {
    isShowingEditorToolbar = true;
    $(".ql-toolbar").css("display", "flex");
    let tbh = $(".ql-toolbar").outerHeight();
    moveEditorTop(tbh);
}

function getEditorWidth() {
    var editorRect = document.getElementById('editor').getBoundingClientRect();
    //var editorHeight = parseInt($('.ql-editor').wi());
    return editorRect.width;
}

function getEditorHeight() {
    var editorRect = document.getElementById('editor').getBoundingClientRect();
    //var editorHeight = parseInt($('.ql-editor').outerHeight());
    return editorRect.height;
}

function getContentWidth() {
    var bounds = quill.getBounds(0, quill.getLength());
    return bounds.width;
}

function getContentHeight() {
    var bounds = quill.getBounds(0, quill.getLength());
    return bounds.height;
}

function getToolbarHeight() {
    var toolbarHeight = parseInt($('.ql-toolbar').outerHeight());
    return toolbarHeight;
}

function getTemplateToolbarHeight() {
    if (!isShowingEditTemplateToolbar && !isShowingPasteTemplateToolbar) {
        return 0;
    }
    if (isShowingEditTemplateToolbar) {
        return parseInt($("#editTemplateToolbar").outerHeight());
    } else if (isShowingPasteTemplateToolbar) {
        return parseInt($("#pasteTemplateToolbar").outerHeight());
    }
    return 0;    
}

function getTotalHeight() {
    var totalHeight = getToolbarHeight() + getEditorHeight() + getTemplateToolbarHeight();
    return totalHeight;
}

function isReadOnly() {
    var isEditable = $('.ql-editor').attr('contenteditable');
    return !isEditable;
}

function enableReadOnly() {
    $('.ql-editor').attr('contenteditable', false);
    $('.ql-editor').css('caret-color', 'transparent');

    hideToolbar();

    //hideScrollbars();
    disableScrolling();

    //return 'MpQuillResponseMessage'  updated master collection of templates
    let qrmObj = {
        itemEncodedHtmlData: getEncodedHtml(),
        userDeletedTemplateGuids: userDeletedTemplateGuids,
        updatedAllAvailableTextTemplates: getAvailableTemplateDefinitions()
    };
    let qrmJsonStr = JSON.stringify(qrmObj);

    log('enableReadOnly() response msg:');
    log(qrmJsonStr);

    return btoa(qrmJsonStr);
}

function disableReadOnly(disableReadOnlyReqStr) {
    let disableReadOnlyMsg = null;

    if (disableReadOnlyReqStr == null) {
        disableReadOnlyMsg = {
            allAvailableTextTemplates: [],
            editorHeight: window.visualViewport.height
        };
    } else {
        let disableReadOnlyReqStr_decoded = atob(disableReadOnlyReqStr);
        disableReadOnlyMsg = JSON.parse(disableReadOnlyReqStr_decoded);
    }

    availableTemplates = disableReadOnlyMsg.allAvailableTextTemplates;
    document.body.style.height = disableReadOnlyMsg.editorHeight;

    $('.ql-editor').attr('contenteditable', true);
    $('.ql-editor').css('caret-color', 'black');

    document.getElementById('editor').style.height = getContentHeight();

    showToolbar();

    showScrollbars();
}

function hideScrollbars() {
    //hides scrollbars without disabling scrolling (for use when scrolling to search match)
    document.querySelector('body').style.overflow = 'hidden';
    //let isNew = false;
    //var style = document.getElementsByName('style');
    //if (style == null) {
    //    isNew = true;
    //    style = document.createElement('style');
    //}

    //style.type = 'text/css';
    //style.innerHTML = '::-webkit-scrollbar{display:none}';
    //if (isNew) {
    //    document.getElementsByTagName('body')[0].appendChild(style);
    //}
}

function showScrollbars() {
    //hides scrollbars without disabling scrolling (for use when scrolling to search match)
    
    document.querySelector('body').style.overflow = 'auto';

    //if (window.innerHeight < )
    //let isNew = false;
    //var style = document.getElementsByName('style');
    //if (style == null) {
    //    isNew = true;
    //    style = document.createElement('style');
    //}

    //style.type = 'text/css';
    //var overflowStyle = 'auto';
    //if (window)
    //style.innerHTML = '::-webkit-scrollbar{display: block; width: 10em; overflow: auto; height: 2em;}';
    //if (isNew) {
    //    document.getElementsByTagName('body')[0].appendChild(style);
    //}
}

function disableScrolling() {
    document.querySelector('body').style.overflow = 'hidden';
}

function enableScrolling() {
    document.querySelector('body').style.overflow = 'scroll';
}

function decodeHtml(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

function createLink() {
    var range = quill.getSelection(true);
    if (range) {
        var text = quill.getText(range.index, range.length);
        quill.deleteText(range.index, range.length);
        var ts = '<a class="square_btn" href="https://www.google.com">' + text + '</a>';
        quill.clipboard.dangerouslyPasteHTML(range.index, ts);

        log('text:\n' + getText());
        console.table('\nhtml:\n' + getHtml());
    }
}

function moveToolbarTop(y) {
    $(".ql-toolbar").css("position", "absolute");
    $(".ql-toolbar").css("top", y);

    //var viewportBottom = window.scrollY + window.innerHeight;
    //let tbh = $(".ql-toolbar").outerHeight();
    //if (y <= 0) {
    //    //keyboard is not visible
    //    $(".ql-toolbar").css("top", y);
    //    $("#editor").css("top", y + tbh);
    //} else {
    //    $(".ql-toolbar").css("top", y - tbh);
    //    $("#editor").css("top", 0);
    //}
    //$("#editor").css("bottom", viewportBottom - tbh);
}

function movePasteTemplateToolbarTop(y) {
    $("#pasteTemplateToolbar").css("position", "absolute");
    $("#pasteTemplateToolbar").css("top", y);
}

function moveEditorTop(y) {
    $("#editor").css("position", "absolute");
    $("#editor").css("top", y);
}

function getTemplateIconStr(isEnabled) {
    if (isEnabled) {
        //black
        return 'data:image/png;base64,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';
        //yellow
        //return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAaVBMVEX/////wQf/vwD/24f/3Iz///3/vQD/ykL/yTj/yT3/0WH/0F7/0Fv/y0T/y0f/yTr/35T/wxT/8tP/6Lj/5az/7sj/9uX/46P/1G7/677/57P/2oL/xSj/8M//1nj//fb//O//4Zz/zVBsBjAmAAAEzklEQVR4nO2di3qiMBBGIUpa1168tFZ70933f8hl0KqVWCuZyfzhm/MAfJ7vhAmwCy0KwzAMwzAMwzAMwzAMwzAMWKq3QSzDyXymrXGe6ta7eLwvl8/aKmGqW1fy4Pz0SdsmAJ9g4zhaaAudwirYOIJl5Bas8QNtqWMEBLEURQRrRZiFKiRYK4KMGzHB0j1ouzXICdYR59p2haxg6d619YQF64gfPRcs3ae24KOsYOmmyoLCBUvtDSOBYOk176TEl2hjqHhdw1WwvuetOXcsN9ET5Cno/PJ1tvh4Gp9xdEM1QZ6C/m29O+Dmj4cyZCroV0fHfAkpahlyFfw+KEOKSoZcBU9//bh9WB1Dtn3w9MCbdkQVQ7Ztov3j71pH1jBk2+h9++n2UyuigiHfpZrftA7+AWDIeKnm162jL/QNOS+2Aw1n6oasF9uB83Cubch7uxS4qF4qz9JqxHq71L5/Xyvvh+z3g/40YjthUkOBG96TM7F9FiY1ZF6i25/vjhVXuvcWUv/4sl+o60/d+0OJglvF6XC2WS+eP53uPb7gUzV34TlNGkOxgpdJY5jiuaiqoWLBNIZJHvxqGuoKJjBUXaIpDDWHTBJD7YLihuoFxQ3VCwob6i9RacMbAEFJwwpCUNIQQ1DOEKSgoCHCkGkQMoQpKGUIsU3skDHEKShjiFRQxhCpoIQh0JBp4DcEE2Q3RCvIbwg1ZBp4DfEKMhtibRM7WA3vAQU5DRGXaMlqCFmQ0RC0IKMhqiCXYQW6REs2Q9iCXIa4BZkMgQvyGCIX5DCE3SZ2xBtiF4w3RC8YbwgvGGkIvNHviTPELxhpmEHBOMMcCkYZPmQh2H77q2cFS/dQ9bugu+laMIshE/Ntk1wEOxccB//POBzdC75lInjfVfC574LFNIuTMOIDSn+zSNh9yBRV3wsWrzkkjCgYeh8cj6iPmOWwSKMKBl4lhsPdd73YbsA/DeMKFsUEfZW6m6iCRTEAN4y4ksnDMLog+iqNLxh+XRqG2CHTgLxbcBSE3vFZChbBTzFhELnRH0A9EbkK1mirhGHYJvYEv/umDc+Q+eId70zkLFggbhi8BWsmYIqMQ+aLJZQie0EC6aG3GwkIFkX4W6EaMA8ZPEWBc/ALjIUqVpBAqChYkLhTV3QjwYKEdkXhgoRuRfGChGZF0SFzQG+iCm30bbQqJiqop5hgyBzQWKgJCxLpKyYtSKTeNJJsE99JWzF5QSJlRYWCRLqKiYfMgVQT1d0qCaaqqLRE0ym6R0XBFAtVWVC+ouoS3SK7aSgOmQOSFQEKEnIVIQoSUhVBChIyExWmICFREaggwa+ovg+ewr1Q4QS5K4It0S2cmwbUkDnAVxGyIMFVEbQgwVMRcMgc4JiowAWJ+IrQBYlYRfCCRNxCzUAwriL8Et3SfdPIoiDRtWImBYluFbMpSHSpmFFB4vqJmlVB4tqKmRUkrlPMriBxzULNUvCaihku0S2/3TQyLUj8rmK2BYnfVMy4IHG5YuaClydq9oKXKvZA8GfFXgj+tFB7Inj+hSLf+btqcEyCf7nXL7V/FyOz91ZGX660fxUvL+VxR+fdpDcrdM9q7Lx3jv7gtPv32j8/oprNJ8PB5HXWTz3DMAzDMAzDMAzDMAzDMHrEf9VjaGNa1FYRAAAAAElFTkSuQmCC";
    } else {
        //gray
        return 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAALsAAADDCAYAAADa4WDGAAAKTElEQVR4nO3dPWwb5x3H8d//3CGbvLeAVaQtskkWPXgzs2WLtmYzC+mO3Sq0RV+m2Fu2OCOPEkxvzUaPnUKPGchKY1EUkIEMzSZvXcx/B92ljESK9/K83d3vA3ihpOMD+IsHx7vnOQJERERERERERERERFSV+B4AXdN/ffGN7zH8n8x8jyAnv/zzc2PHMnUgqi5Jki8BnPgeR6Cm+b80Td/VORBj9yyO45ciMvA9jtCp6pWIvADwomr09wyPiUpg6MWJyAcA+qr6216v99/FYvFt6WNYGBcVwNDrUdWZiByWmeUZuwcM3ZhLETkcjUYXRX6ZsTvG0M1S1asoivpFgmfsDjF0O7IPr7vbTmkiVwPqOoZuj4jcV9Xptt/j1RgHGLp9IrL78OHD88Vi8c9Nv8OZ3TKG7o6ITO76OWd2ixi6WyLywcHBwdWma/Cc2S1h6N4MNv2AV2MsYOh+icj+ukuRnNkNY+j+LZfL/rrXf+J4HK2VJMmOqk5FpO97LF2X/R98dfN1xm5AFvpMRPZ9j4UAAGv/H3gaUxNDD9LuuhcZew0MvVl4GlMRQy9kCuB8zev7AA4dj4WxV8HQt3oB4NldC7OSJNkB8AyWtiPqv7/YkQ//8qP353X2khj6ZmWW2+aGw+Hecrmcich9k2MZ/fWjvvz8929WX+M5ewkM/W4iclImdAAYjUYXIuJkszljL4ihb/UiTdNXVf4w+7sXhsdzC2MvgKHfTVWvcH3+Xcez7DjWMPYtGPp2IjKp+0yXNE3fbVuiWxdjvwNDL8ZUpIzdE4ZeXNkPpbaPAwB4v9y9+RJjX4Oht4DI7s2XGPsNDL29GPsKhl7N0dHRg5COswljzzD06u7du9cP6TibMHYw9LpUdRDScTbpfOwMvT4R6cdx/KTOMeI4fmJ7l1enY2fo5ojIJFvJWFqSJDu2r7EDHY6doRu3q6qz4XC4V+aPhsPhnqrOsGF3kUmdjJ2h2yEi+8vlsnDwK8t7nfw/dG7zBkO3K3vI6Hkcx7NszcytlZBJkjxV1YGq9kUsbalQvfX/26nNGwy9O0Z/+tVMfvHHj1df68xpDEOnTsTO0AnoQOwMnXKtj52hU67VsWcPGWXoBKDFsfNpunRTK2Nn6LRO62Jn6LRJq2Jn6HSX1sTO0GmbVsTO0KmIxsfO0KmoRsfO0KmMxsYex/HvGDqV0cj17EmSPIWDp75SuzRuZs9Cn/geBzVPo2Jn6FRHY2Jn6FRXI2Jn6GRC8LEfHx9/CoZOBgQd+3A43HPx8BzqhmBjt/WVgdRdQcaeJMkOQyfTgot9ZYM0QyejgoqdTwIgm4KJnaGTbUHEztDJhSBiZ+jkgvfY+WwXcsVr7Nx8QS55i52hk2teYmfo5IPz2Bk6+eI0du4bJZ+c7UHlvlHyzcnMzs0XFALrsTN0CoXV2Bk6hcRa7Nl32U9sHZ+oLCuxZ99wPLVxbKKqjMfO7XQUKqOxM3QKhur5zZeMxc7QKShRdHXrJRPHPTo6esDQKXS176Bmu4ymDJ1CV2tm53Y6apLKsTN0CpWqTuTDPzy/+bpUORhDp1Cp6mQ8Hv9m3c9Kz+wMnUKlqucicrLp51VOYyYMnUKThd5P0/Tdpt8pFXscxy8BHNYeGZFBqnoVRdHgrtCBEufs3E5HIcpC749Go4ttv1sodoZOISoTOlDgNIahU6hE5KRo6MCWO6hJknwOYFB3UEQWDNI0fVXmDzaexnCXEQWsdOjAhtgZOgWsUujA5tOYSfWxENmR3R2tFDoQwFN8iYq4axlAUbdi1+/+pnUOSGTBtG7oAGd2Cpxeb68bmDjWuthnJg5MVFeR9S5lcGanUF2aDB1g7BQgVb0SkUOToQOMnQJTdr1LGbdjF5mZfhOiImyGDnBmp0DYDh1YE7v89NfPwec0kkMuQgc2z+wDAJc235gIcBc6sCH2NE3fiQi335FVLkMHgHubfjCfz78/ODi4EpFPXAyEukdVPxuPx29cvd/G2AFgsVh82+v19gF85Gg81B2D8Xj8tcs3LHI1ZgCev5NZldek17E19vz8XVVvPQKYqAIvoQNbTmNy8/n8+0ePHv0HfGYM1eMtdKBg7AAwn88vDg4Odvk0MKrIa+hAhQebxnH8DwZPJXkPHaiwXEBE+jx/pxKCCB2oEHu27JLn7lREMKEDJc7ZVy0Wi7e84URbBBU6UDF24PqGEz+w0gbBhQ5U/OaNVXEcfyMifQNjoXYIMnTAwHr2bMHYZf2hUAsEGzpgIHbeYaVM0KEDBk5jcnEcPxFu6euq4EMHanxAvWmxWLzt9XqX4GXJrmlE6IDB2AEuKeigxoQOGI4dABaLxWsG3wmNCh2w9HQBETnJntFH7dS40AGDH1BvOjo6ehBF0bmI3Lf1HuRFI0MHLD435uzs7G0URVw01i6NDR2wOLPnhsPh3nK5nHGGb7xGhw5Y+IB6E3c5tULjQwccxA5cX5LkNfjGakXogKPYgR+uwXNZcENkDzB6nKbp332PxRRnsQNcFtwUrp/U5YrT2AHedApdW0MHPD2ymjedwtTm0AEHlx43SZJkR1VnnOHDYPrLukLkLXaAwYeiC6EDnmMHGLxvXQkdCOBrZtI0fRdF0YDn8F5MuxI6EMDMnuMM75aqTkx8RXqTBBM7cB08gHMAu56H0mpdDB0I4DRmFTdv29fV0IHAYgeA0Wh0waXB1jzrauhAYKcxq4bD4Z6qTsFTGlNas6CrqmBjB3740HrJtfDVqeqVqg5OT09f+x6Lb8GdxqzKLkvylKai/PY/Q78W9Mye426nSi7fv3/fPzs7e+t7IKEIembPjUajCxHZ5Y2nYrK7oocM/ccaMbPneONpuy7d/i+rETN7LrsO3+cMv56qThj6Zo2a2XOc4W/r8s2ioho1s+dWZviJ77EEYsDQt2vkzL4qjuOXIjLwPQ4fVPVKRE66frOoKOd7UE3r6p7W/Bp6m3b/29b42IHr4Hu9HgD0PQ/Flcsoij5p615RW1oROwDM5/M3XXgQU3Zp8XGapryGXlIjP6BukqbpKxHZb/Hygk7tLDKt8R9Q12nj8gJeWqyvVTN7bmVNfCtuPjF0M1o5s+eym0/TJn8psaqejMfjr3yPow1aHXuuidfieQ3dvNZcjblLdmnyPoDHvsdShKqeR1F0yGvoZnViZs8lSfIUwMT3OO7CVYv2tPID6ib5pUkAl77Hsg5XLdrVqZk9F+KqSV5xsa9TM3suXzUJYOp7LBmuWnSgkzP7qiRJPgfwzMd7c+e/W52PHQDiOH6C61vxzu64tv3B/yFi7JnsG7mnLs7jVfV8uVxyQ7RjnTxnX+fs7Oyti/P4/NIiQ3ePM/sats7jecXFL8a+QRzHT0RkAkPPmuQaF/8Y+x1MLCTL1tYfjsfjN+ZGRlUw9gKSJPkSwEmFP70UkUNecQkDYy/o+Pj4UxGZFL08yTUu4eHVmIJOT09fZ8+bnG37Xa5xCRNn9gr0u6+fAjrAunN51Zn87LOP3Y+KiIiIiIiIiIiIiIjInv8B/nU3wrJp7vIAAAAASUVORK5CYII=';
    }
}

function getOrCreateInlineElement(docIdx) {
    if (docIdx < 0 || docIdx >= quill.getLength()) {
        log('error, ' + docIdx + ' is outside doc range ' + quill.getLength());
        return null;
    }
    let leafNode = quill.getLeaf(docIdx)[0].domNode;
    let leafElementNode = leafNode.nodeType == 3 ? leafNode.parentElement : leafNode;
    if (isInlineElement(leafElementNode)) {
        return leafElementNode;
    }
    //when content's parent element is block level then it has default formatting
    let inlineNode = document.createElement('span');
    //
    inlineNode.innerText = leafElementNode.innerText;
    leafElementNode.innerText = '';
    leafNode.inser
}

var InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
var BlockTags = ['p','ol','ul','li','div','table','colgroup','col','tbody','tr','td','iframe']

function isBlockElement(elm) {
    if (elm == null) {
        return false;
    }
    let tn = elm.tagName.toLowerCase();
    return tn == 'p' || tn == 'ol' || tn == 'ul' || tn == 'li';
}

function isInlineElement(elm) {
    if (elm == null) {
        return false;
    }
    let tn = elm.tagName.toLowerCase();
    return tn == 'span' || tn == 'a' || tn == 'em' ||
            tn == 'strong' || tn == 'u' || tn == 's' ||
            tn == 'sub' || tn == 'sup' || tn == 'img';
}

function getEditorIndexFromPoint(p,docIdxTemplateLookup) {
    let closestIdx = -1;
    let closestDist = Number.MAX_SAFE_INTEGER;
    if (!p) {
        return closestIdx;
    }

    let editorRect = document.getElementById('editor').getBoundingClientRect();
    let erect = { x: 0, y: 0, w: editorRect.width, h: editorRect.height };

    let ex = p.x - editorRect.left; //x position within the element.
    let ey = p.y - editorRect.top;  //y position within the element.
    let ep = { x: ex, y: ey };
    //log('editor pos: ' + ep.x + ' '+ep.y);
    if (!isPointInRect(erect, ep)) {
        return closestIdx;
    }

    for (var i = 0; i < quill.getLength(); i++) {
        let irect = quill.getBounds(i, 1);
        let ix = irect.left;
        let iy = irect.top + (irect.height / 2);
        let ip = { x: ix, y: iy };
        let idist = distSqr(ip, ep);
        if (idist < closestDist) {
            closestDist = idist;
            closestIdx = i;
        }
    }

    return closestIdx;
}

function setCaret(line, col) {
    var ele = document.getElementById("editor").childNodes[0];
    var rng = document.createRange();
    var sel = window.getSelection();
    rng.setStart(ele.childNodes[line], col);
    rng.collapse(true);
    sel.removeAllRanges();
    sel.addRange(rng);
    ele.focus();
}
