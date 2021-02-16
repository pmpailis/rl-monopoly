import os

from monopoly.monopoly import Monopoly
from monopoly.player import Player
from ui.cli import CommandLineInterface

GAME_RESOURCES = os.path.join(os.path.dirname(os.path.abspath(__file__)), "game_resources")

if __name__ == '__main__':
    players = [Player("dinbur")]
    ui = CommandLineInterface()
    monopoly = Monopoly(players=players, ui=ui, resources_path=GAME_RESOURCES)
    monopoly.start()
