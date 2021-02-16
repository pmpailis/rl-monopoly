import json
import logging

from typing import List, Tuple

from monopoly.board import BOARD_POSITIONS
from monopoly.player import Player

START_MONEY = 1500
PASS_GO_MONEY = 200


class GameState(object):

    def __init__(self, *, players: List[Player] = None):
        """
        Constructor for the singleton (per game) GameState object

        :param players: a list of all the players present in this game
        """
        self._players = players
        self._houses = 32
        self._hotels = 12
        self._mortgages = None
        self._current_player = 0

    def init_players(self) -> None:
        for player in self.get_players():
            player.update_amount(START_MONEY)

    def to_json(self) -> str:
        return json.dumps(self)

    def get_players(self) -> List[Player]:
        return self._players

    def get_current_player(self) -> Player:
        return self._players[self._current_player]

    def get_current_player_position(self) -> int:
        return self._players[self._current_player].get_position()

    def move_player(self, position: int) -> None:
        current_position = self.get_current_player_position()
        next_position = (int(current_position) + int(position))
        if next_position >= BOARD_POSITIONS:
            self.get_current_player().update_amount(amount=PASS_GO_MONEY)
            next_position = next_position % BOARD_POSITIONS
        self.get_current_player().move_to(next_position)

    def move_player_to(self, position: int) -> None:
        if self.get_current_player_position() > position:
            self.get_current_player().update_amount(amount=PASS_GO_MONEY)
        self.get_current_player().move_to(position)

    def next_player(self) -> None:
        self._current_player = (self._current_player + 1) % len(self._players)

    def get_owner(self, position: int) -> Tuple[Player, None]:
        owner = [x for x in self._players if x.owns_property(position)]
        return owner[0] if owner else None
