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
    CELL = 'cell'
    CHEST = 'chest'


class Step:
    def __init__(self, name: str, is_clicked: bool = False):
        self.name = name
        self.is_clicked = is_clicked


constPath = "src/images/"
pyautogui.FAILSAFE = True
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
    full_path = os.path.join(constPath, step)

    template = cv2.imread(full_path, cv2.IMREAD_COLOR)

    sct = mss.mss()
    monitor = sct.monitors[1]
    screenshot = np.array(
        sct.grab((0, 0, monitor["width"], monitor["height"])))
    screenshot = screenshot[:, :, :3]
    screen_gray = cv2.cvtColor(screenshot, cv2.COLOR_BGR2GRAY)
    template_gray = cv2.cvtColor(template, cv2.COLOR_BGR2GRAY)

    points = multi_scale_match(screenshot, template, threshold)

    if points:
        w, h = template.shape[1], template.shape[0]
        x, y = points[0]
        x_c = int(x + w // 2)
        y_c = int(y + h // 2)
        return [(x_c, y_c)]
    return []


def move_in_arc(monitor,
                distance=300,
                start_offset=-20, end_offset=40,
                step=5, speed=0.02):
    """
    Di chuyển chuột theo vòng cung nhỏ phía trước nhân vật (phía trên màn hình - trục Y dương).
    - distance: bán kính cung (độ dài “tay vươn”)
    - start_offset / end_offset: góc lệch so với hướng trước (90°)
    - step: bước góc (mịn cung)
    - speed: thời gian giữa mỗi bước (tốc độ di chuyển)
    """
    char_x, char_y = monitor["width"] // 2, monitor["height"] // 2 + 100

    base_angle = 90  # hướng "phía trước" nhân vật (trục Y dương màn hình)

    for offset in range(start_offset, end_offset + 1, step):
        angle = base_angle + offset

        # Tính vị trí chuột theo góc lệch
        target_x = int(char_x + distance * math.cos(math.radians(angle)))
        target_y = int(char_y - distance * math.sin(math.radians(angle)))

        # Random nhẹ để tự nhiên
        target_x += random.randint(-8, 8)
        target_y += random.randint(-8, 8)

        # Giới hạn trong màn hình
        target_x = max(10, min(target_x, monitor["width"] - 10))
        target_y = max(10, min(target_y, monitor["height"] - 10))
        print("Đang di chuyển chuột tới:", (target_x, target_y))
        safe_click(target_x, target_y, number_of_clicks=3)
        time.sleep(speed)


def frange(start, stop, step):
    while start < stop:
        yield start
        start += step


def safe_click(x, y, time_after=1, number_of_clicks=1):
    with pyautogui_lock:
        for _ in range(number_of_clicks):
            pyautogui.click(x=x, y=y)
        time.sleep(time_after)


def action(action_value: str):
    while not stop_event.is_set():
        if not is_game_active():
            time.sleep(1)
            continue
        points = click_template_image(action_value)
        if points:
            x, y = points[0]
            print(f"[STEP] thấy {action_value} ở {(x,y)} -> clicking")
            if ActionType.SKILL.value in action_value:
                move_in_arc(monitor=mss.mss().monitors[1])
            safe_click(x, y, 2)


def create_thread(action_value: str):
    t = threading.Thread(target=action, args=(action_value,), daemon=True)
    t.start()
    return t


def start_threads():
    threads = []

    # actions = [ActionType.CONTINUE, ActionType.BATTLE, ActionType.ATTACK, ActionType.SKIP, ActionType.COLLECT,
    #            ActionType.GEM, ActionType.CHEST, ActionType.SE]
    # for a in actions:
    #     create_thread(a.value)

    # skips = [ActionType.SKILL_1, ActionType.SKILL_2, ActionType.SKILL_3, ActionType.SKILL_5,
    #          ActionType.SKILL_6, ActionType.SKILL_7, ActionType.SKILL_9,
    #          ActionType.SKILL_Q, ActionType.SKILL_W, ActionType.SKILL_E, ActionType.SKILL_0,]
    # for s in skips:
    #     create_thread(s.value)

    images = [f for f in os.listdir(
        constPath) if f.lower().endswith(('.png', '.jpg', '.jpeg'))]
    for s in images:
        create_thread(s)

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
