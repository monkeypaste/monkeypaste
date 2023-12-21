from ultralytics import YOLO

# Load a model
model = YOLO('yolov8n.pt')

# export the model to ONNX format
model.export(format='onnx')