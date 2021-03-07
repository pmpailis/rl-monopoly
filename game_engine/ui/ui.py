import abc
from functools import wraps

from monopoly.state import GameState


class UserInterface(abc.ABC):

    def __init__(self):
        pass

    @abc.abstractmethod
    def init_game(self, game_state: GameState):
        """
        prepare the UI when the game starts

        :param game_state: the current state of the monopoly game
        """
        pass

    @abc.abstractmethod
    def print_state(self, game_state: GameState):
        pass

    @abc.abstractmethod
    def player_property_buys(self, game_state: GameState, position: int):
        pass
