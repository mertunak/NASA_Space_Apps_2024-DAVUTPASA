import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import cv2
import time
import os
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from typing import List, Union
import threading

# Define a Pydantic model for the response
class LandmarksResponse(BaseModel):
    landmarks: Union[List[List[float]], int] = Field(
        ..., 
        description="List of 33 landmarks with x, y, z coordinates or -1 if detection failed"
    )

app = FastAPI(
    title="Pose Landmarker API",
    description="API to get pose landmarks from webcam frames.",
    version="1.0.0"
)

# Global variables to store the latest frame
latest_frame = None
frame_lock = threading.Lock()

# Use an absolute path to the model file
model_path = "pose_landmarker_lite.task"

# Assign necessary classes for easier access
BaseOptions = mp.tasks.BaseOptions
PoseLandmarker = mp.tasks.vision.PoseLandmarker
PoseLandmarkerOptions = mp.tasks.vision.PoseLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode
ImageFormat = mp.ImageFormat

print("Absolute model path:", os.path.abspath(model_path))
if not os.path.exists(model_path):
    raise FileNotFoundError(f"Model file not found at {os.path.abspath(model_path)}")

# Initialize the PoseLandmarker
pose_landmarker = PoseLandmarker.create_from_options(
    PoseLandmarkerOptions(
        base_options=BaseOptions(model_asset_path=model_path),
        running_mode=VisionRunningMode.IMAGE
    )
)

# Initialize OpenCV VideoCapture to access the webcam
cap = cv2.VideoCapture(0)
if not cap.isOpened():
    raise IOError("Cannot open webcam")

def capture_frames():
    global latest_frame
    while True:
        success, frame = cap.read()
        if not success:
            print("Failed to read frame from webcam.")
            time.sleep(0.1)
            continue

        # Acquire lock to update the latest frame
        with frame_lock:
            latest_frame = frame.copy()

def start_frame_capture():
    thread = threading.Thread(target=capture_frames, daemon=True)
    thread.start()

@app.on_event("startup")
def on_startup():
    print("Starting frame capture thread...")
    start_frame_capture()

@app.on_event("shutdown")
def on_shutdown():
    print("Releasing resources...")
    pose_landmarker.close()
    cap.release()

@app.get("/get_landmarks", response_model=LandmarksResponse)
def get_landmarks():
    with frame_lock:
        if latest_frame is not None:
            try:
                # Convert the OpenCV BGR image to RGB as MediaPipe expects RGB
                rgb_frame = cv2.cvtColor(latest_frame, cv2.COLOR_BGR2RGB)

                # Create a MediaPipe Image object from the RGB frame
                mp_image = mp.Image(image_format=ImageFormat.SRGB, data=rgb_frame)

                # Process the image synchronously
                result = pose_landmarker.detect(mp_image)

                # Check if results and landmarks are valid
                if result and hasattr(result, 'landmarks') and result.landmarks:
                    # Extract landmarks as a 33x3 list
                    landmarks = []
                    for landmark in result.landmarks:
                        landmarks.append([landmark.x, landmark.y, landmark.z])
                    return LandmarksResponse(landmarks=landmarks)
                else:
                    print("No landmarks detected.")
                    return LandmarksResponse(landmarks=-1)
            except Exception as e:
                print(f"Pose detection failed: {e}")
                return LandmarksResponse(landmarks=-1)
        else:
            print("No frame available for processing.")
            return LandmarksResponse(landmarks=-1)

