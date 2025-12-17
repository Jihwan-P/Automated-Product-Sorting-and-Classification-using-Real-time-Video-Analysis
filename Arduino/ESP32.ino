#include <WiFi.h>
#include <HTTPClient.h>

// ====================== WiFi 설정 ======================
const char* WIFI_SSID     = "moble_main_2.4G";
const char* WIFI_PASSWORD = "moble2025";

// ====================== Flask 서버 주소 ======================
// PC에서 ipconfig로 확인한 IPv4 주소
const char* SERVER_HOST = "192.168.0.87";
const int   SERVER_PORT = 5000;

String getStateUrl() {
  return String("http://") + SERVER_HOST + ":" + SERVER_PORT + "/api/get_state";
}

// ====================== MEGA와 연결할 UART 핀 ======================
// ESP32-C3 보드 핀맵에 따라 수정 가능
#define MEGA_RX_PIN 3   // ESP32가 수신 (MEGA TX1=18 연결)
#define MEGA_TX_PIN 4   // ESP32가 송신 (MEGA RX1=19 연결)

HardwareSerial MegaSerial(1);

// ====================== WiFi 연결 ======================
void connectWiFi() {
  Serial.println("\n[WiFi] 초기화 중...");
  WiFi.disconnect(true);
  delay(100);

  WiFi.mode(WIFI_STA);
  delay(100);

  Serial.print("[WiFi] Connecting to ");
  Serial.println(WIFI_SSID);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  unsigned long start = millis();
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(300);

    if (millis() - start > 15000) {
      Serial.println("\n[WiFi] 연결 타임아웃! 다시 시도");
      WiFi.disconnect(true);
      delay(200);
      WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
      start = millis();
    }
  }

  Serial.println();
  Serial.print("[WiFi] 연결 성공! IP: ");
  Serial.println(WiFi.localIP());
}

// ====================== Flask에서 state 가져오기 ======================
int fetchStateFromServer() {
  if (WiFi.status() != WL_CONNECTED) {
    connectWiFi();
  }

  HTTPClient http;
  String url = getStateUrl();

  http.begin(url);
  int httpCode = http.GET();

  if (httpCode == 200) {
    String payload = http.getString();   // 예: "32"
    int state = payload.toInt();

    Serial.print("[ESP32] 서버 응답: ");
    Serial.print(payload);
    Serial.print("  -> state=");
    Serial.println(state);

    http.end();
    return state;
  } else {
    Serial.print("[ESP32] HTTP 에러 코드: ");
    Serial.println(httpCode);
    http.end();
    return 0;   // 에러 시 명령 없음
  }
}

// ====================== 기본 함수 ======================
void setup() {
  Serial.begin(115200);  // USB 디버그용
  delay(500);

  // MEGA와 UART1로 연결
  MegaSerial.begin(115200, SERIAL_8N1, MEGA_RX_PIN, MEGA_TX_PIN);

  connectWiFi();

  Serial.println("[ESP32] 준비 완료. Flask에서 state를 읽어 MEGA로 전송합니다.");
}

void loop() {
  int state = fetchStateFromServer();   // 0 또는 1~32 등

  // 0이면 명령 없음, 1 이상이면 유효한 명령
  if (state > 0) {
    Serial.print("[ESP32] MEGA로 전송: ");
    Serial.println(state);

    // 🔥 "CMD:숫자\n" 형식으로 전송
    MegaSerial.print("CMD:");
    MegaSerial.print(state);
    MegaSerial.print('\n');
  }

  delay(200);   // 0.2초마다 폴링
}
