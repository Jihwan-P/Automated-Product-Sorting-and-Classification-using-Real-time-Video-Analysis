import sqlite3
import os
# [수정] hashlib 대신 werkzeug 라이브러리 사용
from werkzeug.security import generate_password_hash 

# 데이터베이스 파일 이름
DATABASE_FILE = 'mydatabase.db'

# [수정] 암호화 방식을 werkzeug로 변경
def hash_password(password):
    """비밀번호를 Werkzeug 보안 방식(Salt 포함)으로 암호화"""
    return generate_password_hash(password)

def setup_database():
    conn = None
    try:
        # [중요] 기존 DB 파일 삭제 (새로운 데이터 적용을 위해 초기화)
        if os.path.exists(DATABASE_FILE):
            try:
                os.remove(DATABASE_FILE)
                print(f"⚠️ 기존 '{DATABASE_FILE}' 파일을 삭제하고 새로 생성합니다.")
            except PermissionError:
                print(f"❌ 오류: '{DATABASE_FILE}' 파일이 사용 중입니다. 프로그램을 종료하고 다시 실행해주세요.")
                return

        conn = sqlite3.connect(DATABASE_FILE)
        cursor = conn.cursor()
        print(f"✅ '{DATABASE_FILE}' 데이터베이스 연결 성공.")

        # ---------------------------------------------------------
        # [1] users 테이블 생성
        # ---------------------------------------------------------
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS users (
            id TEXT PRIMARY KEY,
            password TEXT NOT NULL,
            name TEXT NOT NULL,
            nickname TEXT,
            role TEXT DEFAULT 'STAFF',
            email TEXT,
            phone TEXT,
            birthdate TEXT,
            profile_image TEXT
        )
        ''')
        print("✅ 'users' 테이블 준비 완료.")

        # ---------------------------------------------------------
        # [2] products 테이블 생성
        # ---------------------------------------------------------
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS products (
            item_code TEXT PRIMARY KEY,
            product_name TEXT NOT NULL,
            brand TEXT NOT NULL,
            category TEXT NOT NULL,
            color TEXT NOT NULL,
            size TEXT NOT NULL,
            stock INTEGER NOT NULL DEFAULT 0
        )
        ''')
        print("✅ 'products' 테이블 준비 완료.")

        # ---------------------------------------------------------
        # [3] orders 테이블 생성
        # ---------------------------------------------------------
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS orders (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            company TEXT NOT NULL,
            item_name TEXT NOT NULL,
            quantity INTEGER NOT NULL,
            price INTEGER DEFAULT 0,
            order_date TEXT,
            due_date TEXT,
            contact TEXT,
            note TEXT,
            status TEXT DEFAULT '대기중'
        )
        ''')
        print("✅ 'orders' 테이블 준비 완료.")

        # ---------------------------------------------------------
        # [4] slots 테이블 생성
        # ---------------------------------------------------------
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS slots (
            slot_id TEXT PRIMARY KEY,
            x INTEGER,
            y INTEGER,
            w INTEGER,
            h INTEGER,
            is_active BOOLEAN DEFAULT 0
        )
        ''')
        print("✅ 'slots' 테이블 준비 완료.")

        # ---------------------------------------------------------
        # [5] 초기 데이터 삽입
        # ---------------------------------------------------------
        
        # 1. 관리자 계정 (비번: 1234)
        # [중요] 여기서 바뀐 함수(generate_password_hash)가 실행됩니다.
        admin_pw = hash_password("1234")
        cursor.execute("INSERT INTO users (id, password, name, nickname, role) VALUES (?, ?, ?, ?, ?)",
                       ("admin", admin_pw, "관리자", "Admin", "ADMIN"))

        # 2. 제품 데이터
        products_data = [
            # [빈폴] BeanPole -> BP
            ('BP-01-01-01', '빈폴 베이직 티셔츠', 'BeanPole', 'TOP', 'Black', 'XS', 10),
            ('BP-01-02-02', '빈폴 로고 피케 셔츠', 'BeanPole', 'TOP', 'White', 'S', 15),
            ('BP-02-03-04', '빈폴 컴포트 치노 팬츠', 'BeanPole', 'BOTTOM', 'Gray', 'L', 8),

            # [엄브로] Umbro -> UB
            ('UB-01-04-05', '엄브로 팀 트레이닝 탑', 'Umbro', 'TOP', 'Red', 'XL', 12),
            ('UB-02-05-03', '엄브로 우븐 조거 팬츠', 'Umbro', 'BOTTOM', 'Blue', 'M', 20),
            ('UB-03-01-03', '엄브로 벤치 롱 코트', 'Umbro', 'OUTER', 'Black', 'M', 7),
            ('UB-03-02-06', '엄브로 아노락 자켓', 'Umbro', 'OUTER', 'White', 'Free', 5),

            # [퓨마] Puma -> PM
            ('PM-01-03-02', '퓨마 T7 트랙 재킷', 'Puma', 'TOP', 'Gray', 'S', 18),
            ('PM-02-01-05', '퓨마 아이코닉 T7 팬츠', 'Puma', 'BOTTOM', 'Black', 'XL', 1),

            # [데상트] DESCENTE -> DS
            ('DS-03-01-04', '데상트 스위스 스키팀 재킷', 'DESCENTE', 'OUTER', 'Black', 'L', 5)
        ]

        cursor.executemany("""
        INSERT INTO products (item_code, product_name, brand, category, color, size, stock) 
        VALUES (?, ?, ?, ?, ?, ?, ?)
        """, products_data)
        print(f"✅ 제품 데이터 {len(products_data)}건 추가 완료.")

        # 3. 주문 데이터
        orders_data = [
            ('빈폴 본사', '빈폴 로고 피케 셔츠', 50, 45000, '2023-11-25', '2023-11-30', '010-1111-2222', '빠른 배송 요망', '대기중'),
            ('엄브로 스포츠', '엄브로 우븐 조거 팬츠', 20, 39000, '2023-11-24', '2023-12-01', '010-3333-4444', '오후 배송', '승인됨'),
            ('데상트 코리아', '스키팀 티셔츠', 10, 15000, '2023-11-20', '2023-11-28', '02-123-4567', '사이즈 혼합', '취소')
        ]

        cursor.executemany("""
        INSERT INTO orders (company, item_name, quantity, price, order_date, due_date, contact, note, status)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, orders_data)
        print(f"✅ 주문 데이터 {len(orders_data)}건 추가 완료.")

        # 4. 슬롯 데이터
        slots_data = [
            ("A-1", 10, 10, 100, 50, 1),
            ("A-2", 120, 10, 100, 50, 1),
            ("B-1", 10, 80, 100, 50, 0)
        ]
        cursor.executemany("INSERT INTO slots (slot_id, x, y, w, h, is_active) VALUES (?, ?, ?, ?, ?, ?)", slots_data)
        print(f"✅ 슬롯 데이터 {len(slots_data)}건 추가 완료.")

        conn.commit()
        print("🎉 데이터베이스 설정이 모두 완료되었습니다!")

    except Exception as e:
        print(f"❌ 오류 발생: {e}")

    finally:
        if conn:
            conn.close()
            print("🔌 연결 종료.")

if __name__ == '__main__':
    setup_database()