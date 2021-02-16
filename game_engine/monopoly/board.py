import json
import os

from random import shuffle
from monopoly.game_cards import GameCardProperties, GameCard, GameCardCommands

BOARD_POSITIONS = 40
PROPERTIES = "properties"
CHANCE = "chance"
COMMUNITY_CHEST = "community_chest"
SPECIAL = "special"
POSITIONS = "positions"


class Board(object):

    def __init__(self, resources_path):
        self._properties = self.load_properties(resources_path)
        self._community_chest_cards, self._chance_cards = self.load_commands(resources_path)
        shuffle(self._community_chest_cards)
        shuffle(self._chance_cards)
        self._properties_positions, self._chance_positions, self._community_chest_positions, self._special_positions = self.load_board(resources_path)

    def is_property(self, position):
        return position in self._properties_positions

    def is_game_card(self, position):
        return position in self._chance_positions or position in self._community_chest_positions

    def is_special(self, position):
        return position in self._special_positions

    @staticmethod
    def load_properties(data_path):
        properties_file = os.path.join(data_path, "properties.json")
        with open(properties_file, 'r') as f:
            return json.load(f)

    @staticmethod
    def load_commands(data_path):
        commands_file = os.path.join(data_path, "commands.json")
        with open(commands_file, 'r') as f:
            commands = json.load(f)
            return [x for x in commands if x[GameCardCommands.CARD_TYPE] == "community_chest"], [x for x in commands if x[GameCardCommands.CARD_TYPE] == "chance"]

    @staticmethod
    def load_board(data_path):
        board_file = os.path.join(data_path, "board.json")
        with open(board_file, 'r') as f:
            board = json.load(f)
            return board[PROPERTIES][POSITIONS], board[CHANCE][POSITIONS], board[COMMUNITY_CHEST][POSITIONS], board[SPECIAL][POSITIONS]

    def draw_card(self, position):
        if position in self._community_chest_positions:
            return self.draw_community_chest()
        elif position in self._chance_positions:
            return self.draw_chance_card()
        raise ValueError("Position not valid for drawing a game card")

    def draw_community_chest(self) -> GameCard:
        card = self._community_chest_cards.pop(0)
        self._community_chest_cards.append(card)
        return card

    def draw_chance_card(self) -> GameCard:
        card = self._chance_cards.pop(0)
        self._chance_cards.append(card)
        return card

    def get_group_properties(self, property_group):
        return [x for x in self._properties if x[GameCardProperties.TYPE] == property_group]

    def get_property(self, position):
        return [x for x in self._properties if x[GameCardProperties.POSITION] == position][0]

    def get_position(self, property_name):
        if property_name == 'go':
            return 0
        return [x[GameCardProperties.POSITION] for x in self._properties if x[GameCardProperties.NAME] == property_name][0]
