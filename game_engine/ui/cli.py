from enum import Enum
from functools import wraps
from time import strftime
from datetime import datetime

from monopoly.action import Action
from monopoly.state import GameState
from ui.ui import UserInterface


class LogLevel(Enum):
    DEBUG = 1
    INFO = 2
    WARN = 3
    ERROR = 4


DATE_FORMAT = "%Y-%m-%d %H%M%S%f"
PRINT_FORMAT = "%s - %s - %s"  # DATE - LOG_LEVEL - MESSAGE


def print_message(msg: str, log_level: LogLevel = None) -> None:
    """
    print a message to the console, along with the timestamp and the log-level provided
    :param log_level: the log-level of the message to be printed
    :param msg: the message to be printed
    """
    date_str = datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
    print(PRINT_FORMAT % (date_str, log_level if log_level else "", msg))


class CommandLineInterface(UserInterface):

    def __init__(self):
        super().__init__()

    def init_game(self, game_state):
        print_message("*" * 30)
        print_message("Welcome to the Monopoly Game!")
        print_message("*" * 30)

    def print_state(self, game_state: GameState):
        print_message("\tPlayers:" + "\n\t\t".join(["%d - %s" % (i + 1, x.print()) for i, x in enumerate(game_state.get_players())]))

    def player_property_buys(self, game_state: GameState, position: int):
        print_message("Do you want to buy property: " + str(position))
        return Action.BUY_PROPERTY
