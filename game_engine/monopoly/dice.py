import random
import time
from typing import Tuple

MIN_VALUE = 1
MAX_VALUE = 6


class Dice(object):

    def __init__(self):
        random.seed(time.time)

    @staticmethod
    def roll_dice() -> Tuple[int, int]:
        dice_a = random.randint(MIN_VALUE, MAX_VALUE)
        dice_b = random.randint(MIN_VALUE, MAX_VALUE)
        return dice_a, dice_b
