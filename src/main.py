import math
import random
import cv2
import numpy as np
import pyautogui
import time
import mss
import os
from win32 import winxpgui

# ===============================
# CONFIG
# ===============================
TEMPLATE_PATH = "src/images/"
CHECK_INTERVAL = 1.0
CLICK_DELAY = 0.5
LOG_FILE = "bot_log.txt"

# ===============================
# LOGGING
# ===============================


def log_action(message: str):
    timestamp = time.strftime("[%Y-%m-%d %H:%M:%S]")
    line = f"{timestamp} {message}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")

# ===============================
# LOAD TEMPLATE (CACHE)
# ===============================


def load_templates():
    templates = {}
    if not os.path.exists(TEMPLATE_PATH):
        os.makedirs(TEMPLATE_PATH)
    for name in os.listdir(TEMPLATE_PATH):
        if name.lower().endswith((".png", ".jpg", ".jpeg")):
            path = os.path.join(TEMPLATE_PATH, name)
            img = cv2.imread(path)
            if img is not None:
                templates[name] = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    log_action(f"Loaded templates: {list(templates.keys())}")
    return templates

# ===============================
# SCREEN CAPTURE
# ===============================


def capture_screen():
    with mss.mss() as sct:
        monitor = sct.monitors[1]
        img = np.array(sct.grab(monitor))
        return cv2.cvtColor(img, cv2.COLOR_BGRA2BGR)

# ===============================
# SCREEN DETECTION
# ===============================


def detect_screen_type(img_bgr, templates, threshold=0.45):
    img_gray = cv2.cvtColor(img_bgr, cv2.COLOR_BGR2GRAY)
    best_type = "unknown"
    best_val = 0
    for name, tmpl in templates.items():
        res = cv2.matchTemplate(img_gray, tmpl, cv2.TM_CCOEFF_NORMED)
        _, max_val, _, _ = cv2.minMaxLoc(res)
        if max_val > best_val:
            best_val = max_val
            best_type = name
    if best_val > threshold:
        return best_type
    return "unknown"


def wait_for_screen(expected, templates, timeout=5.0, interval=0.5):
    start = time.time()
    while time.time() - start < timeout:
        img = capture_screen()
        screen = detect_screen_type(img, templates)
        if expected in screen:
            log_action(f"[OK] Screen changed to '{expected}' ✅")
            return True
        time.sleep(interval)
    log_action(f"[WARN] Screen '{expected}' not detected within {timeout}s ❌")
    return False

# ===============================
# CLICK UTILS
# ===============================


def click(x, y, action_name="unknown", delay=0.2, num_clicks=1):
    if is_game_active():
        try:
            for _ in range(num_clicks):
                pyautogui.click(x, y)
            log_action(f"[CLICK] {action_name.upper()} at ({x},{y})")
            time.sleep(delay)
        except Exception as e:
            log_action(
                f"[ERROR] Click failed at ({x},{y}) for '{action_name}': {e}")


def is_game_active(window_keywords=["Royal Revolt"]):
    try:
        hwnd = winxpgui.GetForegroundWindow()
        window_title = winxpgui.GetWindowText(hwnd)
        return any(keyword.lower() in window_title.lower() for keyword in window_keywords)
    except Exception:
        return False

# ===============================
# ACTION FUNCTIONS
# ===============================


def click_battle(w, h): click(int(w*0.92), int(h*0.90), "battle")


def click_attack(w, h): click(int(w*0.75), int(h*0.82), "attack")


def click_continue(w, h):
    positions = [(0.65, 0.55), (0.75, 0.85)]
    for i, (sx, sy) in enumerate(positions, start=1):
        click(int(w*sx), int(h*sy), f"continue_{i}")


def click_sell(w, h): click(int(w*0.35), int(h*0.69), "sell")


def click_collect(w, h): click(int(w*0.65), int(h*0.69), "collect")


def click_close(w, h): click(int(w*0.95), int(h*0.15), "close")


def click_skills(w, h):
    skill_positions = [
        (0.1, 0.8), (0.2, 0.8), (0.3, 0.8),
        (0.5, 0.8), (0.7, 0.8), (0.8, 0.8), (0.9, 0.8)
    ]
    for i, (sx, sy) in enumerate(skill_positions, start=1):
        click(int(w*sx), int(h*sy), f"skill_{i}", 0.1)


def click_chests(w, h):
    x_ratios = [0.25, 0.5, 0.75]
    y_ratios = [0.55, 0.75]
    for y in y_ratios:
        for x in x_ratios:
            click(int(w*x), int(h*y), f"chest_{x}_{y}", 0.3)
    log_action("[CHEST] Clicked 6 chests")


def move_character(screen_w, screen_h):
    center_x = screen_w // 2
    center_y = int(screen_h * 0.55)
    move_radius = int(min(screen_w, screen_h) * random.uniform(0.08, 0.15))

    angle_deg = random.randrange(0, 110, 10)
    angle_rad = math.radians(angle_deg)

    target_x = int(center_x + move_radius * math.cos(angle_rad))
    target_y = int(center_y - move_radius * math.sin(angle_rad))

    target_x = max(10, min(target_x, screen_w - 10))
    target_y = max(10, min(target_y, screen_h - 10))

    pyautogui.mouseDown(target_x, target_y)
    time.sleep(10)
    pyautogui.mouseUp(target_x, target_y)
    click_skills(screen_w, screen_h)
    time.sleep(random.uniform(0.2, 0.5))

# ===============================
# ACTION LIST
# ===============================


actions = [
    {"name": "battle", "func": click_battle, "is_clicked": False,
        "expected_screen": "pop_up", "next_steps": ["attack", "collect"]},

    {"name": "attack", "func": click_attack, "is_clicked": False,
        "expected_screen": "game_play", "next_steps": ["move"], "back_steps": ["collect"]},

    {"name": "move", "func": move_character, "is_clicked": False,
        "expected_screen": "continue_button", "next_steps": ["continue"]},

    {"name": "continue", "func": click_continue, "is_clicked": False,
        "expected_screen": "chest_selection", "next_steps": ["chests"]},

    {"name": "chests", "func": click_chests, "is_clicked": False,
        "expected_screen": "sell", "next_steps": ["sell"]},

    {"name": "sell", "func": click_sell, "is_clicked": False,
        "expected_screen": "main", "next_steps": ["battle"]},

    {"name": "collect", "func": click_collect, "is_clicked": False,
        "expected_screen": "main", "next_steps": ["battle"]},
]

action_map = {a["name"]: a for a in actions}


# ===============================
# MAIN BOT LOOP (SMART STATE)
# ===============================

def bot_loop():
    templates = load_templates()
    current_action = "battle"
    log_action("=== BOT SESSION STARTED ===")

    while True:
        if not is_game_active():
            time.sleep(0.5)
            continue

        # Lấy action hiện tại
        action = action_map.get(current_action)
        if not action:
            log_action(
                f"[WARN] Unknown action '{current_action}', reset to battle")
            current_action = "battle"
            continue

        w, h = pyautogui.size()
        log_action(f"[DO] Executing: {action['name']}")

        # Thực thi hành động
        action["func"](w, h)

        # Kiểm tra trạng thái sau khi click
        img = capture_screen()
        detected_screen = detect_screen_type(img, templates)
        expected_screen = action.get("expected_screen", "")

        log_action(
            f"[STATE] Detected: {detected_screen} | Expected: {expected_screen}")

        # Nếu đúng màn → sang bước tiếp theo
        if expected_screen in detected_screen:
            action["is_clicked"] = True
            action["retries"] = 0
            next_steps = action.get("next_steps", [])
            if next_steps:
                current_action = next_steps[0]
            log_action(f"[NEXT] {action['name']} → {current_action}")

        # Nếu sai màn → thử lùi bước nếu có back_steps
        elif "back_steps" in action and any(step in detected_screen for step in action["back_steps"]):
            prev = action["back_steps"][0]
            current_action = prev
            log_action(f"[ROLLBACK] '{action['name']}' rolled back → {prev}")
            break
        # Nếu không đúng cũng không lùi được → retry hoặc reset
        else:
            action["retries"] = action.get("retries", 0) + 1
            if action["retries"] > 3:
                log_action(
                    f"[FAIL] {action['name']} stuck after {action['retries']} retries → reset to battle")
                current_action = "battle"
                action["retries"] = 0
            else:
                log_action(
                    f"[RETRY] Retrying {action['name']} (#{action['retries']})")

        time.sleep(CHECK_INTERVAL * 0.8)


# ===============================
# RUN
# ===============================
if __name__ == "__main__":
    try:
        bot_loop()
    except KeyboardInterrupt:
        log_action("[EXIT] Bot stopped by user.")
