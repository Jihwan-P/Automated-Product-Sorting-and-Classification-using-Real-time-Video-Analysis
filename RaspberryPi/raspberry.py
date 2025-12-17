#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import cv2
import time
import requests
import threading
from flask import Flask, Response

# =========================================================
# [설정 구간]
# =========================================================
CAMERA_ID = "cam4"           # ★ 각 라파에 맞게 수정 (cam1 ~ cam4)
AI_SERVER_URL = "http://192.168.0.87:5000/upload_frame" # AI 분석 서버 주소

CAM_INDEX = 0
TARGET_FPS = 30

# =========================================================
# Flask 웹 서버 설정
# =========================================================
app = Flask(__name__)
frame_buffer = None # 최신 프레임 저장용
buffer_lock = threading.Lock()

# AI 서버 전송용 쓰레드 함수
def send_to_ai_server(image_data, cam_id):
    try:
        files = {"frame": ("frame.jpg", image_data, "image/jpeg")}
        data  = {"camera_id": cam_id}
        requests.post(AI_SERVER_URL, files=files, data=data, timeout=0.5)
    except Exception:
        pass

# 카메라 촬영 루프 (백그라운드 쓰레드)
def camera_loop():
    global frame_buffer
    cap = cv2.VideoCapture(CAM_INDEX)
    cap.set(cv2.CAP_PROP_FPS, TARGET_FPS)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

    frame_counter = 0

    while True:
        ret, frame = cap.read()
        if not ret:
            time.sleep(0.1)
            continue

        frame_counter += 1

        # JPEG 인코딩
        encode_ok, encoded = cv2.imencode(".jpg", frame, [int(cv2.IMWRITE_JPEG_QUALITY), 80])
        if not encode_ok:
            continue

        data = encoded.tobytes()

        # 1. 최신 프레임 업데이트 (웹 스트리밍용)
        with buffer_lock:
            frame_buffer = data

        # 2. AI 서버 전송 (10fps - 3프레임마다 1번)
        if frame_counter % 3 == 0:
            t = threading.Thread(target=send_to_ai_server, args=(data, CAMERA_ID))
            t.daemon = True
            t.start()

        # CPU 과부하 방지 (약간의 여유)
        time.sleep(0.005)

# 웹 스트리밍 생성기
def generate_mjpeg():
    while True:
        with buffer_lock:
            if frame_buffer is None:
                continue
            data = frame_buffer

        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + data + b'\r\n')

        # 스트리밍 전송 속도 조절 (약 30fps)
        time.sleep(1.0 / TARGET_FPS)

@app.route('/stream.mjpg')
def stream():
    return Response(generate_mjpeg(), mimetype='multipart/x-mixed-replace; boundary=frame')

if __name__ == "__main__":
    # 카메라 촬영을 별도 쓰레드로 시작
    t = threading.Thread(target=camera_loop)
    t.daemon = True
    t.start()

    # 웹 서버 시작 (포트 8000 사용)
    # WinForm WebView2는 http://라파IP:8000/stream.mjpg 로 접속하면 됨
    app.run(host='0.0.0.0', port=8000, threaded=True, debug=False)