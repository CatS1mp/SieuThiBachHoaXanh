import sys
import json
import face_recognition
import numpy as np

def encode_image(image_path):
    try:
        image = face_recognition.load_image_file(image_path)
        encodings = face_recognition.face_encodings(image)
        if len(encodings) == 0:
            return {"success": False, "message": "Không tìm thấy khuôn mặt trong ảnh"}
        return {"success": True, "embedding": encodings[0].tolist(), "message": ""}
    except Exception as e:
        return {"success": False, "message": str(e)}

def authenticate_image(image_path, known_embedding_json):
    try:
        # Tải ảnh và tạo encoding mới
        image = face_recognition.load_image_file(image_path)
        new_encodings = face_recognition.face_encodings(image)
        if len(new_encodings) == 0:
            return {"success": False, "message": "Không tìm thấy khuôn mặt trong ảnh"}

        # Chuyển embedding đã lưu từ JSON sang numpy array
        known_embedding = np.array(json.loads(known_embedding_json))

        # So sánh khuôn mặt
        result = face_recognition.compare_faces([known_embedding], new_encodings[0])[0]
        return {"success": True, "match": bool(result), "message": ""}
    except Exception as e:
        return {"success": False, "message": str(e)}

if __name__ == "__main__":
    command = sys.argv[1]
    if command == "encode_image":
        image_path = sys.argv[2]
        result = encode_image(image_path)
        print(json.dumps(result))
    elif command == "authenticate_image":
        image_path = sys.argv[2]
        known_embedding_json = sys.argv[3]
        result = authenticate_image(image_path, known_embedding_json)
        print(json.dumps(result))