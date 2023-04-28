import sys
import base64
import json
import jsonpickle
import random

class Annotation:
	def __init__(self):
		self.minScore = 0
		self.maxScore = 1
		self.score = 1
		self.label = ""
		self.box = None
		self.range = None
		self.children = None
		
class TextAnnotation(Annotation):
	def __init__(self):
		self.rangeStart = 0
		self.rangeLength = 0

class ImageAnnotation(Annotation):
	def __init__(self):
		self.x = 0
		self.y = 0
		self.width = 0
		self.height = 0
	
		
class DataObject(dict):
	def __init__(self,format,data):
		self.set(format,data)		
		
	def set(self,format,data):
		self[format] = data
		
	def get(self,format):
		return self[format]	
		
	
class RequestItem:
	def __init__(self):
		self.paramId = None
		self.value = ""
	def __init__(self, paramId, value):
		self.paramId = paramId
		self.value = value
		
class Request:
	def __init__(self):
		self.items = []
	def __init__(self, base64req):
		req_obj = json.loads(base64.b64decode(f"{base64req}{'=' * (4 - len(base64req) % 4)}"))
		self.items = []
		for kvp in req_obj["items"]:
			item = RequestItem(kvp["paramId"],kvp["value"])
			self.items.append(item)			

class Response:
	def __init__(self):
		self.errorMessage = ""
		self.retryMessage = ""
		self.otherMessage = ""
		self.userNotifications = []
		self.newContentItem = None
		self.annotations = [];
		self.dataObject = None
		
cur_req = None

def getRequest():
	global cur_req
	if cur_req == None:
		cur_req = Request(sys.argv[1])
	return cur_req
	
def getParam(paramId):
	req = getRequest()
	for kvp in req.items:
		if kvp.paramId == str(paramId):
			return kvp.value
	return ""
	
def setResponse(resp):
	#print(json.dumps(resp.toJson(), indent=4))
	resp_json = json.dumps(json.loads(jsonpickle.encode(resp, keys=True))).encode();#"utf-8")
	print(base64.b64encode(resp_json).decode())
	cur_req = None
	#print(resp_json)
	#print(base64.b64encode())
	return
	
	
