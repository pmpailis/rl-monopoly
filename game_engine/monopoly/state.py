import json

from typing import List

from monopoly.player import Player


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

    def to_json(self):
        return json.dumps(self)
