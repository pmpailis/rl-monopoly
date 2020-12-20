from dice import Dice
from state import GameState


class Monopoly(object):
    """
    Main class for the Monopoly game-engine handling all player actions and game state updates.
    The rules followed in this game are defined in the official rules at https://www.hasbro.com/common/instruct/monins.pdf

    Support a callable after all actions taken so that any attached UI can update its state as well.
    """
    
    __monopoly = None
    
    def __init__(self):
        if Monopoly.__monopoly is not None:
            raise Exception("Creating multiple instances of the Monopoly class is not allowed")
        else:
            Monopoly.__monopoly = self
        self._game_state = GameState()
        self._dice = Dice()

    @staticmethod
    def get_instance():
        if Monopoly.__monopoly is None:
            Monopoly()
        return Monopoly.__monopoly

    """ GAME ENGINE """
    def start(self):
        self.init_game()
        while not self.game_is_finished():
            self.prepare_round()
            self.play_round()
            self.post_round()
        self.game_completed()

    def init_game(self):
        pass

    def play_round(self):
        self.player_move()
        self.player_action()
        self.property_bidding()
        self.player_build()

    def game_is_finished(self) -> bool:
        pass

    def prepare_round(self):
        pass

    def player_move(self):
        dice_roll = self._dice.roll_dice()
        self.move_current_player_to_position(dice_roll)

    def player_action(self):
        if self.is_owned_by_other():
            self.pay_owner_rent()

    def property_bidding(self):
        pass

    def player_build(self):
        pass

    def post_round(self):
        pass

    def game_completed(self):
        pass

    """ ACTIONS """
    def move_current_player_to_place(self, place):
        pass

    def move_current_player_to_position(self, position):
        pass

    def move_current_player_group(self, movement_type, movement_group):
        pass

    def is_owned_by_other(self) -> bool:
        pass

    def pay_owner_rent_by_dice(self, factor):
        pass

    def pay_owner_rent(self, factor=1):
        pass

    def collect_from_bank(self, amount):
        pass
    
    def collect_from_players(self, amount):
        pass

    def pay_per_building(self, house_cost, hotel_cost):
        pass

    def pay_to_bank(self, amount):
        pass

    def pay_to_players(self, amount):
        pass

    def send_to_jail(self):
        pass
