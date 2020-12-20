import json

from typing import List, Tuple

from player import Player


class GameState(object):

    def __init__(self, *, players: List[Player] = None):
        """
        Constructor for the singleton (per game) GameState object

        :param players: a list of all the players present in this game
        """
        self._players_info = None
        self._properties = None
        self._buildings = None
        self._mortgages = None
        self._current_player = None

    def get_current_player_info(self) -> Tuple[Player, int]:
        return self._current_player, self._players_info[self._current_player].position

    def to_json(self):
        return json.dumps(self)

