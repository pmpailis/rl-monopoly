import json

from typing import List

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

    @classmethod
    def move_current_player_place(cls, place):
        pass

    @classmethod
    def move_current_player_position(cls, position):
        pass

    @classmethod
    def move_current_player_group(cls, movement_type, movement_group):
        pass

    @classmethod
    def collect_from_bank(cls, amount):
        pass

    @classmethod
    def collect_from_players(cls, amount):
        pass

    @classmethod
    def pay_to_bank(cls, amount):
        pass

    @classmethod
    def pay_to_players(cls, amount):
        pass

    @classmethod
    def pay_per_building(cls, house_cost, hotel_cost):
        pass

    @classmethod
    def pay_owner_by_dice(cls, factor):
        pass

    @classmethod
    def is_owned_by_other(cls):
        pass

    @classmethod
    def pay_owner_rent_by_dice(cls, factor):
        pass

    @classmethod
    def pay_owner_rent(cls, factor):
        pass

    @classmethod
    def send_to_jail(cls):
        pass
