import sqlite3
from flask import Blueprint, render_template, request, jsonify
import time

factory_bp = Blueprint('factory', __name__)

# --- [DB 연결 함수] ---
# 메인 서버(app.py)와 같은 DB 파일을 사용합니다.
def get_db_connection():
    conn = sqlite3.connect('mydatabase.db')
    conn.row_factory = sqlite3.Row
    return conn

# --- [가격표] HTML과 동일하게 맞춘 가격 정보 ---
# 웹에서 가격을 안 보내주니 서버가 여기서 찾아서 DB에 넣습니다.
PRODUCT_PRICES = {
    "Descente": 89000,
    "Beanpole": 129000,
    "Umbro": 55000,
    "Puma": 49000,
}

# --- 친구의 전역 변수들 (기존 로직 유지용) ---
ORDERS_DB = []
BRAND_CODES = { "Descente": 'D', "Beanpole": 'B', "Umbro": 'U', "Puma": 'P' }

def create_mock_command(orders):
    command = ""
    for item in orders:
        brand_name = item['name']
        quantity = item['quantity']
        code = BRAND_CODES.get(brand_name)
        if code:
            command += f"{code}{quantity}"
    return f"<{command}>" 

def mock_process_start(command_string):
    print("==========================================")
    print(f"✅ [MOCK] 가상 공정 시작 명령 시뮬레이션: {command_string}")
    print("==========================================")

# --- 라우트(경로) 설정 ---

@factory_bp.route('/')
def index():
    # ★주의: HTML 파일 이름이 맞는지 꼭 확인하세요!
    return render_template('factory_index.html') 

@factory_bp.route('/api/process_order', methods=['POST'])
def process_order():
    try:
        data = request.get_json()
        orders = data.get('orders', [])
        user_id = data.get('user_id', 'Guest') # HTML에서 보낸 로그인 ID 받기
        order_time = time.strftime('%Y-%m-%d %H:%M:%S')

        if not orders:
            return jsonify({"status": "error", "message": "주문 목록이 비어 있습니다."}), 400

        # 1. 친구의 메모리 리스트에 저장 (가상 공정용 - 기존 유지)
        ORDERS_DB.append({
            "user": user_id,
            "time": order_time,
            "details": orders,
            "command": create_mock_command(orders)
        })
        
        # ---------------------------------------------------------
        # ▼▼▼ [핵심] 실제 DB(mydatabase.db)에 주문 저장하기 ▼▼▼
        # ---------------------------------------------------------
        conn = get_db_connection()
        cursor = conn.cursor()
        
        for item in orders:
            name = item['name']      # 예: Descente
            qty = item['quantity']   # 예: 2
            
            # 가격표에서 가격 찾기 (없으면 0원 처리)
            price = PRODUCT_PRICES.get(name, 0)
            
            # DB에 저장! (가격과 상태, 연락처 등 필수 정보 포함)
            cursor.execute("""
                INSERT INTO orders (company, item_name, quantity, order_date, status, contact, price, note)
                VALUES (?, ?, ?, ?, '대기중', ?, ?, ?)
            """, (user_id, name, qty, order_time, '010-0000-0000', price, '웹사이트 주문')) 

        conn.commit()
        conn.close()
        # ---------------------------------------------------------

        # 가상 공정 시작 알림
        mock_process_start(ORDERS_DB[-1]["command"])

        return jsonify({
            "status": "success",
            "message": "주문이 정상적으로 접수되었습니다. (DB 저장 완료)",
        }), 200

    except Exception as e:
        print(f"🚨 주문 처리 중 오류 발생: {e}")
        return jsonify({"status": "error", "message": f"서버 처리 오류: {str(e)}"}), 500

@factory_bp.route('/api/get_orders', methods=['GET'])
def get_orders():
    latest_orders = ORDERS_DB[-5:][::-1]
    return jsonify({
        "status": "success",
        "total_count": len(ORDERS_DB),
        "latest_orders": latest_orders
    }), 200