import json
import logging

from typing import List, Tuple

from monopoly.board import BOARD_POSITIONS
from monopoly.dice import Dice
from monopoly.player import Player


class GameState(object):

    def __init__(self, *, players: List[Player] = None):
        """
        Constructor for the singleton (per game) GameState object

        :param players: a list of all the players present in this game
        """
        self._players = players
        self._properties = None
        self._houses = 32
        self._hotels = 12
        self._mortgages = None
        self._current_player = 0

    def to_json(self):
        return json.dumps(self)

    def get_players(self) -> List[Player]:
        return self._players

    def get_current_player(self) -> Player:
        return self._players[self._current_player]

    def get_current_player_position(self) -> int:
        return self._players[self._current_player].position

    def init_players(self) -> None:
        for player in self.get_players():
            player.add_amount(1500)

    def move_player(self, position: int):
        current_position = self.get_current_player_position()
        next_position = (int(current_position) + int(position)) % BOARD_POSITIONS
        logging.info("Moving player to position: " + str(next_position))
        self.get_current_player().move_to(next_position)

    def move_player_to(self, position):
        logging.info("Moving player to position: " + str(position))
        self.get_current_player().move_to(position)

    def next_player(self):
        self._current_player = (self._current_player + 1) % len(self._players)

    def player_in_jail(self):
        return self.get_current_player().in_jail

    def get_owner(self, position):
        owner = [x for x in self._players if x.owns(position)]
        return owner[0] if owner else None

    def get_rent_level(self, position):
        return self.get_owner(position).buildings[position]
