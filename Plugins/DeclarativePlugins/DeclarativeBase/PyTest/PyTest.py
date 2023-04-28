import analyzer
import sys

data = analyzer.getParam("1")
out_data = "TEST!!! "+data
resp = analyzer.Response()
resp.dataObject = analyzer.DataObject("Text",out_data)
img_ann = analyzer.ImageAnnotation()
img_ann.label = "test img ann"
img_ann.x = 10
img_ann.y = 10
img_ann.width = 100
img_ann.height = 100
resp.annotations = [img_ann]

analyzer.setResponse(resp)
