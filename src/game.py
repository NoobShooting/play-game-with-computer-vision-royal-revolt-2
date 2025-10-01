import cv2
import numpy as np
import pyautogui
import mss
from time import sleep

pyautogui.FAILSAFE = False

sct = mss.mss()
default_monitor = sct.monitors[
    1
]


def multi_scale_template_match(screenshot, template, scales=None):
    if scales is None:
        scales = [0.5, 0.6, 0.7, 0.8, 0.9, 1.0]
    best_val = -1
    best_loc = None
    best_template = None
    best_result = None
    for scale in scales:
        resized = cv2.resize(template, None, fx=scale,
                             fy=scale, interpolation=cv2.INTER_LINEAR)
        result = cv2.matchTemplate(screenshot, resized, cv2.TM_CCOEFF_NORMED)
        min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(result)
        if max_val > best_val:
            best_val = max_val
            best_loc = max_loc
            best_template = resized
            best_result = result
    return best_val, best_loc, best_template, best_result


def click_template_image(template_image_path: str, monitor=default_monitor, threshold: float = 0.7, number_of_clicks: int = 1):
    print(f"{template_image_path} search")
    template_image = cv2.imread(template_image_path, 1)
    game_screenshot = np.array(
        sct.grab((0, 0, monitor["width"], monitor["height"])))
    game_screenshot = game_screenshot[:, :, :3]
    max_val, max_loc, template_resized, search_result = multi_scale_template_match(
        game_screenshot, template_image)
    y_coords, x_coords = np.where(search_result >= threshold)
    width_reset_multiplier = game_screenshot.shape[1] / monitor["width"]
    height_reset_multiplier = game_screenshot.shape[0] / monitor["height"]
    w = template_image.shape[1]
    h = template_image.shape[0]
    for idx in range(number_of_clicks):
        if idx + 1 > len(x_coords):
            continue
        x, y = x_coords[idx], y_coords[idx]
        x /= width_reset_multiplier
        y /= height_reset_multiplier
        x_c = int((x + x + w) // 2)
        y_c = int((y + y + h) // 2)
        for _ in range(3):
            pyautogui.click(x=x_c, y=y_c)
            sleep(0.3)
            pyautogui.click(x=x_c, y=y_c)
        print("Item clicked")
        sleep(0.3)


constPath = "src/images/"

close_buttons = ["close.png"]


class Step:
    def __init__(self, name: str):
        self.name = name


steps = [
    Step("join_battle.png"),
    Step("attack.png"),
    Step("collect.png"),
    Step("spawn.png"),
]


while True:
    for step in steps:
        # Lặp cho đến khi step hoàn thành
        while True:
            click_template_image(constPath + step.name)
            # Kiểm tra đã click thành công step này chưa
            template_image = cv2.imread(constPath + step.name, 1)
            game_screenshot = np.array(
                sct.grab((0, 0, default_monitor["width"], default_monitor["height"])))
            game_screenshot = game_screenshot[:, :, :3]
            max_val, _, _, _ = multi_scale_template_match(
                game_screenshot, template_image)
            if max_val < 0.7:
                print(f"Step {step.name} đã hoàn thành!")
                break
            else:
                print(f"Step {step.name} chưa hoàn thành, thử lại...")
