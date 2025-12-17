/*
  호서대전자과 존잘미남 이진희님 전용
  [v14.6 Fixed: Command Response]
  - 회전 명령 후 자동 복귀 로직 제거
  - 모든 명령은 새 명령이 올 때까지 유지
  - 라인트레이서 보정 로직 제거 (수동 조작 전용)
*/

// ----------------------------------------------------------------------
// [통신 및 원격 제어용 설정]
// ----------------------------------------------------------------------
const char* WIFI_SSID = "moble_main_2.4G";
const char* WIFI_PASS = "moble2025";
const char* TCP_PORT = "80";
const long PC_BAUD_RATE = 9600;
const long ESP_BAUD_RATE = 9600;
const unsigned long TIMEOUT = 5000;

// (핀 설정)
#define REAR_ENA 8
#define REAR_IN1 9
#define REAR_IN2 10
#define REAR_IN3 11
#define REAR_IN4 12
#define REAR_ENB 13
#define FRONT_ENA 7
#define FRONT_IN1 6
#define FRONT_IN2 5
#define FRONT_IN3 4
#define FRONT_IN4 3
#define FRONT_ENB 2

#define FRONT_TRIG_PIN A0
#define FRONT_ECHO_PIN A1
#define REAR_TRIG_PIN  A2
#define REAR_ECHO_PIN  A3
#define STOP_DISTANCE 5
#define BUZZER_PIN A8

// [라인 센서]
#define leftSensorPin A4
#define rightSensorPin A5
#define middleSensorPin A6
#define rearLeftSensorPin A9
#define rearRightSensorPin A10

// 센서 상태 변수
int middleSensorState = 0;

// ----------------------------------------------------------------------
// [전역 변수 & 타이머 변수]
// ----------------------------------------------------------------------
int current_move_direction = 0; // 0=정지, 1=전진, -1=후진 등
int current_max_speed = 255;    // 초기 속도

// 비동기 처리를 위한 타이머 변수들
unsigned long currentMillis = 0;
unsigned long buzzerStartTime = 0;
bool isBuzzingCmd = false;

// [주차 센서 무시용 타이머]
unsigned long parkingIgnoreStartTime = 0;
bool isParkingIgnored = false;

// [장애물 부저 상태]
bool obstacle_buzzing = false;

// ----------------------------------------------------------------------
// [함수 정의]
// ----------------------------------------------------------------------
float getDistance(int trigPin, int echoPin) {
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  long duration = pulseIn(echoPin, HIGH, 30000);
  if (duration == 0) return 999.0; 
  return duration * 0.034 / 2.0;
}

void driveMotor(int ena, int in1, int in2, int speed, int dir) {
  if (dir > 0)      { digitalWrite(in1, HIGH); digitalWrite(in2, LOW); }
  else if (dir < 0) { digitalWrite(in1, LOW);  digitalWrite(in2, HIGH); }
  else              { digitalWrite(in1, LOW);  digitalWrite(in2, LOW); }
  analogWrite(ena, (dir == 0) ? 0 : constrain(speed, 0, 255));
}

void stopAllMotors(const char* reason) {
  driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, 0, 0);
  driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, 0, 0);
  driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, 0, 0);
  driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, 0, 0);
  
  static String last_reason = "";
  if (last_reason != reason) {
    Serial.print("⛔ ");
    Serial.print(reason);
    Serial.println(" - 모터 정지");
    last_reason = reason;
  }
}

void handleCommand(char command) {
  switch(command) {
    case 'F':
      current_move_direction = 1;
      // ★ 핵심: 명령을 받으면 1초간 라인 센서를 무시하도록 설정
      isParkingIgnored = true;
      parkingIgnoreStartTime = millis();
      Serial.println(">>> [전진] <<<");
      break;
      
    case 'S':
      current_move_direction = 0;
      isParkingIgnored = false;
      Serial.println(">>> [정지] <<<");
      break;
      
    case 'B':
      current_move_direction = -1;
      // ★ 후진 시에도 1초간 무시 (출발 용이)
      isParkingIgnored = true;
      parkingIgnoreStartTime = millis();
      Serial.println(">>> [후진] <<<");
      break;
      
    case 'D':
      current_move_direction = -2;
      Serial.println(">>> [전진우회전] <<<");
      break;
      
    case 'A':
      current_move_direction = 2;
      Serial.println(">>> [전진좌회전] <<<");
      break;
      
    case 'W':
      current_move_direction = -3;
      Serial.println(">>> [후진우회전] <<<");
      break;
      
    case 'X':
      current_move_direction = 3;
      Serial.println(">>> [후진좌회전] <<<");
      break;
      
    case 'Q':
      current_move_direction = -4;
      Serial.println(">>> [정지우회전] <<<");
      break;
      
    case 'E':
      current_move_direction = 4;
      Serial.println(">>> [정지좌회전] <<<");
      break;
      
    case 'Z':
      Serial.println(">>> [부저] 빵빵!  <<<");
      tone(BUZZER_PIN, 2000);
      buzzerStartTime = millis();
      isBuzzingCmd = true;
      break;
      
    default:
      Serial.println(">>> 알 수 없는 명령 <<<");
      break;
  }
}

boolean sendATCommand(const char* command, unsigned long timeout) {
  while (Serial1.available()) Serial1.read(); // 버퍼 비우기
  
  Serial1.println(command);
  unsigned long startTime = millis();
  String response = "";
  
  while (millis() - startTime < timeout) {
    if (Serial1.available()) {
      response += (char)Serial1.read();
      if (response.indexOf("OK") != -1 || 
          response.indexOf("WIFI GOT IP") != -1 || 
          response.indexOf("ready") != -1) {
        Serial.print("<- 성공: "); Serial.println(response);
        return true;
      } else if (response.indexOf("ERROR") != -1 || 
                 response.indexOf("FAIL") != -1) {
        Serial.print("<- 실패: "); Serial.println(response);
        return false;
      }
    }
  }
  Serial.print("<- 타임아웃: "); Serial.println(response);
  return false;
}

boolean sendDataToClient(char conn_id, String data_to_send) {
  String cmd = "AT+CIPSEND=";
  cmd += conn_id;
  cmd += ",";
  cmd += data_to_send.length();
  Serial1.println(cmd);
  
  unsigned long startTime = millis();
  String response = "";
  
  while (millis() - startTime < 3000) {
    if (Serial1.available()) {
      char c = Serial1.read();
      response += c;
      if (response.indexOf('>') != -1) {
        Serial1.println(data_to_send);
        startTime = millis();
        response = "";
        while (millis() - startTime < 3000) {
          if (Serial1.available()) {
            response += (char)Serial1.read();
            if (response.indexOf("SEND OK") != -1) {
              Serial.println("<- 데이터 전송 성공");
              return true;
            } else if (response.indexOf("ERROR") != -1) {
              return false;
            }
          }
        }
        return false;
      }
    }
  }
  return false;
}

void setup() {
  // [모터 핀 초기화]
  int pins[] = {
    REAR_ENA, REAR_IN1, REAR_IN2, REAR_ENB, REAR_IN3, REAR_IN4,
    FRONT_ENA, FRONT_IN1, FRONT_IN2, FRONT_ENB, FRONT_IN3, FRONT_IN4
  };
  for (int p : pins) pinMode(p, OUTPUT);

  // [센서 핀 초기화]
  pinMode(FRONT_TRIG_PIN, OUTPUT);
  pinMode(FRONT_ECHO_PIN, INPUT);
  pinMode(REAR_TRIG_PIN, OUTPUT);
  pinMode(REAR_ECHO_PIN, INPUT);
  pinMode(BUZZER_PIN, OUTPUT);
  pinMode(middleSensorPin, INPUT);

  Serial.begin(PC_BAUD_RATE);
  Serial1.begin(ESP_BAUD_RATE);
  Serial.println("--- 4WD Robot (v14.6: Fixed) ---");

  Serial.println("--- ESP-01 초기화 시작 ---");
  if (! sendATCommand("AT+RST", 2000)) return;
  delay(2000);
  if (!sendATCommand("AT", 1000)) return;
  if (!sendATCommand("AT+CWMODE=1", TIMEOUT)) return;

  String cmd = "AT+CWJAP=\"";
  cmd += WIFI_SSID;
  cmd += "\",\"";
  cmd += WIFI_PASS;
  cmd += "\"";
  if (! sendATCommand(cmd.c_str(), 15000)) {
    Serial.println("Wi-Fi 연결 실패");
    return;
  }
  Serial.println("Wi-Fi 연결 성공");
  delay(2000);

  if (!sendATCommand("AT+CIPMUX=1", TIMEOUT)) return;
  delay(500);
  sendATCommand("AT+CIPCLOSE", 500);
  sendATCommand("AT+CIPSERVER=0", 500);

  String serverCmd = "AT+CIPSERVER=1,";
  serverCmd += TCP_PORT;
  if (!sendATCommand(serverCmd.c_str(), TIMEOUT)) return;
  Serial.println("TCP 서버 시작 (포트 80)");

  Serial.println("\n*** IP 주소 확인 ***");
  sendATCommand("AT+CIFSR", TIMEOUT);
  Serial.println("\n--- 준비 완료 ---\n");

  stopAllMotors("초기화");
}

// ----------------------------------------------------------------------
// [메인 루프]
// ----------------------------------------------------------------------
void loop() {
  currentMillis = millis();

  // [부저 자동 끄기]
  if (isBuzzingCmd && (currentMillis - buzzerStartTime > 300)) {
    noTone(BUZZER_PIN);
    isBuzzingCmd = false;
  }

  // [주차 무시 타이머] - 1초가 지나면 무시 모드 해제
  if (isParkingIgnored && (currentMillis - parkingIgnoreStartTime >= 1000)) {
    isParkingIgnored = false;
  }

  // 1. 원격 명령 수신
  if (Serial1.available()) {
    String response = Serial1.readStringUntil('\n');
    response.trim();

    if (response.startsWith("+IPD") && response.indexOf("cmd=") != -1) {
      int id_start = response.indexOf(',') + 1;
      char conn_id = response.charAt(id_start);

      int cmd_index = response.indexOf("cmd=") + 4;
      char command = ' ';
      if (cmd_index < (int)response.length()) {
        command = response.charAt(cmd_index);
      }

      int speed_index = response.indexOf("speed=");
      if (speed_index != -1) {
        String speed_str = response.substring(speed_index + 6);
        int new_speed = speed_str.toInt();
        if (new_speed >= 100 && new_speed <= 255) {
          current_max_speed = new_speed;
          Serial.print("속도 변경: "); Serial.println(new_speed);
        }
      }

      if (command != 'V' && command != ' ') {
        handleCommand(command);
      }

      String http_response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nOK";
      sendDataToClient(conn_id, http_response);

      String close_cmd = "AT+CIPCLOSE=";
      close_cmd += conn_id;
      sendATCommand(close_cmd.c_str(), 500);
    }
  }

  // 2. 센서 읽기
  middleSensorState = digitalRead(middleSensorPin);
  float front_distance = getDistance(FRONT_TRIG_PIN, FRONT_ECHO_PIN);
  float rear_distance = getDistance(REAR_TRIG_PIN, REAR_ECHO_PIN);

  bool front_obstacle = (front_distance > 0 && front_distance <= STOP_DISTANCE);
  bool rear_obstacle = (rear_distance > 0 && rear_distance <= STOP_DISTANCE);

  // 3. 장애물 감지
  bool obstacle_detected = false;
  
  if ((current_move_direction == 1 && front_obstacle) ||
      (current_move_direction == -1 && rear_obstacle)) {
    stopAllMotors("장애물 감지");
    obstacle_detected = true;
    if (!obstacle_buzzing) {
      tone(BUZZER_PIN, 1000);
      obstacle_buzzing = true;
    }
  } else {
    if (obstacle_buzzing) {
      noTone(BUZZER_PIN);
      obstacle_buzzing = false;
    }
  }

  if (obstacle_detected) return;

  // 4. 주차 라인 감지 (중앙 센서만)
  // ★ isParkingIgnored가 true인 1초 동안은 이 코드가 실행되지 않아 라인을 무시하고 통과함
  if ((current_move_direction == 1 || current_move_direction == -1) &&
      middleSensorState == HIGH && !isParkingIgnored) {
    stopAllMotors("주차 라인");
    current_move_direction = 0;
    return;
  }

  // 5. 모터 구동
  switch (current_move_direction) {
    case 1: // 전진
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, current_max_speed, 1);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, current_max_speed, 1);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, current_max_speed, 1);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, current_max_speed, 1);
      break;

    case -1: // 후진
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, current_max_speed, -1);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, current_max_speed, -1);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, current_max_speed, -1);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, current_max_speed, -1);
      break;

    case 2: // 전진좌회전
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, 0, 0);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, 255, 1);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, 0, 0);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, 255, 1);
      break;

    case -2: // 전진우회전
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, 255, 1);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, 0, 0);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, 255, 1);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, 0, 0);
      break;

    case 3: // 후진좌회전
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, 255, -1);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, 0, 0);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, 255, -1);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, 0, 0);
      break;

    case -3: // 후진우회전
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, 0, 0);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, 255, -1);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, 0, 0);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, 255, -1);
      break;

    case 4: // 정지좌회전
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, 0, 0);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, 255, 1);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, 0, 0);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, 255, 1);
      break;

    case -4: // 정지우회전
      driveMotor(REAR_ENA, REAR_IN1, REAR_IN2, 255, 1);
      driveMotor(REAR_ENB, REAR_IN3, REAR_IN4, 0, 0);
      driveMotor(FRONT_ENA, FRONT_IN1, FRONT_IN2, 255, 1);
      driveMotor(FRONT_ENB, FRONT_IN3, FRONT_IN4, 0, 0);
      break;

    case 0: // 정지
    default:
      stopAllMotors("대기");
      break;
  }
}