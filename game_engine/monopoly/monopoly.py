from functools import wraps
from typing import List, Dict, Any, Callable

from monopoly.dice import Dice
from monopoly.game_cards import GameCardProperties, GameCardCommands
from monopoly.board import Board, BOARD_POSITIONS
from monopoly.player import Player
from monopoly.state import GameState
from ui.ui import UserInterface

import logging

logging.basicConfig(filename='monopoly.log', format='%(asctime)s %(message)s', datefmt='%m/%d/%Y %I:%M:%S %p', level=logging.DEBUG)


class Monopoly(object):
    """
    Main class for the Monopoly game-engine handling all player actions and game state updates.
    The rules followed in this game are defined in the official rules at https://www.hasbro.com/common/instruct/monins.pdf

    Support a callable after all actions taken so that any attached UI can update its state as well.
    """

    __monopoly = None

    def __init__(self, *, conf_dir: str = None, players: List[Player] = None, ui: UserInterface = None, resources_path: str = None):
        """
        Constructor for the singleton monopoly class.

        :param conf_dir: the directory containing all configuration for the current game
        :param players: the players that will be part of this game
        :param ui: the user interface instance is to be used
        :param resources_path: the absolute path where the properties.json and commands.json files are stored
        """
        if Monopoly.__monopoly is not None:
            raise Exception("Creating multiple instances of the Monopoly class is not allowed")
        else:
            Monopoly.__monopoly = self
        self._ui = ui
        self._dice = Dice()
        self._game_state = GameState(players=players)
        self._board = Board(resources_path=resources_path)
        self._round = 0

    @staticmethod
    def get_instance():
        if Monopoly.__monopoly is None:
            Monopoly()
        return Monopoly.__monopoly

    def state_step(method: Callable):
        @wraps(method)
        def _impl(self, *method_args, **method_kwargs):
            res = method(self, *method_args, **method_kwargs)
            self._ui.print_state(self._game_state)
            return res
        return _impl

    """ GAME ENGINE """
    def start(self):
        self.init_game()
        while not self.game_is_finished():
            self.prepare_round()
            self.play_round()
            self.post_round()
        self.game_completed()

    @state_step
    def init_game(self):
        logging.info("Starting game!")
        self._round = 0
        self._ui.init_game(self._game_state)
        self._game_state.init_players()

    @state_step
    def play_round(self):
        self.player_move()
        self.player_action()

    def game_is_finished(self) -> bool:
        return self._round > 100

    def prepare_round(self):
        logging.info("Round: " + str(self._round) \
                     + " - Currently playing " + str(self._game_state.get_current_player().get_name()) \
                     + " at position: " + str(self._game_state.get_current_player_position()))

    @state_step
    def player_move(self):
        logging.info("Moving player")
        dice_roll = self._dice.roll_dice()
        logging.info("Rolled " + str(dice_roll))
        self._game_state.move_player(sum(dice_roll))

    def player_action(self):
        current_position = self._game_state.get_current_player_position()
        if self._board.is_property(current_position):
            self.player_action_on_property(current_position)
        elif self._board.is_game_card(current_position):
            self.apply_game_card(current_position)
        elif self._board.is_special(current_position):
            pass
        self.player_build()

    def player_action_on_property(self, position: int):
        if self.is_owned_by_other():
            logging.info("Property '" + self._board.get_property(position)[GameCardProperties.DISPLAY_NAME] + "' owned by " + str(self._game_state.get_owner().get_name()))
            self.pay_owner_rent()
        else:
            bought = self.player_buys()
            if bought:
                logging.info("Property bought by " + str(self._game_state.get_current_player().get_name()))
            else:
                self.start_bidding_war()

    def apply_game_card(self, position):
        card = self._board.draw_card(position)
        logging.info("Card to be applied: " + str(card[GameCardCommands.TEXT]))
        self.apply_card(card)

    def player_build(self):
        pass

    def post_round(self):
        self._game_state.next_player()
        self._round += 1

    def game_completed(self):
        pass

    """ ACTIONS """
    def move_to_nearest_group(self, movement_group):
        logging.info("Moving player to nearest " + str(movement_group))
        group_properties = self._board.get_group_properties(movement_group)
        current_position = self._game_state.get_current_player_position()
        min_distance = BOARD_POSITIONS + 1
        for group_property in group_properties:
            distance = int(group_property[GameCardProperties.POSITION]) - current_position
            logging.info("Distance of " + str(group_property[GameCardProperties.DISPLAY_NAME]) + " is " + str(distance))
            if distance < 0:
                distance += BOARD_POSITIONS
            if distance < min_distance:
                position_to_move = int(group_property[GameCardProperties.POSITION])
                min_distance = distance
        if position_to_move is None:
            raise ValueError("Failed to find position to move to")
        logging.info("Closest property to move to is " + self._board.get_property(position_to_move)[GameCardProperties.DISPLAY_NAME] + " at position " + str(position_to_move))
        self._game_state.move_player_to(position_to_move)

    def is_owned_by_current_player(self) -> bool:
        current_position = self._game_state.get_current_player_position()
        if not self._board.is_property(current_position):
            return False
        owner = self._game_state.get_owner(current_position)
        return owner == self._game_state.get_current_player()

    def pay_owner_rent_by_dice(self, factor):
        dice_roll = self._dice.roll_dice()
        current_player = self._game_state.get_current_player()
        current_position = self._game_state.get_current_player_position()
        owner = self._game_state.get_owner(current_position)
        if owner is None:
            return
        amount = sum(dice_roll) * factor
        current_player.add_amount(-amount)
        owner.add_amount(amount)

    def pay_owner_rent(self, factor=1):
        current_position = self._game_state.get_current_player_position()
        owner = self._game_state.get_owner(current_position)
        if owner is None:
            return

        rent = self._board.get_property(current_position)[GameCardProperties.RENT]
        logging.info("Rent for property: " + str(rent))
        rent_level = self._game_state.get_rent_level(current_position)
        amount = rent[rent_level] * factor
        logging.info("Paying " + str(amount) + ' to ' + str(owner.get_name()))
        current_player = self._game_state.get_current_player()
        current_player.add_amount(-amount)
        owner.add_amount(amount)

    def collect_from_bank(self, amount):
        self._game_state.get_current_player().add_amount(amount)

    def collect_from_players(self, amount):
        current_player = self._game_state.get_current_player()
        for player in self._game_state.get_players():
            if current_player == player:
                continue
            player.add_amount(-amount)
            current_player.add_amount(amount)

    def pay_per_building(self, house_cost, hotel_cost):
        current_player = self._game_state.get_current_player()
        total_buildings = [current_player.buildings[x] for x in current_player.buildings]
        amount = 0
        for building in total_buildings:
            if 0 < building < 5:
                amount += building * house_cost
            elif building == 5:
                amount += hotel_cost
        self.pay_to_bank(amount)

    def pay_to_bank(self, amount):
        self._game_state.get_current_player().add_amount(-amount)

    def pay_to_players(self, amount):
        current_player = self._game_state.get_current_player()
        for player in self._game_state.get_players():
            if current_player == player:
                continue
            player.add_amount(amount)
            current_player.add_amount(-amount)

    def send_to_jail(self):
        self._game_state.get_current_player().in_jail = True

    def apply_card(self, card):
        if GameCardCommands.MOVEMENT in card:
            self._apply_movement_card(card)
        elif GameCardCommands.TRANSACTION in card:
            self._apply_transaction_card(card)
        elif GameCardCommands.SPECIAL in card:
            self._apply_special_card(card)

    def _apply_movement_card(self, conf: Dict[str, Any]):
        movement_conf = conf[GameCardCommands.MOVEMENT]
        if GameCardCommands.MOVEMENT_FIXED in movement_conf:
            position = self._board.get_position(movement_conf[GameCardCommands.MOVEMENT_FIXED])
            self._game_state.move_player_to(position)
        elif GameCardCommands.MOVEMENT_RELATIVE in movement_conf:
            relative_movement_conf = movement_conf[GameCardCommands.MOVEMENT_RELATIVE]
            if GameCardCommands.MOVEMENT_RELATIVE_POSITION in relative_movement_conf:
                position = relative_movement_conf[GameCardCommands.MOVEMENT_RELATIVE_POSITION]
                self._game_state.move_player(position)
            else:
                movement_type = relative_movement_conf[GameCardCommands.MOVEMENT_RELATIVE_TYPE]
                movement_group = relative_movement_conf[GameCardCommands.MOVEMENT_RELATIVE_GROUP]
                if movement_type == "nearest":
                    self.move_to_nearest_group(movement_group)
                if not self.is_owned_by_current_player():
                    transaction_conf = conf[GameCardCommands.TRANSACTION]
                    if GameCardCommands.TRANSACTION_SPECIAL in transaction_conf:
                        amount_source = transaction_conf[GameCardCommands.TRANSACTION_SPECIAL][GameCardCommands.TRANSACTION_SPECIAL_AMOUNT_SOURCE]
                        factor = transaction_conf[GameCardCommands.TRANSACTION_SPECIAL][GameCardCommands.TRANSACTION_SPECIAL_FACTOR]
                        if "dice" == amount_source:
                            self.pay_owner_rent_by_dice(factor)
                        elif "rent" == amount_source:
                            self.pay_owner_rent(factor)

    def _apply_transaction_card(self, conf: Dict[str, Any]):
        transaction_conf = conf[GameCardCommands.TRANSACTION]
        amount = transaction_conf[GameCardCommands.TRANSACTION_AMOUNT] if GameCardCommands.TRANSACTION_AMOUNT in transaction_conf else 0
        transaction_type = transaction_conf[GameCardCommands.TRANSACTION_TYPE] if GameCardCommands.TRANSACTION_TYPE in transaction_conf else None
        if "collect" == transaction_type:
            source = transaction_conf[GameCardCommands.TRANSACTION_SOURCE]
            if "bank" == source:
                self.collect_from_bank(amount)
            elif "players" == source:
                self.collect_from_players(amount)
        elif "pay" == transaction_type:
            destination = transaction_conf[GameCardCommands.TRANSACTION_DESTINATION]
            house_cost = transaction_conf[GameCardCommands.TRANSACTION_HOUSE_COST] if GameCardCommands.TRANSACTION_HOUSE_COST in transaction_conf else None
            hotel_cost = transaction_conf[GameCardCommands.TRANSACTION_HOTEL_COST] if GameCardCommands.TRANSACTION_HOTEL_COST in transaction_conf else None
            if "bank" == destination:
                if house_cost and hotel_cost:
                    self.pay_per_building(house_cost=house_cost, hotel_cost=hotel_cost)
                else:
                    self.pay_to_bank(amount)
            elif "players" == destination:
                self.pay_to_players(amount)

    def _apply_special_card(self, conf: Dict[str, Any]):
        special_conf = conf[GameCardCommands.SPECIAL]
        if GameCardCommands.SPECIAL_JAIL in special_conf:
            self.send_to_jail()

    def is_owned_by_other(self):
        owner = self._game_state.get_owner(self._game_state.get_current_player_position())
        return owner is not None and owner != self._game_state.get_current_player()

    def player_buys(self):
        return False

    def start_bidding_war(self):
        pass
