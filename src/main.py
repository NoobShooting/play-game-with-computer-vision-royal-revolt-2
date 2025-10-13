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
CLICK_DELAY = 0.8

# ===============================
# LOAD TEMPLATE (CACHE)
# ===============================


def load_templates():
    templates = {}
    images = [f for f in os.listdir(
        TEMPLATE_PATH) if f.lower().endswith(('.png', '.jpg', '.jpeg'))]
    for name in images:
        path = os.path.join(TEMPLATE_PATH, f"{name}")
        print(path)
        if not os.path.exists(path):
            print(f"[WARN] Missing template: {path}")
            continue
        img = cv2.imread(path)
        templates[name] = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    print(f"[INFO] Loaded templates: {list(templates.keys())}")
    return templates


# ===============================
# SCREEN CAPTURE
# ===============================
def capture_screen():
    with mss.mss() as sct:
        monitor = sct.monitors[1]
        img = np.array(sct.grab(monitor))
        img = cv2.cvtColor(img, cv2.COLOR_BGRA2BGR)
    return img


# ===============================
# DETECT SCREEN TYPE
# ===============================
def detect_screen_type(img_bgr, templates, threshold=0.4):
    img_gray = cv2.cvtColor(img_bgr, cv2.COLOR_BGR2GRAY)
    best_type = "unknown"
    best_val = 0

    for screen_type, tmpl in templates.items():
        if tmpl is None:
            continue
        res = cv2.matchTemplate(img_gray, tmpl, cv2.TM_CCOEFF_NORMED)
        _, max_val, _, _ = cv2.minMaxLoc(res)
        print(f"[DEBUG] Match {screen_type}: {max_val:.3f}")
        if max_val > best_val:
            best_val = max_val
            best_type = screen_type
    print(best_val)
    return best_type


# ===============================
# CLICK ACTIONS (CỐ ĐỊNH)
# ===============================
def click_battle(screen_w, screen_h):
    x = int(screen_w * 0.92)
    y = int(screen_h * 0.90)
    click(x, y, "Battle button", CLICK_DELAY)


def click_attack(screen_w, screen_h):
    x = int(screen_w * 0.75)
    y = int(screen_h * 0.82)
    click(x, y, "Attack button", CLICK_DELAY)
    time.sleep(CLICK_DELAY)


def click_continue(screen_w, screen_h):
    """Click nút CONTINUE khi popup pause hiện lên."""
    continue_positions = [
        (0.65, 0.55),
        (0.75, 0.85),
    ]
    for i, (sx, sy) in enumerate(continue_positions, start=1):
        x = int(screen_w * sx)
        y = int(screen_h * sy)
        click(x, y, f"Continue button {i}", 0.5, num_clicks=2)


def click_sell(screen_w, screen_h):
    """Click nút Sell khi popup pause hiện lên."""
    x_ratio, y_ratio = 0.35, 0.69
    x = int(screen_w * x_ratio)
    y = int(screen_h * y_ratio)
    click(x, y, "Sell button", 0.3)


def click_close(screen_w, screen_h):
    """Click nút close khi popup pause hiện lên."""
    x_ratio, y_ratio = 0.95, 0.15
    x = int(screen_w * x_ratio)
    y = int(screen_h * y_ratio)
    click(x, y, "Close button", 0.3)


def click_skills(screen_w, screen_h):
    skill_positions = [
        (0.1, 0.8),  # skill 1 - trái ngoài
        (0.2, 0.8),  # skill 2
        (0.3, 0.8),  # skill 3
        (0.5, 0.8),  # skill 4 - giữa
        (0.7, 0.8),  # skill 5
        (0.8, 0.8),  # skill 6
        (0.9, 0.8),  # skill 7 - phải ngoài
    ]

    for i, (sx, sy) in enumerate(skill_positions, start=1):
        x = int(screen_w * sx)
        y = int(screen_h * sy)
        click(x, y, f"skill_{i}", 0.5, num_clicks=2)


def click_chests(screen_w, screen_h):
    """
    Click vào 6 chest theo bố cục 2x3 (3 cột, 2 hàng).
    Các vị trí được tính theo tỉ lệ màn hình.
    """

    chest_positions = []
    x_ratios = [0.25, 0.5, 0.75]   # Cột: trái, giữa, phải
    y_ratios = [0.55, 0.75]        # Hàng: trên, dưới

    # Tạo danh sách (x, y) tỉ lệ
    for y in y_ratios:
        for x in x_ratios:
            chest_positions.append((x, y))

    # Click từng chest
    for i, (sx, sy) in enumerate(chest_positions, start=1):
        x = int(screen_w * sx)
        y = int(screen_h * sy)
        click(x, y, f"chest_{i}", 1)
        time.sleep(0.3)  # delay nhỏ để tránh miss-click

    print(f"[CHEST] Clicked {len(chest_positions)} chests (2x3 grid)")


def move_character(screen_w, screen_h):
    """
    Di chuyển nhân vật bằng cách giữ chuột gần nhân vật (phía trên-phải),
    ngẫu nhiên hướng trong góc 0°–110°, giữ chuột ngắn (1–2s) để tối ưu tốc độ combat.
    """

    # Tâm nhân vật ~ giữa màn hình (lệch nhẹ xuống)
    center_x = screen_w // 2
    center_y = int(screen_h * 0.55)

    # Bán kính di chuyển nhỏ để giữ nhân vật trong khu vực combat
    move_radius = int(min(screen_w, screen_h) * random.uniform(0.08, 0.15))

    # Dò góc ngẫu nhiên trong vùng 0°–110° (vùng phía trước phải)
    angle_deg = random.randrange(0, 110, 10)
    angle_rad = math.radians(angle_deg)

    # Tính tọa độ đích
    target_x = int(center_x + move_radius * math.cos(angle_rad))
    target_y = int(center_y - move_radius * math.sin(angle_rad))

    # Giới hạn trong khung hình
    target_x = max(10, min(target_x, screen_w - 10))
    target_y = max(10, min(target_y, screen_h - 10))

    # Giữ chuột để di chuyển
    pyautogui.mouseDown(target_x, target_y)
    click_skills(screen_w, screen_h)
    time.sleep(5)
    pyautogui.mouseUp(target_x, target_y)

    # Giãn nhịp cực ngắn (0.2–0.5s) để không spam quá nhanh
    time.sleep(random.uniform(0.2, 0.5))


def click(x, y, action_name="unknown", delay=0.2, num_clicks=1):
    """
    Thực hiện click tại tọa độ (x, y) và log lại hành động.

    Args:
        x (int): Tọa độ X trên màn hình.
        y (int): Tọa độ Y trên màn hình.
        action_name (str): Mô tả hành động (vd: 'battle', 'attack', 'skill_1', ...).
        delay (float): Thời gian nghỉ sau click (giây).
    """
    if is_game_active():
        try:
            for _ in range(num_clicks):
                pyautogui.click(x, y)
            print(f"[CLICK] {action_name.upper()} at ({x}, {y})")
            time.sleep(delay)
        except Exception as e:
            print(
                f"[ERROR] Click failed at ({x}, {y}) for action '{action_name}': {e}")


def is_game_active(window_keywords=["Royal Revolt"]):
    try:
        hwnd = winxpgui.GetForegroundWindow()
        window_title = winxpgui.GetWindowText(hwnd)
        return any(keyword.lower() in window_title.lower() for keyword in window_keywords)
    except Exception:
        return False


# ===============================
# MAIN LOOP
# ===============================


def bot_loop():
    templates = load_templates()
    print("[INFO] Bot started. Press Ctrl+C to stop.")

    while True:
        if not is_game_active():
            time.sleep(1)
            continue

        img = capture_screen()
        h, w = img.shape[:2]
        current = detect_screen_type(img, templates)

        print(f"[STATE] Current screen: {current}")

        if current.__contains__("main"):
            click_battle(w, h)

        elif current.__contains__("pop_up"):
            click_attack(w, h)

        elif current.__contains__("game_play"):
            move_character(w, h)

        elif current.__contains__("continue_button"):
            click_continue(w, h)

        elif current.__contains__("chest_selection"):
            click_chests(w, h)

        elif current.__contains__("sell"):
            click_sell(w, h)

        else:
            click_close(w, h)
            print("[DEBUG] Unknown screen → skip")

        time.sleep(CHECK_INTERVAL)


# ===============================
# RUN
# ===============================
if __name__ == "__main__":
    try:
        bot_loop()
    except KeyboardInterrupt:
        print("\n[EXIT] Bot stopped by user.")
