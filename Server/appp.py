import sqlite3
import os
import requests  # [필수] 네이버 API 및 외부 통신용
from flask import Flask, jsonify, request, send_from_directory, render_template
from werkzeug.security import generate_password_hash, check_password_hash
from werkzeug.utils import secure_filename
from mock_factory import factory_bp

app = Flask(__name__)

DATABASE_FILE = 'mydatabase.db'

# =========================================================
# ▼ [설정] 네이버 API 키 & 업로드 폴더
# =========================================================
NAVER_CLIENT_ID = "ZuNmfh2elsZgAtX166p3"
NAVER_CLIENT_SECRET = "TczU_CH5Jy"

UPLOAD_FOLDER = 'uploads'
os.makedirs(UPLOAD_FOLDER, exist_ok=True)
app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER

def get_db_connection():
    conn = sqlite3.connect(DATABASE_FILE)
    conn.row_factory = sqlite3.Row
    return conn

# =========================================================
# ▼ [초기화] DB 테이블 및 기초 데이터 생성
# =========================================================
def init_tables():
    conn = get_db_connection()
    cursor = conn.cursor()
    
    # 1. users
    cursor.execute('''CREATE TABLE IF NOT EXISTS users (id TEXT PRIMARY KEY, password TEXT NOT NULL, name TEXT, nickname TEXT, role TEXT, email TEXT, phone TEXT, birthdate TEXT, profile_image TEXT)''')
    # 2. products
    cursor.execute('''CREATE TABLE IF NOT EXISTS products (item_code TEXT PRIMARY KEY, product_name TEXT, brand TEXT, category TEXT, color TEXT, size TEXT, stock INTEGER)''')
    # 3. orders
    cursor.execute('''CREATE TABLE IF NOT EXISTS orders (id INTEGER PRIMARY KEY AUTOINCREMENT, company TEXT, item_name TEXT, quantity INTEGER, order_date TEXT, due_date TEXT, status TEXT DEFAULT '대기중', contact TEXT, price INTEGER, note TEXT)''')
    # 4. slots
    cursor.execute('''CREATE TABLE IF NOT EXISTS slots (slot_id TEXT PRIMARY KEY, x INTEGER, y INTEGER, w INTEGER, h INTEGER, is_active INTEGER)''')
    
    conn.commit()
    conn.close()

def insert_initial_products():
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT count(*) FROM products")
    if cursor.fetchone()[0] == 0:
        print("📦 초기 제품 데이터 추가 중...")
        products_data = [
            ('BP-01-01-01', '빈폴 베이직 티셔츠', 'BeanPole', 'TOP', 'Black', 'XS', 10),
            ('BP-01-02-02', '빈폴 로고 피케 셔츠', 'BeanPole', 'TOP', 'White', 'S', 15),
            ('BP-02-03-04', '빈폴 컴포트 치노 팬츠', 'BeanPole', 'BOTTOM', 'Gray', 'L', 8),
            ('UB-01-04-05', '엄브로 팀 트레이닝 탑', 'Umbro', 'TOP', 'Red', 'XL', 12),
            ('UB-02-05-03', '엄브로 우븐 조거 팬츠', 'Umbro', 'BOTTOM', 'Blue', 'M', 20),
            ('UB-03-01-03', '엄브로 벤치 롱 코트', 'Umbro', 'OUTER', 'Black', 'M', 7),
            ('UB-03-02-06', '엄브로 아노락 자켓', 'Umbro', 'OUTER', 'White', 'Free', 5),
            ('PM-01-03-02', '퓨마 T7 트랙 재킷', 'Puma', 'TOP', 'Gray', 'S', 18),
            ('PM-02-01-05', '퓨마 아이코닉 T7 팬츠', 'Puma', 'BOTTOM', 'Black', 'XL', 1),
            ('DS-03-01-04', '데상트 스위스 스키팀 재킷', 'DESCENTE', 'OUTER', 'Black', 'L', 5)
        ]
        cursor.executemany("INSERT INTO products (item_code, product_name, brand, category, color, size, stock) VALUES (?, ?, ?, ?, ?, ?, ?)", products_data)
        conn.commit()
    conn.close()

init_tables()
insert_initial_products()
app.register_blueprint(factory_bp, url_prefix='/factory')

# =========================================================
# ▼ [웹페이지] HTML 렌더링 (View)
# =========================================================
@app.route('/')
def home():
    return render_template('factory_index.html')

@app.route('/login')
def login_page():
    return render_template('login.html')

@app.route('/register')
def register_page():
    return render_template('register.html')

@app.route('/cart')
def cart_page():
    return render_template('cart.html')

@app.route('/order_history')
def order_history_page():
    return render_template('order_history.html')

@app.route('/uploads/<filename>')
def uploaded_file(filename):
    return send_from_directory(app.config['UPLOAD_FOLDER'], filename)

# =========================================================
# ▼ [기능] 네이버 API (검색, 캡차)
# =========================================================
@app.route('/api/naver/search', methods=['GET'])
def search_naver_shopping():
    query = request.args.get('query')
    if not query: return jsonify([])
    
    url = "https://openapi.naver.com/v1/search/shop.json"
    headers = { "X-Naver-Client-Id": NAVER_CLIENT_ID, "X-Naver-Client-Secret": NAVER_CLIENT_SECRET }
    params = { "query": query, "display": 20, "start": 1, "sort": "sim" }
    
    try:
        res = requests.get(url, headers=headers, params=params)
        return jsonify(res.json()['items']) if res.status_code == 200 else jsonify([])
    except:
        return jsonify([])

@app.route('/api/captcha/key', methods=['GET'])
def get_captcha_key():
    try:
        url = "https://openapi.naver.com/v1/captcha/nkey?code=0"
        headers = { "X-Naver-Client-Id": NAVER_CLIENT_ID, "X-Naver-Client-Secret": NAVER_CLIENT_SECRET }
        res = requests.get(url, headers=headers).json()
        key = res.get('key')
        image_url = f"https://openapi.naver.com/v1/captcha/ncaptcha.bin?key={key}"
        return jsonify({"key": key, "image_url": image_url})
    except:
        return jsonify({"message": "캡차 발급 실패"}), 500

# =========================================================
# ▼ [기능] 인증 (회원가입, 로그인, 정보수정)
# =========================================================
@app.route('/api/check_id', methods=['GET'])
def check_id():
    user_id = request.args.get('id')
    conn = get_db_connection()
    user = conn.execute("SELECT id FROM users WHERE id = ?", (user_id,)).fetchone()
    conn.close()
    return jsonify({"message": "이미 존재하는 아이디입니다."}) if user else jsonify({"message": "사용 가능한 아이디입니다."}), 200

@app.route('/api/register', methods=['POST'])
def register():
    data = request.get_json()
    
    # 1. 캡차 검증 (웹 요청인 경우만)
    if 'captcha_key' in data and data['captcha_key']:
        c_key = data.get('captcha_key')
        c_val = data.get('captcha_val')
        verify_url = f"https://openapi.naver.com/v1/captcha/nkey?code=1&key={c_key}&value={c_val}"
        headers = { "X-Naver-Client-Id": NAVER_CLIENT_ID, "X-Naver-Client-Secret": NAVER_CLIENT_SECRET }
        verify_res = requests.get(verify_url, headers=headers).json()
        if not verify_res.get('result'):
            return jsonify({"message": "보안 문자가 틀렸습니다."}), 400

    # 2. DB 저장 (Werkzeug 암호화)
    user_id = data.get('id')
    user_pw = data.get('pw')
    if not user_id or not user_pw: return jsonify({"message": "정보 누락"}), 400

    hashed_pw = generate_password_hash(user_pw)

    try:
        conn = get_db_connection()
        conn.execute("INSERT INTO users (id, password, name, nickname, role, email, phone, birthdate) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                     (user_id, hashed_pw, data.get('name',''), data.get('nickname',''), data.get('role','STAFF'), data.get('email',''), data.get('phone',''), data.get('birthdate','')))
        conn.commit()
        conn.close()
        return jsonify({"message": "회원가입 성공"}), 201
    except sqlite3.IntegrityError:
        return jsonify({"message": "이미 존재하는 아이디입니다."}), 409
    except Exception as e:
        return jsonify({"message": f"오류: {e}"}), 500

@app.route('/api/login', methods=['POST'])
def login():
    data = request.get_json()
    conn = get_db_connection()
    user = conn.execute("SELECT * FROM users WHERE id = ?", (data.get('id'),)).fetchone()
    conn.close()

    if user and check_password_hash(user['password'], data.get('pw')):
        profile_img = user['profile_image']
        full_img_url = f"http://127.0.0.1:5000/uploads/{profile_img}" if profile_img else ""
        
        user_info = {
            "name": user['name'], "nickname": user['nickname'], "role": user['role'],
            "email": user['email'], "phone": user['phone'], "birthdate": user['birthdate'],
            "profile_image": full_img_url
        }
        return jsonify({"message": "로그인 성공", "userInfo": user_info}), 200
    return jsonify({"message": "아이디 또는 비밀번호 오류"}), 401

@app.route('/api/user/update', methods=['POST'])
def update_user_info():
    try:
        data = request.get_json()
        user_id = data.get('id')
        conn = get_db_connection()
        
        sql = "UPDATE users SET name=?, nickname=?, email=?, phone=?, birthdate=? WHERE id=?"
        params = [data.get('name'), data.get('nickname'), data.get('email'), data.get('phone'), data.get('birthdate'), user_id]
        
        if data.get('new_password'):
            hashed_pw = generate_password_hash(data.get('new_password'))
            conn.execute("UPDATE users SET password=? WHERE id=?", (hashed_pw, user_id))
        
        conn.execute(sql, params)
        conn.commit()
        conn.close()
        return jsonify({"success": True, "message": "수정되었습니다."})
    except Exception as e:
        return jsonify({"success": False, "message": str(e)}), 500

@app.route('/api/user/upload_image', methods=['POST'])
def upload_image():
    try:
        file = request.files.get('file')
        user_id = request.form.get('user_id')
        if not file or not user_id: return jsonify({"success": False}), 400
        
        filename = secure_filename(file.filename)
        save_name = f"{user_id}_{filename}"
        file.save(os.path.join(app.config['UPLOAD_FOLDER'], save_name))
        
        conn = get_db_connection()
        conn.execute("UPDATE users SET profile_image=? WHERE id=?", (save_name, user_id))
        conn.commit()
        conn.close()
        return jsonify({"success": True, "url": f"http://127.0.0.1:5000/uploads/{save_name}"})
    except Exception as e:
        return jsonify({"success": False, "message": str(e)}), 500

# =========================================================
# ▼ [기능] 상품 & 주문 & 슬롯 관리 (WinForms + Web 통합)
# =========================================================

# 제품 목록 조회
@app.route('/api/products', methods=['GET'])
def get_products():
    conn = get_db_connection()
    rows = conn.execute("SELECT * FROM products").fetchall()
    conn.close()
    return jsonify([dict(row) for row in rows])

# 재고 수정 (WinForms)
@app.route('/api/product/update_stock', methods=['POST'])
def update_stock():
    try:
        data = request.get_json()
        conn = get_db_connection()
        conn.execute("UPDATE products SET stock = ? WHERE item_code = ?", (data['new_stock'], data['item_code']))
        conn.commit()
        conn.close()
        return jsonify({"success": True})
    except Exception as e: return jsonify({"success": False, "message": str(e)}), 500

# 제품 추가 (WinForms)
@app.route('/api/product/add', methods=['POST'])
def add_product():
    try:
        d = request.get_json()
        conn = get_db_connection()
        conn.execute("INSERT INTO products (item_code, product_name, brand, category, color, size, stock) VALUES (?,?,?,?,?,?,?)", 
                     (d['item_code'], d['item_code'], d['brand'], d['category'], d['color'], d['size'], d.get('stock',0)))
        conn.commit()
        conn.close()
        return jsonify({"success": True})
    except Exception as e: return jsonify({"success": False, "message": str(e)}), 500

# 제품 삭제 (WinForms)
@app.route('/api/product/delete', methods=['POST'])
def delete_product():
    try:
        conn = get_db_connection()
        conn.execute("DELETE FROM products WHERE item_code = ?", (request.get_json()['item_code'],))
        conn.commit()
        conn.close()
        return jsonify({"success": True})
    except Exception as e: return jsonify({"success": False, "message": str(e)}), 500

# 주문 목록 조회 (WinForms - 전체 조회)
@app.route('/api/orders', methods=['GET'])
def get_orders():
    conn = get_db_connection()
    rows = conn.execute("SELECT * FROM orders ORDER BY id DESC").fetchall()
    conn.close()
    return jsonify([dict(row) for row in rows])

# 내 주문 내역 조회 (Web - 로그인 사용자용)
@app.route('/api/order/my_list', methods=['GET'])
def get_my_orders():
    user_id = request.args.get('user_id')
    if not user_id: return jsonify([])
    conn = get_db_connection()
    rows = conn.execute("SELECT * FROM orders WHERE contact = ? ORDER BY id DESC", (user_id,)).fetchall()
    conn.close()
    return jsonify([dict(row) for row in rows])

# 주문 등록 (Web/WinForms 공용)
@app.route('/api/order/add', methods=['POST'])
def add_order():
    try:
        d = request.get_json()
        conn = get_db_connection()
        conn.execute("INSERT INTO orders (company, item_name, quantity, order_date, due_date, status, contact, price, note) VALUES (?,?,?,?,?,?,?,?,?)",
                     (d['company'], d['item_name'], d.get('quantity',1), d.get('order_date',''), d.get('due_date',''), '대기중', d.get('contact',''), d.get('price',0), d.get('note','')))
        conn.commit()
        conn.close()
        return jsonify({"success": True})
    except Exception as e: return jsonify({"success": False, "message": str(e)}), 500


# =========================================================
# [수정] 결제 완료 처리 (강력한 예외 처리 적용)
# =========================================================
@app.route('/api/payment/complete', methods=['POST'])
def complete_payment():
    try:
        data = request.get_json()
        print("결제 요청 데이터:", data) # (디버깅용) 터미널에 찍어봅니다.

        user_id = data.get('user_id')
        items = data.get('items')
        
        if not items: 
            return jsonify({"success": False, "message": "상품 정보가 없습니다."}), 400

        conn = get_db_connection()
        
        # 1. 연락처 조회 (없으면 아이디 사용)
        user_row = conn.execute("SELECT phone FROM users WHERE id = ?", (user_id,)).fetchone()
        user_contact = user_row['phone'] if user_row and user_row['phone'] else user_id

        for item in items:
            # 2. [핵심] 품목명 찾기 (가능한 모든 이름표를 다 확인합니다)
            # product_name이 있으면 쓰고, 없으면 name, 그것도 없으면 item_name을 찾음
            p_name = item.get('product_name') or item.get('name') or item.get('item_name') or '상품명 없음'

            # 3. [핵심] 브랜드명 찾기 (없으면 MobleStore)
            brand = item.get('brand') or 'MobleStore'

            # 4. 수량과 가격 (숫자로 변환해서 안전하게 저장)
            qty = int(item.get('quantity', 1))
            price = int(item.get('price', 0))

            conn.execute("""
                INSERT INTO orders 
                (company, item_name, quantity, price, contact, status, order_date) 
                VALUES (?, ?, ?, ?, ?, '결제완료', datetime('now', 'localtime'))
            """, (brand, p_name, qty, price, user_contact))
            
        conn.commit()
        conn.close()
        
        return jsonify({"success": True, "message": "주문이 완료되었습니다."})

    except Exception as e:
        print("결제 처리 중 에러:", e) # 터미널에서 에러 내용을 볼 수 있습니다.
        return jsonify({"success": False, "message": str(e)}), 500
    
# 주문 상태 변경 (WinForms)
@app.route('/api/order/update_status', methods=['POST'])
def update_order_status():
    d = request.get_json()
    conn = get_db_connection()
    conn.execute("UPDATE orders SET status = ? WHERE id = ?", (d.get('status'), d.get('id')))
    conn.commit()
    conn.close()
    return jsonify({"success": True})

# 슬롯 조회 (WinForms)
@app.route('/api/slots', methods=['GET'])
def get_slots():
    conn = get_db_connection()
    rows = conn.execute("SELECT * FROM slots").fetchall()
    conn.close()
    return jsonify([{"slot_id": r["slot_id"], "x": r["x"], "y": r["y"], "w": r["w"], "h": r["h"], "is_active": bool(r["is_active"])} for r in rows])

# 슬롯 저장 (WinForms)
@app.route('/api/slots/save', methods=['POST'])
def save_slot():
    try:
        d = request.get_json()
        conn = get_db_connection()
        cursor = conn.cursor()
        cnt = cursor.execute("SELECT count(*) FROM slots WHERE slot_id = ?", (d['slot_id'],)).fetchone()[0]
        active = 1 if d.get('is_active') else 0
        if cnt > 0:
            cursor.execute("UPDATE slots SET x=?, y=?, w=?, h=?, is_active=? WHERE slot_id=?", (d['x'], d['y'], d['w'], d['h'], active, d['slot_id']))
        else:
            cursor.execute("INSERT INTO slots (slot_id, x, y, w, h, is_active) VALUES (?,?,?,?,?,?)", (d['slot_id'], d['x'], d['y'], d['w'], d['h'], active))
        conn.commit()
        conn.close()
        return jsonify({'success': True})
    except Exception as e: return jsonify({'success': False, 'message': str(e)}), 500

# 슬롯 삭제 (WinForms)
@app.route('/api/slots/delete', methods=['POST'])
def delete_slot():
    try:
        conn = get_db_connection()
        conn.execute("DELETE FROM slots WHERE slot_id = ?", (request.get_json().get('slot_id'),))
        conn.commit()
        conn.close()
        return jsonify({'success': True})
    except Exception as e: return jsonify({'success': False, 'message': str(e)}), 500

# [수정됨] 주문 삭제 API
@app.route('/api/order/delete', methods=['POST'])
def delete_order():
    try:
        data = request.get_json()
        order_id = data.get('id') 

        # [핵심 수정] 파일명을 직접 쓰지 않고 공통 함수 사용!
        # 이렇게 하면 맨 위에서 설정한 'mydatabase.db'를 자동으로 찾아갑니다.
        conn = get_db_connection() 
        
        conn.execute("DELETE FROM orders WHERE id = ?", (order_id,))
        
        conn.commit()
        conn.close()

        return jsonify({'success': True, 'message': '삭제되었습니다.'})

    except Exception as e:
        return jsonify({'success': False, 'message': str(e)}), 500  

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)