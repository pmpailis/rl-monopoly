from functools import wraps
from typing import List, Dict, Any, Callable, Union

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
        self.rolled_double = 0

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
    def init_game(self) -> None:
        logging.info("Starting game!")
        self._round = 0
        self._ui.init_game(self._game_state)
        self._game_state.init_players()

    @state_step
    def play_round(self) -> None:
        self.player_move()
        self.player_action()

    def game_is_finished(self) -> bool:
        return self._round > 100

    def prepare_round(self) -> None:
        logging.info("Round: " + str(self._round) \
                     + " - Currently playing " + str(self._game_state.get_current_player().get_name()) \
                     + " at position: " + str(self._game_state.get_current_player_position()))

    @state_step
    def player_move(self) -> None:
        logging.info("Moving player")
        dice_roll = self._dice.roll_dice()
        logging.info("Rolled " + str(dice_roll))
        if dice_roll[0] == dice_roll[1]:
            self.rolled_double += 1
        else:
            self.rolled_double = 0
        self._game_state.move_player(sum(dice_roll))

    def player_action(self) -> None:
        current_position = self._game_state.get_current_player_position()
        if self._board.is_property(current_position):
            self.player_action_on_property(current_position)
        elif self._board.is_game_card(current_position):
            self.apply_game_card(current_position)
        elif self._board.is_special(current_position):
            pass
        self.player_build()

    def player_action_on_property(self, position: int) -> None:
        if self.is_owned_by_other(position):
            owner = self._game_state.get_owner(position)
            logging.info("Property '" + self._board.get_property(position)[GameCardProperties.DISPLAY_NAME] + "' is owned by " + str(owner.get_name()))
            self.pay_rent(owner, position)
        else:
            bought = self.player_buys(position)
            if bought:
                logging.info("Property bought by " + str(self._game_state.get_current_player().get_name()))
            else:
                self.start_bidding_war(position)

    def apply_game_card(self, position: int) -> None:
        card = self._board.draw_card(position)
        logging.info("Card to be applied: " + str(card[GameCardCommands.TEXT]))
        self.apply_card(card)

    def player_build(self) -> None:
        pass

    def post_round(self) -> None:
        if self.rolled_double == 0:
            self._game_state.next_player()
        elif self.rolled_double == 3:
            self._game_state.get_current_player().set_in_jail(True)
        self._round += 1

    def game_completed(self) -> None:
        pass

    """ ACTIONS """
    def move_to_nearest_group(self, movement_group: str) -> None:
        logging.info("Moving player to nearest " + str(movement_group))
        group_properties = self._board.get_group_properties(movement_group)
        current_position = self._game_state.get_current_player_position()
        min_distance = BOARD_POSITIONS + 1
        for group_property in group_properties:
            distance = int(group_property[GameCardProperties.POSITION]) - current_position
            if distance < 0:
                distance += BOARD_POSITIONS
            logging.info("Distance of " + str(group_property[GameCardProperties.DISPLAY_NAME]) + " is " + str(distance))
            if distance < min_distance:
                position_to_move = int(group_property[GameCardProperties.POSITION])
                min_distance = distance
        if position_to_move is None:
            raise ValueError("Failed to find position to move to")
        logging.info("Closest property to move to is " + self._board.get_property(position_to_move)[GameCardProperties.DISPLAY_NAME] + " at position " + str(position_to_move))
        self._game_state.move_player_to(position_to_move)

    def pay_owner_rent_by_dice(self, current_player: Player, owner: Player, factor: int) -> None:
        dice_roll = self._dice.roll_dice()
        amount = sum(dice_roll) * factor
        logging.info("Rolled " + str(dice_roll) + " . Amount to be paid is multiplied by " + str(factor))
        self.transaction(current_player, owner, amount)

    def pay_rent(self, current_player: Player, owner: Player, position: int, factor: int = 1) -> None:
        rent = self._board.get_property(position)[GameCardProperties.RENT]
        logging.info("Rent for property: " + str(rent))
        rent_level = owner.get_rent_level(position)
        amount = rent[rent_level] * factor
        self.transaction(source=current_player, destination=owner, amount=amount)

    def transaction(self, source: Union[Player, None], destination: Union[Player, None], amount: int) -> None:
        logging.info((source.get_name() if source is not None else "Bank") + " is ordered to pay " + str(amount) + " to " + (destination.get_name() if destination is not None else "Bank"))
        if source is not None:
            if not source.has_sufficient_funds(amount):
                logging.info("Player " + source.get_name() + " cannot afford paying " + str(amount))
            source.update_amount(-amount)
        if destination is not None:
            destination.update_amount(amount)
            logging.info(str(amount) + " has been credited to " + destination.get_name())

    def collect_from_bank(self, amount) -> None:
        return self.transaction(source=None, destination=self._game_state.get_current_player(), amount=amount)

    def collect_from_players(self, current_player: Player, amount: int) -> None:
        for player in self._game_state.get_players():
            if current_player == player:
                continue
            self.transaction(source=player, destination=current_player, amount=amount)

    def pay_per_building(self, current_player: Player, house_cost: int, hotel_cost: int) -> None:
        total_buildings = [current_player.get_buildings()[x] for x in current_player.get_buildings()]
        amount = 0
        for building in total_buildings:
            if 0 < building < 5:
                amount += (building * house_cost)
            elif building == 5:
                amount += hotel_cost
        self.pay_to_bank(current_player, amount)

    def pay_to_bank(self, current_player: Player, amount: int) -> None:
        self.transaction(source=current_player, destination=None, amount=amount)

    def pay_to_players(self, current_player: Player, amount: int) -> None:
        logging.info("Paying " + str(amount) + " to all others players")
        for player in self._game_state.get_players():
            if current_player == player:
                continue
            self.transaction(source=current_player, destination=player, amount=amount)

    def send_to_jail(self) -> None:
        self._game_state.get_current_player().set_in_jail(True)

    def apply_card(self, card: Dict) -> None:
        if GameCardCommands.MOVEMENT in card:
            self._apply_movement_card(card)
        elif GameCardCommands.TRANSACTION in card:
            self._apply_transaction_card(card)
        elif GameCardCommands.SPECIAL in card:
            self._apply_special_card(card)

    def _apply_movement_card(self, conf: Dict[str, Any]) -> None:
        current_player = self._game_state.get_current_player()
        current_position = self._game_state.get_current_player_position()

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
                if self.is_owned_by_other(position=current_position):
                    transaction_conf = conf[GameCardCommands.TRANSACTION]
                    if GameCardCommands.TRANSACTION_SPECIAL in transaction_conf:
                        amount_source = transaction_conf[GameCardCommands.TRANSACTION_SPECIAL][GameCardCommands.TRANSACTION_SPECIAL_AMOUNT_SOURCE]
                        factor = transaction_conf[GameCardCommands.TRANSACTION_SPECIAL][GameCardCommands.TRANSACTION_SPECIAL_FACTOR]
                        owner = self._game_state.get_owner(position=current_position)
                        if "dice" == amount_source:
                            self.pay_owner_rent_by_dice(current_player=current_player, owner=owner, factor=int(factor))
                        elif "rent" == amount_source:
                            self.pay_rent(current_player=current_player, owner=owner, position=self._game_state.get_current_player_position(), factor=int(factor))
                else:
                    self.player_buys(self._game_state.get_current_player_position())

    def _apply_transaction_card(self, conf: Dict[str, Any]) -> None:
        current_player = self._game_state.get_current_player()
        transaction_conf = conf[GameCardCommands.TRANSACTION]
        amount = int(transaction_conf[GameCardCommands.TRANSACTION_AMOUNT]) if GameCardCommands.TRANSACTION_AMOUNT in transaction_conf else 0
        transaction_type = transaction_conf[GameCardCommands.TRANSACTION_TYPE] if GameCardCommands.TRANSACTION_TYPE in transaction_conf else None
        if "collect" == transaction_type:
            source = transaction_conf[GameCardCommands.TRANSACTION_SOURCE]
            if "bank" == source:
                self.collect_from_bank(amount)
            elif "players" == source:
                self.collect_from_players(current_player=current_player, amount=amount)
        elif "pay" == transaction_type:
            destination = transaction_conf[GameCardCommands.TRANSACTION_DESTINATION]
            house_cost = transaction_conf[GameCardCommands.TRANSACTION_HOUSE_COST] if GameCardCommands.TRANSACTION_HOUSE_COST in transaction_conf else None
            hotel_cost = transaction_conf[GameCardCommands.TRANSACTION_HOTEL_COST] if GameCardCommands.TRANSACTION_HOTEL_COST in transaction_conf else None
            if "bank" == destination:
                if house_cost and hotel_cost:
                    self.pay_per_building(current_player=current_player, house_cost=house_cost, hotel_cost=hotel_cost)
                else:
                    self.pay_to_bank(current_player=current_player, amount=amount)
            elif "players" == destination:
                self.pay_to_players(current_player=current_player, amount=amount)

    def _apply_special_card(self, conf: Dict[str, Any]):
        special_conf = conf[GameCardCommands.SPECIAL]
        if GameCardCommands.SPECIAL_JAIL in special_conf:
            self.send_to_jail()

    def is_owned_by_other(self, position: int):
        owner = self._game_state.get_owner(position)
        return owner is not None and owner != self._game_state.get_current_player()

    def player_buys(self, position):
        self._ui.player_property_buys(position)
        return False

    def start_bidding_war(self, position):
        logging.info("Start bidding war for " + self._board.get_property(position)[GameCardProperties.DISPLAY_NAME])
