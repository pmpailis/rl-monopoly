import logging
from typing import Dict

from monopoly.game_cards import GameCardProperties


class Player(object):

    def __init__(self, name):
        self._name = name
        self._position = 0
        self._money = 0
        self._in_jail = False
        self._properties = []
        self._buildings = {}

    def move_to(self, position: int) -> None:
        logging.info("Moving player '" + self._name + "' to position " + str(position))
        self._position = int(position)

    def update_amount(self, amount: int) -> None:
        logging.info(("Adding " if int(amount) > 0 else "Subtracting ") + str(amount) + " to player " + str(self._name))
        self._money += amount
        logging.info("Total money for " + self._name + " " + str(self._money))

    def owns_property(self, position: int) -> bool:
        return position in [x[GameCardProperties.POSITION] for x in self._properties]

    def get_name(self) -> str:
        return self._name

    def get_position(self) -> int:
        return self._position

    def is_in_jail(self) -> bool:
        return self._in_jail

    def get_buildings(self) -> Dict:
        return self._buildings

    def has_sufficient_funds(self, amount: int) -> bool:
        return self._money >= amount

    def get_rent_level(self, position):
        return self._buildings[position]

    def set_in_jail(self, in_jail: bool) -> None:
        self._in_jail = in_jail

    def print(self) -> str:
        return self._name + " - $" + str(self._money) + " - at position: " + str(self._position)
