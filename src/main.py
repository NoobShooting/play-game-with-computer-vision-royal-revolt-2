import math
import random
from enum import Enum
import threading
import time
import pyautogui
import cv2
import mss
import numpy as np
import os
from win32 import winxpgui


class ActionType(Enum):
    BATTLE = 'battle'
    ATTACK = 'attack'
    SKIP = 'skip'
    COLLECT = 'collect'
    CONTINUE = 'continue'
    CLOSE = 'close'
    SKILL = 'skill'
    SKILL_1 = SKILL + '_1'
    SKILL_2 = SKILL + '_2'
    SKILL_3 = SKILL + '_3'
    SKILL_5 = SKILL + '_5'
    SKILL_6 = SKILL + '_6'
    SKILL_7 = SKILL + '_7'
    SKILL_9 = SKILL + '_9'
    SKILL_0 = SKILL + '_0'
    SKILL_E = SKILL + '_e'
    SKILL_Q = SKILL + '_q'
    SKILL_W = SKILL + '_w'
    GEM = 'gem'


constPath = "src/images/"

STEP_POLL_INTERVAL = 1

# Lock để đồng bộ thao tác pyautogui giữa các thread
pyautogui_lock = threading.Lock()

# Biến trạng thái chung
stop_event = threading.Event()


def multi_scale_match(screen_gray, template_gray, threshold=0.8, scales=None):
    if scales is None:
        scales = [0.9, 1.0, 1.1]
    best_val = 0
    best_points = []

    for scale in scales:
        resized = cv2.resize(template_gray, None, fx=scale,
                             fy=scale, interpolation=cv2.INTER_LINEAR)
        res = cv2.matchTemplate(screen_gray, resized, cv2.TM_CCOEFF_NORMED)
        min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(res)
        if max_val > best_val and max_val >= threshold:
            best_val = max_val
            best_points = [max_loc]

    return best_points


def click_template_image(step: str, threshold=0.75):
    """
    Tìm vị trí template trên màn hình.
    Nếu không tìm thấy thì trả về vị trí mặc định (giữa màn hình, lệch +10px).
    """
    full_path = os.path.join(constPath, step) + ".png"

    # --- Đọc template ---
    template = cv2.imread(full_path, cv2.IMREAD_COLOR)

    # --- Chụp màn hình game ---
    sct = mss.mss()
    monitor = sct.monitors[1]  # màn hình chính
    screenshot = np.array(
        sct.grab((0, 0, monitor["width"], monitor["height"])))
    screenshot = screenshot[:, :, :3]  # bỏ alpha
    screen_gray = cv2.cvtColor(screenshot, cv2.COLOR_BGR2GRAY)
    template_gray = cv2.cvtColor(template, cv2.COLOR_BGR2GRAY)

    # --- Match template ---
    points = multi_scale_match(screen_gray, template_gray, threshold)

    # --- Nếu tìm thấy template ---
    if points:
        w, h = template.shape[1], template.shape[0]
        x, y = points[0]
        x_c = int(x + w // 2)
        y_c = int(y + h // 2)
        return [(x_c, y_c)]
    return []


def move_in_arc(monitor, radius_ratio=0.3, start_angle=10, end_angle=60, step_angle=10):
    """
    Di chuyển theo hình vòng cung phía trên bên phải màn hình.

    monitor: thông tin màn hình từ mss (monitor["width"], monitor["height"])
    radius_ratio: tỉ lệ bán kính so với chiều rộng màn hình (0.3 = 30%)
    start_angle: góc bắt đầu (0 = ngang phải, 90 = thẳng lên)
    end_angle: góc kết thúc
    step_angle: bước góc giữa các click
    """

    center_x = int(monitor["width"] * 0.5)
    center_y = int(monitor["height"] * 0.5)
    radius = int(monitor["width"] * radius_ratio)

    print(
        f"[MOVE] Quét cung từ {start_angle}° → {end_angle}° | bán kính {radius}px")

    for angle in range(start_angle, end_angle + 1, step_angle):
        # Tính tọa độ theo góc
        x = int(center_x + radius * math.cos(math.radians(angle)))
        y = int(center_y - radius * math.sin(math.radians(angle)))

        # Thêm độ lệch ngẫu nhiên nhẹ để tự nhiên
        x += random.randint(-15, 15)
        y += random.randint(-15, 15)

        # Giới hạn trong màn hình
        x = max(50, min(x, monitor["width"] - 50))
        y = max(50, min(y, monitor["height"] - 50))

        # Thực hiện click
        safe_click(x, y)
        print(f"[MOVE] Click cung tại ({x}, {y}) góc {angle}°")

        # Delay giữa các click
        time.sleep(random.uniform(0.8, 1.6))

    print("[MOVE] Quét cung hoàn tất.\n")


def safe_click(x, y):
    with pyautogui_lock:
        pyautogui.click(x=x, y=y)


def action(action_value: str):
    while not stop_event.is_set():
        if not is_game_active():
            time.sleep(1)
            continue
        points = click_template_image(action_value)
        if points:
            x, y = points[0]
            print(f"[STEP] thấy {action_value} ở {(x,y)} -> clicking")
            safe_click(x, y)
            time.sleep(3)
            if ActionType.SKIP.value in action_value:
                move_in_arc(monitor=mss.mss().monitors[1])
        else:
            time.sleep(STEP_POLL_INTERVAL)


def start_threads():
    actions = [ActionType.CONTINUE, ActionType.BATTLE, ActionType.ATTACK, ActionType.SKIP, ActionType.COLLECT,
               ActionType.GEM]
    # actions = [ActionType.SKIP]
    skips = [ActionType.SKILL_1, ActionType.SKILL_2, ActionType.SKILL_3, ActionType.SKILL_5,
             ActionType.SKILL_6, ActionType.SKILL_7, ActionType.SKILL_9,
             ActionType.SKILL_Q, ActionType.SKILL_W, ActionType.SKILL_E, ActionType.SKILL_0,]
    threads = []

    for s in actions:
        t = threading.Thread(target=action, args=(s.value,), daemon=True)
        t.start()
        threads.append(t)

    for s in skips:
        t = threading.Thread(target=action, args=(s.value,), daemon=True)
        t.start()
        threads.append(t)
    return threads


def is_game_active(window_keywords=["Royal Revolt"]):
    try:
        hwnd = winxpgui.GetForegroundWindow()
        window_title = winxpgui.GetWindowText(hwnd)
        return any(keyword.lower() in window_title.lower() for keyword in window_keywords)
    except Exception:
        return False


if __name__ == "__main__":
    print("Bot started... Nhấn CTRL+C để dừng")
    try:
        threads = start_threads()
        # giữ main chạy
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("Stopping bot...")
        stop_event.set()
        time.sleep(1)
        print("Stopped.")
