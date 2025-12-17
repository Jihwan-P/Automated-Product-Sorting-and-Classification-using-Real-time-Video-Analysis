#include <Servo.h>
#include <stdio.h>

// ------------------------------------------------------------------
// 함수 원형 선언
// ------------------------------------------------------------------
void moveToState(int stateIndex);
void moveToAngles(int targetAngles[6]);
void runSequence(int seq[][6], int count);
void moveJointRelative(int jointIndex, int delta);
void setpickup(int a0,int a1,int a2,int a3,int a4,int a5);
void handlePCCommand(String line);      // PC 직접 입력용
void processCmdLine(String line);       // "CMD:state,much" 처리용
void buildSymmetricSequences();         // one/two → oneback/twoback 대칭 생성

// ------------------------------------------------------------------
// 상수 및 전역 변수
// ------------------------------------------------------------------
const int GRIP_OPEN = 55;
const int GRIP_HOLD = 76;

// 6개의 서보 객체
Servo servos[6];

// 서보 핀 (MEGA의 PWM 핀)
int servoPins[6] = {2, 3, 4, 5, 6, 7};

// 상태 각도 테이블
int stateAngles[13][6] = {
  {90, 145,  70, 145, 40, 55},   // home
  {90, 180, 90, 153, 90, 135},   // 2층잡기 2
  {90, 180, 180, 63, 90, 135},   // 2층적재준비 3
  {90, 180, 180, 63, 90, 70},    // 2층적재 4
  {90, 180, 150, 153, 90, 135},  // 1층적재준비과정 5
  {90, 160, 160, 103, 90, 135},  // 1층적재준비과정과정 6
  {90, 140, 160, 73, 90, 135},   // 1층준비 7
  {90, 140, 170, 53, 90, 135},   // 1층준비 8
  {90, 130, 170, 33, 90, 135},   // 1층준비 9
  {90, 120, 170, 23, 90, 135},   // 1층준비 10
  {90, 120, 180, 3, 90, 135},    // 1층준비 11
  {90, 120, 180, 0, 90, 135},    // 1층준비 12
  {90, 120, 180, 0, 90, 70},     // 1층적재 13
};

// ===== 수납장 시퀀스(정방향) =====
int two[11][6] = {
  {90, 145,  70, 145, 40, GRIP_OPEN},
  {90, 145,  70, 145, 40, GRIP_OPEN},
  {180, 145,  70, 145, 40, GRIP_OPEN},
  {180, 145,  70, 145, 40, GRIP_OPEN},
  {180, 145,  70, 145, 40, GRIP_HOLD},
  {180, 155,  50, 135, 40, GRIP_HOLD},
  {180, 145,  70, 145, 40, GRIP_HOLD}, //1
  {180, 145,  70, 145, 40, GRIP_OPEN}, //2
  {180, 155,  70, 145, 40, GRIP_OPEN}, //3 
  {180, 160,  70, 145, 40, GRIP_OPEN}, //4
  {180, 175, 100, 142, 40, GRIP_OPEN}, //5
};

int one[11][6] = {
  {90, 145,  70, 145, 40, GRIP_OPEN},
  {90, 145,  70, 145, 40, GRIP_OPEN},
  {180, 145,  70, 145, 40, GRIP_OPEN},
  {180, 145,  70, 145, 40, GRIP_OPEN},
  {180, 145,  70, 145, 40, GRIP_HOLD},
  {180, 130,  30, 180, 40, GRIP_HOLD},
  {180, 175, 170,  85, 40, GRIP_HOLD}, //1
  {180, 175, 170,  85, 40, GRIP_OPEN}, //2
  {180, 175, 155, 105, 40, GRIP_OPEN}, //3
  {180, 175, 150, 115, 40, GRIP_OPEN}, //4
  {180, 175, 100, 165, 40, GRIP_OPEN}, //5
};

// ===== 수납장 시퀀스(역방향; 코드에서 자동 생성) =====
int twoback[11][6];
int oneback[11][6];

int seqCount = 11;

// 현재 각도
int currentAngles[6];

// ------------------------------------------------------------------
// setup
// ------------------------------------------------------------------
void setup() {
  Serial.begin(115200);   // PC 시리얼 모니터

  Serial.println("6축 로봇팔 '부드러운' 상태 제어 (PC 시리얼 입력 전용)");
  Serial.println("사용 예시:");
  Serial.println("  숫자 state: 1, 2, 3, ..., 18, 21, 31");
  Serial.println("  CMD 형식 : CMD:7,5  (joint0 +5도)");
  Serial.println("  HOME / SET / ABS / REL 사용 가능");
  Serial.println("  각도 6개 : 90 160 0 80 0 70");
  Serial.println("----------------------------------------");

  // 서보 attach
  for (int i = 0; i < 6; i++) {
    servos[i].attach(servoPins[i]);
  }

  // 초기 각도 90으로 가정
  for (int i = 0; i < 6; i++) {
    currentAngles[i] = 90;
    servos[i].write(90);
  }

  // Home 자세로 부드럽게 이동
  int homeTarget[6];
  for (int i = 0; i < 6; i++) {
    homeTarget[i] = stateAngles[0][i];
  }
  moveToAngles(homeTarget);

  // one / two를 기준으로 oneback / twoback 완전 대칭 생성
  buildSymmetricSequences();

  Serial.println("초기화 완료, 명령 대기 중...");
}

// ------------------------------------------------------------------
// loop : PC 시리얼(Serial)에서만 명령 처리
// ------------------------------------------------------------------
void loop() {
  if (Serial.available()) {
    String line = Serial.readStringUntil('\n');
    line.trim();
    if (line.length() == 0) return;

    // "CMD:"로 시작하면 그대로 해석
    if (line.startsWith("CMD:")) {
      Serial.print("[MEGA] PC에서 CMD 수신 = ");
      Serial.println(line);
      processCmdLine(line);
    } else {
      // HOME / SET / ABS / REL / 숫자 state / 각도 6개 등 처리
      handlePCCommand(line);
    }
  }
}

// ------------------------------------------------------------------
// one, two → oneback, twoback 완전 역순 복사 (대칭 시퀀스 생성)
// ------------------------------------------------------------------
void buildSymmetricSequences() {
  for (int i = 0; i < seqCount; i++) {
    int rev = seqCount - 1 - i;  // 역방향 인덱스
    for (int j = 0; j < 6; j++) {
      oneback[i][j]  = one[rev][j];
      twoback[i][j]  = two[rev][j];
    }
  }
}

// ------------------------------------------------------------------
// "CMD:state,much" 문자열 하나를 공통으로 처리
// ------------------------------------------------------------------
void processCmdLine(String line) {
  if (!line.startsWith("CMD:")) {
    Serial.println("[MEGA] CMD:로 시작 안 해서 무시");
    return;
  }

  String payload = line.substring(4);   // "7,5" 또는 "21"
  payload.trim();

  int state = 0;
  int much  = 0;

  int commaIndex = payload.indexOf(',');
  if (commaIndex < 0) {
    state = payload.toInt();
    much  = 0;
  } else {
    String sState = payload.substring(0, commaIndex);
    String sMuch  = payload.substring(commaIndex + 1);
    sState.trim();
    sMuch.trim();
    state = sState.toInt();
    much  = sMuch.toInt();   // 음수/양수 모두 허용
  }

  if (state <= 0) {
    Serial.println("[MEGA] state <= 0, 무시");
    return;
  }

  Serial.print("[MEGA] 해석된 state = ");
  Serial.print(state);
  Serial.print(", much = ");
  Serial.println(much);

  // ==== 기존 잡기/회전 명령 ====
  if (state == 1) {
    Serial.println("명령: (0,0) 잡기 대기");
    setpickup(75, 160, 118, 140, 40, GRIP_OPEN);

  } else if (state == 2) {
    Serial.println("명령: (1,0) 잡기 대기");
    setpickup(89, 160, 118, 140, 40, GRIP_OPEN);

  } else if (state == 3) {
    Serial.println("명령: (0,1) 잡기 대기");
    setpickup(79, 130, 85, 130, 40, GRIP_OPEN);

  } else if (state == 4) {
    Serial.println("명령: (1,1) 잡기 대기");
    setpickup(89, 130, 85, 130, 40, GRIP_OPEN);

  } else if (state == 5) {
    Serial.println("명령: 좌회전 (베이스 -90도)");
    moveJointRelative(0, -90);

  } else if (state == 6) {
    Serial.println("명령: 우회전 (베이스 +90도)");
    moveJointRelative(0, +90);

  // ==== 모터를 much만큼 상대 이동 (음수/양수 모두 허용) ====
  } else if (state == 7) {      // joint 0 +
    Serial.print("명령: 0모터 이동 (delta = ");
    Serial.print(much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(0,  much);

  } else if (state == 8) {      // joint 0 -
    Serial.print("명령: 0모터 이동 (delta = ");
    Serial.print(-much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(0, -much);

  } else if (state == 9) {      // joint 1 +
    Serial.print("명령: 1모터 이동 (delta = ");
    Serial.print(much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(1,  much);

  } else if (state == 10) {     // joint 1 -
    Serial.print("명령: 1모터 이동 (delta = ");
    Serial.print(-much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(1, -much);

  } else if (state == 11) {     // joint 2 +
    Serial.print("명령: 2모터 이동 (delta = ");
    Serial.print(much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(2,  much);

  } else if (state == 12) {     // joint 2 -
    Serial.print("명령: 2모터 이동 (delta = ");
    Serial.print(-much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(2, -much);

  } else if (state == 13) {     // joint 3 +
    Serial.print("명령: 3모터 이동 (delta = ");
    Serial.print(much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(3,  much);

  } else if (state == 14) {     // joint 3 -
    Serial.print("명령: 3모터 이동 (delta = ");
    Serial.print(-much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(3, -much);

  } else if (state == 15) {     // joint 4 +
    Serial.print("명령: 4모터 이동 (delta = ");
    Serial.print(much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(4,  much);

  } else if (state == 16) {     // joint 4 -
    Serial.print("명령: 4모터 이동 (delta = ");
    Serial.print(-much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(4, -much);

  } else if (state == 17) {     // joint 5 +
    Serial.print("명령: 5모터 이동 (delta = ");
    Serial.print(much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(5,  much);

  } else if (state == 18) {     // joint 5 -
    Serial.print("명령: 5모터 이동 (delta = ");
    Serial.print(-much);
    Serial.println(" 도)");
    if (much != 0) moveJointRelative(5, -much);
  }

  // ==== 수납장 시퀀스 ====
  else if (state == 31) {
    Serial.println("명령: 수납장 2로 이동 (two 시퀀스)");
    runSequence(two, seqCount);

  } else if (state == 21) {
    Serial.println("명령: 수납장 1로 이동 (one 시퀀스)");
    runSequence(one, seqCount);

  } else if (state == 32) {
    Serial.println("명령: 수납장 2에서 빼기 (twoback 시퀀스)");
    runSequence(twoback, seqCount);

  } else if (state == 22) {
    Serial.println("명령: 수납장 1에서 빼기 (oneback 시퀀스)");
    runSequence(oneback, seqCount);
  }
}

// ------------------------------------------------------------------
// PC에서 직접 입력한 문자열 처리
//  - 숫자만 입력: state 번호로 처리 → processCmdLine("CMD:state")
//  - HOME: 홈 자세 (state 0)
//  - SET / ABS / REL: 기존 명령
//  - "a0 a1 a2 a3 a4 a5": 6개 각도 직접 입력
// ------------------------------------------------------------------
void handlePCCommand(String line) {
  Serial.print("[PC] 명령: ");
  Serial.println(line);

  // 1) 숫자만 들어온 경우 → state 번호로 간주해서 processCmdLine 재사용
  bool onlyDigits = true;
  for (unsigned int i = 0; i < line.length(); i++) {
    if (!isDigit(line[i])) {
      onlyDigits = false;
      break;
    }
  }

  if (onlyDigits && line.length() > 0) {
    int state = line.toInt();
    if (state > 0) {
      String cmd = "CMD:" + String(state);   // "CMD:1", "CMD:21", "CMD:31" ...
      Serial.print("[PC] 숫자 입력 → ");
      Serial.println(cmd);
      processCmdLine(cmd);
      return;
    }
  }

  // 2) HOME 명령
  if (line.equalsIgnoreCase("HOME")) {
    Serial.println("[PC] HOME → state 0으로 이동");
    moveToState(0);
    return;
  }

  // 문자열을 C 문자열로 변환 (sscanf 사용용)
  char buf[64];
  line.toCharArray(buf, sizeof(buf));

  int a0, a1, a2, a3, a4, a5;
  int joint, val;
  int parsed;

  // 3) SET 명령: "SET a0 a1 a2 a3 a4 a5"
  parsed = sscanf(buf, "SET %d %d %d %d %d %d",
                  &a0, &a1, &a2, &a3, &a4, &a5);
  if (parsed == 6) {
    int target[6] = {a0, a1, a2, a3, a4, a5};
    Serial.println("[PC] SET 명령 → 지정 각도로 이동");
    moveToAngles(target);
    return;
  }

  // 4) ABS 명령: "ABS joint val"
  parsed = sscanf(buf, "ABS %d %d", &joint, &val);
  if (parsed == 2) {
    if (joint < 0) joint = 0;
    if (joint > 5) joint = 5;
    if (val   < 0) val   = 0;
    if (val   > 180) val = 180;

    int target[6];
    for (int i = 0; i < 6; i++) target[i] = currentAngles[i];
    target[joint] = val;

    Serial.print("[PC] ABS 명령 → 조인트 ");
    Serial.print(joint);
    Serial.print(" 을(를) ");
    Serial.print(val);
    Serial.println(" 도로 이동");
    moveToAngles(target);
    return;
  }

  // 5) REL 명령: "REL joint delta"
  int delta;
  parsed = sscanf(buf, "REL %d %d", &joint, &delta);
  if (parsed == 2) {
    Serial.print("[PC] REL 명령 → 조인트 ");
    Serial.print(joint);
    Serial.print(" 을(를) ");
    Serial.print(delta);
    Serial.println(" 도 만큼 상대 이동");
    moveJointRelative(joint, delta);
    return;
  }

  // 6) 각도 6개를 공백으로만 입력한 경우:
  //    예: "90 160 0 80 0 70"
  parsed = sscanf(buf, "%d %d %d %d %d %d",
                  &a0, &a1, &a2, &a3, &a4, &a5);
  if (parsed == 6) {
    int target[6] = {a0, a1, a2, a3, a4, a5};

    // 각도 범위 보정 (0 ~ 180)
    for (int i = 0; i < 6; i++) {
      if (target[i] < 0)   target[i] = 0;
      if (target[i] > 180) target[i] = 180;
    }

    Serial.print("명령: 직접 입력 각도로 이동 -> ");
    for (int i = 0; i < 6; i++) {
      Serial.print(target[i]);
      if (i < 5) Serial.print(", ");
    }
    Serial.println();

    moveToAngles(target);
    return;
  }

  // 7) 어느 형식에도 안 맞으면 에러 메시지
  Serial.println("[PC] 알 수 없는 명령입니다.");
  Serial.println("사용 예) HOME / SET / ABS / REL / 숫자 state(1, 2, 21, 31) / 각도 6개(예: 90 160 0 80 0 70)");
}

// ------------------------------------------------------------------
// 집기 위치 자동 구성
//  - 이제 forward(one, two)만 수정하고,
//  - 마지막에 buildSymmetricSequences()로 back 시퀀스 자동 재생성
// ------------------------------------------------------------------
void setpickup(int a0,int a1,int a2,int a3,int a4,int a5) {
  int base[6] = {a0, a1, a2, a3, a4, a5};

  two[1][0] = base[0];
  one[1][0] = base[0];
  // 1) 잡기 준비 위치를 1번 스텝에 반영
  for (int i = 0; i < 6; i++) {
    two[2][i] = base[i];
    one[2][i] = base[i];
  }

  // 2) 그 위치에서 그립을 닫는 상태(2번 스텝)
  base[5] = GRIP_HOLD;
  for (int i = 0; i < 6; i++) {
    two[3][i] = base[i];
    one[3][i] = base[i];
  }

  // 3) 어깨(1번 조인트)를 180도로 틀어 넣는 상태(3번 스텝)
  base[1] = 180;
  base[2] = 95;
  for (int i = 0; i < 6; i++) {
    two[4][i] = base[i];
    one[4][i] = base[i];
  }

  // forward 시퀀스를 수정했으니, 항상 그 기준으로 back 시퀀스를 대칭 생성
  buildSymmetricSequences();
}

// ------------------------------------------------------------------
void runSequence(int seq[][6], int count) {
  for (int i = 0; i < count; i++) {
    moveToAngles(seq[i]);
    delay(200);
  }
}

// ------------------------------------------------------------------
void moveToAngles(int target[6]) {
  int steps = 50;
  for (int s = 1; s <= steps; s++) {
    for (int i = 0; i < 6; i++) {
      int newAngle = currentAngles[i] +
                     (target[i] - currentAngles[i]) * s / steps;
      servos[i].write(newAngle);
    }
    delay(50);
  }
  for (int i = 0; i < 6; i++) {
    currentAngles[i] = target[i];
  }
}

// ------------------------------------------------------------------
void moveToState(int stateIndex) {
  int targetAngles[6];
  for (int i = 0; i < 6; i++) {
    targetAngles[i] = stateAngles[stateIndex][i];
  }
  moveToAngles(targetAngles);
}

// ------------------------------------------------------------------
void moveJointRelative(int jointIndex, int delta) {
  int target[6];
  for (int i = 0; i < 6; i++) target[i] = currentAngles[i];

  if (jointIndex < 0) jointIndex = 0;
  if (jointIndex > 5) jointIndex = 5;

  int newAngle = currentAngles[jointIndex] + delta;
  if (newAngle < 0)   newAngle = 0;
  if (newAngle > 180) newAngle = 180;

  target[jointIndex] = newAngle;

  Serial.print("조인트 ");
  Serial.print(jointIndex);
  Serial.print(" : ");
  Serial.print(currentAngles[jointIndex]);
  Serial.print(" -> ");
  Serial.println(newAngle);

  moveToAngles(target);
}
