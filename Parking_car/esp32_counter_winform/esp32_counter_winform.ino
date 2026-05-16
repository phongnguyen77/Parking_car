#include <Arduino.h>
#include <SPI.h>
#include <MFRC522.h>
#include <ESP32Servo.h>

// ===================== PIN MAP =====================
static const int PIN_SCK  = 12;
static const int PIN_MISO = 13;
static const int PIN_MOSI = 11;

static const int PIN_CS_ENTRY  = 9;   // SDA/SS RFID Entry
static const int PIN_CS_EXIT   = 10;  // SDA/SS RFID Exit
static const int PIN_RST_ENTRY = 14;
static const int PIN_RST_EXIT  = 21;

static const int IR_ENTRY_FRONT = 4;
static const int IR_ENTRY_REAR  = 5;
static const int IR_EXIT_FRONT  = 6;
static const int IR_EXIT_REAR   = 7;

static const int SERVO_ENTRY_PIN = 17;
static const int SERVO_EXIT_PIN  = 18;

// ============== CONFIG (tùy chỉnh theo thực tế) ==============
static const bool IR_ACTIVE_LOW = true;       // đa số module IR output LOW khi bị che
static const uint32_t IR_DEBOUNCE_MS = 80;    // chống nhiễu
static const uint32_t AUTH_TIMEOUT_MS = 30000; // chờ PC xác thực trong 30s
static const uint32_t MIN_GATE_OPEN_MS = 4000; // giữ barie mở tối thiểu 4 giây

static const int SERVO_CLOSED_ANGLE = 0;
static const int SERVO_OPEN_ANGLE   = 90;

// ===================== RFID =====================
MFRC522 rfidEntry(PIN_CS_ENTRY, PIN_RST_ENTRY);
MFRC522 rfidExit(PIN_CS_EXIT, PIN_RST_EXIT);

// ===================== SERVO =====================
Servo servoEntry;
Servo servoExit;

// ===================== STATE MACHINES =====================
enum LaneState { IDLE, WAIT_AUTH, GATE_OPEN };
struct LaneFSM {
  LaneState state = IDLE;

  // IR edge detect + debounce
  bool lastFront = false;
  bool lastRear  = false;
  uint32_t lastFrontChangeMs = 0;
  uint32_t lastRearChangeMs  = 0;

  // Timing
  uint32_t waitStartMs = 0;
  uint32_t gateOpenMs  = 0;   // thời điểm barie mở

  // Flags
  bool detectedSent  = false;
  bool rearWasActive = false; // edge detect: đóng khi xe qua hết (rear: active→inactive)
};

LaneFSM entryLane;
LaneFSM exitLane;

// ===================== SERIAL LINE BUFFER =====================
String serialLine;

// ===================== HELPERS =====================
bool readIR(int pin) {
  int v = digitalRead(pin);
  bool active = IR_ACTIVE_LOW ? (v == LOW) : (v == HIGH);
  return active;
}

void servoSetClosed(bool isEntry) {
  if (isEntry) servoEntry.write(SERVO_CLOSED_ANGLE);
  else         servoExit.write(SERVO_CLOSED_ANGLE);
}

void servoSetOpen(bool isEntry) {
  if (isEntry) servoEntry.write(SERVO_OPEN_ANGLE);
  else         servoExit.write(SERVO_OPEN_ANGLE);
}

String uidToString(MFRC522 &rfid) {
  String uid = "";
  for (byte i = 0; i < rfid.uid.size; i++) {
    if (rfid.uid.uidByte[i] < 0x10) uid += "0";
    uid += String(rfid.uid.uidByte[i], HEX);
    if (i < rfid.uid.size - 1) uid += " ";
  }
  uid.toUpperCase();
  return uid;
}

void sendEvent(const String &s) {
  Serial.println(s);
}

// ===================== RFID POLL =====================
void pollRFIDOne(MFRC522 &reader, const char *tagPrefix) {
  if (!reader.PICC_IsNewCardPresent()) return;
  if (!reader.PICC_ReadCardSerial()) return;

  String uid = uidToString(reader);
  sendEvent(String(tagPrefix) + ":UID=" + uid);

  reader.PICC_HaltA();
  reader.PCD_StopCrypto1();
}

void pollRFID() {
  // Poll both readers quickly
  pollRFIDOne(rfidEntry, "RFID_ENTRY");
  pollRFIDOne(rfidExit,  "RFID_EXIT");
}

// ===================== IR FSM UPDATE =====================
void updateLaneFSM(
  LaneFSM &lane,
  bool frontActive,
  bool rearActive,
  const char *eventDetected,
  const char *eventPassed,
  const char *eventTimeout
) {
  uint32_t now = millis();

  // ----- front edge detect (debounce) -----
  if (frontActive != lane.lastFront) {
    if (now - lane.lastFrontChangeMs >= IR_DEBOUNCE_MS) {
      lane.lastFrontChangeMs = now;
      lane.lastFront = frontActive;

      if (frontActive) {
        // Front IR triggered
        if (lane.state == IDLE) {
          lane.state = WAIT_AUTH;
          lane.waitStartMs = now;
          lane.detectedSent = false;
        }
      }
    }
  }

  // ----- rear edge detect (debounce) -----
  if (rearActive != lane.lastRear) {
    if (now - lane.lastRearChangeMs >= IR_DEBOUNCE_MS) {
      lane.lastRearChangeMs = now;
      lane.lastRear = rearActive;
      // rearActive used below depending state
    }
  }

  // ----- state actions -----
  switch (lane.state) {
    case IDLE:
      lane.detectedSent = false;
      break;

    case WAIT_AUTH:
      if (!lane.detectedSent) {
        sendEvent(eventDetected);
        lane.detectedSent = true;
      }
      // Timeout if PC doesn't authorize
      if (now - lane.waitStartMs > AUTH_TIMEOUT_MS) {
        sendEvent(eventTimeout);
        lane.state = IDLE;
      }
      break;

    case GATE_OPEN:
      // Giữ barie mở tối thiểu MIN_GATE_OPEN_MS để tránh đóng ngay do IR nhiễu
      if (now - lane.gateOpenMs >= MIN_GATE_OPEN_MS) {
        // Đóng barie khi xe qua hết: rear IR chuyển từ active → inactive (falling edge)
        if (lane.rearWasActive && !rearActive) {
          sendEvent(eventPassed);
          lane.state = IDLE;
        }
        lane.rearWasActive = rearActive;
      }
      break;
  }
}

// ===================== SERIAL COMMANDS =====================
void handleCommand(String cmd) {
  cmd.trim();
  cmd.toUpperCase();

  if (cmd == "PING") {
    sendEvent("PONG");
    return;
  }

  if (cmd == "RESET") {
    // reset states and close gates
    entryLane.state = IDLE;
    exitLane.state  = IDLE;
    servoSetClosed(true);
    servoSetClosed(false);
    sendEvent("RESET_OK");
    return;
  }

  if (cmd == "OPEN_ENTRY") {
    servoSetOpen(true);
    entryLane.state        = GATE_OPEN;
    entryLane.gateOpenMs   = millis();
    entryLane.rearWasActive = false;
    sendEvent("ENTRY_OPENED");
    return;
  }

  if (cmd == "CLOSE_ENTRY") {
    servoSetClosed(true);
    entryLane.state = IDLE;
    sendEvent("ENTRY_CLOSED");
    return;
  }

  if (cmd == "OPEN_EXIT") {
    servoSetOpen(false);
    exitLane.state        = GATE_OPEN;
    exitLane.gateOpenMs   = millis();
    exitLane.rearWasActive = false;
    sendEvent("EXIT_OPENED");
    return;
  }

  if (cmd == "CLOSE_EXIT") {
    servoSetClosed(false);
    exitLane.state = IDLE;
    sendEvent("EXIT_CLOSED");
    return;
  }

  // unknown command
  sendEvent("ERR:UNKNOWN_CMD");
}

void pollSerial() {
  while (Serial.available()) {
    char c = (char)Serial.read();
    if (c == '\n') {
      String cmd = serialLine;
      serialLine = "";
      cmd.replace("\r", "");
      if (cmd.length() > 0) handleCommand(cmd);
    } else {
      // prevent huge buffer
      if (serialLine.length() < 120) serialLine += c;
    }
  }
}

// ===================== SETUP =====================
void setup() {
  Serial.begin(115200);
  delay(300);

  pinMode(IR_ENTRY_FRONT, INPUT_PULLUP);
  pinMode(IR_ENTRY_REAR,  INPUT_PULLUP);
  pinMode(IR_EXIT_FRONT,  INPUT_PULLUP);
  pinMode(IR_EXIT_REAR,   INPUT_PULLUP);

  // Servo attach (ESP32Servo)
  servoEntry.setPeriodHertz(50);
  servoExit.setPeriodHertz(50);
  servoEntry.attach(SERVO_ENTRY_PIN, 500, 2400);
  servoExit.attach(SERVO_EXIT_PIN,  500, 2400);

  servoSetClosed(true);
  servoSetClosed(false);

  // SPI init with custom pins (ESP32-S3 ok)
  SPI.begin(PIN_SCK, PIN_MISO, PIN_MOSI);

  rfidEntry.PCD_Init();
  rfidExit.PCD_Init();

  sendEvent("ESP32_READY");
}

// ===================== LOOP =====================
void loop() {
  pollSerial();
  pollRFID();

  bool entryFront = readIR(IR_ENTRY_FRONT);
  bool entryRear  = readIR(IR_ENTRY_REAR);
  bool exitFront  = readIR(IR_EXIT_FRONT);
  bool exitRear   = readIR(IR_EXIT_REAR);

  updateLaneFSM(entryLane, entryFront, entryRear,
                "ENTRY_DETECTED", "ENTRY_PASSED", "ENTRY_TIMEOUT");

  updateLaneFSM(exitLane, exitFront, exitRear,
                "EXIT_DETECTED", "EXIT_PASSED", "EXIT_TIMEOUT");

  // If state returned to IDLE from GATE_OPEN, close servo here (ensures closure)
  if (entryLane.state == IDLE) servoSetClosed(true);
  if (exitLane.state  == IDLE) servoSetClosed(false);

  delay(5);
}