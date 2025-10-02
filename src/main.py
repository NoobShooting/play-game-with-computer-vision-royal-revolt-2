import cv2
import mss
import numpy as np
import pyautogui
import time
import os
import json

from step import Step

pyautogui.FAILSAFE = True  # di chuột ra góc màn hình để dừng bot

DELAY = 1.5  # thời gian nghỉ giữa 2 lần click

constPath = "src/images/"


def click_template_image(step: Step, threshold=0.7):
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
    """
    Tìm target trên màn hình bằng template matching
    """
    # đọc template
    template = cv2.imread(full_path, cv2.IMREAD_COLOR)  # ép về BGR 3 kênh
    if template is None:
        raise FileNotFoundError(f"Không tìm thấy template: {full_path}")

    # convert cả 2 sang grayscale để dễ so sánh
    screen_gray = cv2.cvtColor(game_screenshot, cv2.COLOR_BGR2GRAY)
    template_gray = cv2.cvtColor(template, cv2.COLOR_BGR2GRAY)

    res = cv2.matchTemplate(screen_gray, template_gray, cv2.TM_CCOEFF_NORMED)
    loc = np.where(res >= threshold)
    points = list(zip(*loc[::-1]))
    # img_name = constPath + "sct_{width}x{height}.png"
    # game_screenshot_path = img_name.format(**monitor)
    # sct_img = sct.grab((0, 0, monitor["width"], monitor["height"]))
    # mss.tools.to_png(sct_img.rgb, sct_img.size,
    #                  output=game_screenshot_path)

    # tìm target

    # click nếu tìm thấy

    if points:
        template_image = cv2.imread(full_path, 1)
        w = template_image.shape[1]
        h = template_image.shape[0]
        x, y = points[0]
        x /= width_reset_multiplier
        y /= height_reset_multiplier
        x_c = int((x + x + w) // 2)
        y_c = int((y + y + h) // 2)
        print("Trying to click " + full_path)
        pyautogui.click(x=x_c, y=y_c)
        time.sleep(0.5)
        return click_template_image(step)
    elif not points:
        print("Not found!")
        return points
    print(full_path + " is clicked!")
    time.sleep(DELAY)
    return points


print("Bot started... Nhấn CTRL+C để dừng")
# Đọc JSON
with open("src/gameplayconfig.json", "r") as f:
    data = json.load(f)

print("Step:", data["step"])
print("Next available step:", data["next_available_step"])


def step_searching(step_name):
    img_path = os.path.join(constPath, step_name)
    step = Step(img_path)
    print(step_name + " searching")
    return click_template_image(step)


# Lặp qua next steps
while True:
    for parent_step in data["step"]:
        if not step_searching(parent_step):
            for next_step in data["next_available_step"]:
                step_searching(next_step)
