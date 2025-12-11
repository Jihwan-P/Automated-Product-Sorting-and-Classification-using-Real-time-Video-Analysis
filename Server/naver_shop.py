import requests
from flask import Blueprint, request, jsonify
import urllib.parse

# Blueprint 생성
shop_bp = Blueprint('shop', __name__)

# ★★★ 발급받은 네이버 API 키를 여기에 입력하세요 ★★★
NAVER_CLIENT_ID = "여기에_Client_ID_입력"
NAVER_CLIENT_SECRET = "여기에_Client_Secret_입력"

# 우리가 허용할 브랜드 목록
TARGET_BRANDS = ["빈폴", "엄브로", "데상트", "퓨마"]

@shop_bp.route('/api/shop/search', methods=['GET'])
def search_shop():
    # 1. 프론트엔드에서 검색어와 페이지 번호를 받음
    user_query = request.args.get('query', '') # 예: "후드티"
    category = request.args.get('category', '') # 예: "상의"
    page = int(request.args.get('page', 1))    # 현재 페이지 (기본 1)
    display = 20                               # 한 페이지당 20개

    # 2. 검색어 조합 (브랜드 한정 + 카테고리 + 검색어)
    # 네이버 검색 연산자 활용: (빈폴 OR 엄브로 ...) AND (상의 후드티)
    brands_query = " OR ".join(TARGET_BRANDS)
    final_query = f"({brands_query}) {category} {user_query}".strip()
    
    # URL 인코딩
    encText = urllib.parse.quote(final_query)
    
    # 시작 위치 계산 (1페이지=1, 2페이지=21, 3페이지=41 ...)
    start = (page - 1) * display + 1

    # 3. 네이버 API 요청
    url = f"https://openapi.naver.com/v1/search/shop.json?query={encText}&display={display}&start={start}&sort=sim"
    
    headers = {
        "X-Naver-Client-Id": NAVER_CLIENT_ID,
        "X-Naver-Client-Secret": NAVER_CLIENT_SECRET
    }

    try:
        response = requests.get(url, headers=headers)
        res_code = response.status_code

        if res_code == 200:
            return jsonify(response.json())
        else:
            print("Error Code:", res_code)
            return jsonify({"error": "Naver API Error"}), res_code

    except Exception as e:
        print("Server Error:", str(e))
        return jsonify({"error": str(e)}), 500

# app.py 상단에 import 추가
from naver_shop import shop_bp

# ... (중간 생략) ...

# 기존 factory_bp 등록 밑에 추가
app.register_blueprint(shop_bp)