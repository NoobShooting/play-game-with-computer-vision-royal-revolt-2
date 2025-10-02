import cv2
import mss
import numpy as np
import pyautogui
import time
import os
import json

from step import Step

pyautogui.FAILSAFE = True
DELAY = 1.5
constPath = "src/images/"


def multi_scale_match(screen_gray, template_gray, threshold=0.75, scales=None):
    if scales is None:
        scales = [0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2]
    best_points = []
    for scale in scales:
        resized = cv2.resize(template_gray, None, fx=scale,
                             fy=scale, interpolation=cv2.INTER_LINEAR)
        res = cv2.matchTemplate(screen_gray, resized, cv2.TM_CCOEFF_NORMED)
        loc = np.where(res >= threshold)
        points = list(zip(*loc[::-1]))
        if points:
            best_points = points
            break
    return best_points


def click_template_image(step: Step, threshold=0.75):
    full_path = step.name
    sct = mss.mss()
    monitor = sct.monitors[1]  # màn hình chính
    game_screenshot = np.array(
        sct.grab((0, 0, monitor["width"], monitor["height"])))
    game_screenshot = game_screenshot[:, :, :3]
    width_reset_multiplier = game_screenshot.shape[1] / monitor["width"]
    height_reset_multiplier = game_screenshot.shape[0] / monitor["height"]

    # chụp màn hình
    img = np.array(sct.grab(monitor))
    img = cv2.cvtColor(img, cv2.COLOR_BGRA2BGR)

    template = cv2.imread(full_path, cv2.IMREAD_COLOR)
    if template is None:
        raise FileNotFoundError(f"Không tìm thấy template: {full_path}")

    screen_gray = cv2.cvtColor(game_screenshot, cv2.COLOR_BGR2GRAY)
    template_gray = cv2.cvtColor(template, cv2.COLOR_BGR2GRAY)

    points = multi_scale_match(screen_gray, template_gray, threshold)

    # click nếu tìm thấy
    print("Thử click " + full_path)

    if points and step.is_clicked is False:
        template_image = cv2.imread(full_path, 1)
        w = template_image.shape[1]
        h = template_image.shape[0]
        x, y = points[0]
        x /= width_reset_multiplier
        y /= height_reset_multiplier
        x_c = int((x + x + w) // 2)
        y_c = int((y + y + h) // 2)
        print(full_path + " đã được click!")
        pyautogui.click(x=x_c, y=y_c)
    time.sleep(DELAY)
    return points


def step_searching(step_name):
    img_path = os.path.join(constPath, step_name)
    step = Step(img_path)
    while click_template_image(step) != []:
        step.is_clicked = True
    print("Step:", step.name, "is_clicked:", step.is_clicked)
    return step


print("Bot started... Nhấn CTRL+C để dừng")
# Đọc JSON
with open("src/script.json", "r") as f:
    data = json.load(f)

print("Step:", data["step"])
print("Next available step:", data["next_available_step"])

# Lặp qua next steps
while True:
    for parent_step in data["step"]:
        while step_searching(parent_step).is_clicked is False:
            pass
