from typing import Any, Dict

from state import GameState


class GameCardProperties:
    CARD_TYPE = "card_type"
    TEXT = "text"
    MOVEMENT = "movement"
    MOVEMENT_FIXED = "fixed"
    MOVEMENT_RELATIVE = "relative"
    MOVEMENT_RELATIVE_TYPE = "type"
    MOVEMENT_RELATIVE_GROUP = "group"
    MOVEMENT_RELATIVE_POSITION = "position"
    TRANSACTION = "transaction"
    TRANSACTION_AMOUNT = "amount"
    TRANSACTION_TYPE = "type"
    TRANSACTION_SOURCE = "source"
    TRANSACTION_DESTINATION = "destination"
    TRANSACTION_HOUSE_COST = "house_cost"
    TRANSACTION_HOTEL_COST = "hotel_cost"
    TRANSACTION_SPECIAL = "special"
    TRANSACTION_SPECIAL_AMOUNT_SOURCE = "amount_source"
    TRANSACTION_SPECIAL_FACTOR = "factor"
    SPECIAL = "special"
    SPECIAL_JAIL = "jail"


class GameCard(object):

    def __init__(self, conf: Dict[str, Any]):
        self._conf = conf

    def apply_card(self):
        if GameCardProperties.MOVEMENT in self._conf:
            self._apply_movement_card(self._conf)
        elif GameCardProperties.TRANSACTION in self._conf:
            self._apply_transaction_card(self._conf)
        elif GameCardProperties.SPECIAL in self._conf:
            self._apply_special_card(self._conf)

    def get_card_type(self) -> str:
        return self._conf[GameCardProperties.CARD_TYPE]

    def get_card_text(self) -> str:
        return self._conf[GameCardProperties.TEXT]

    @staticmethod
    def _apply_movement_card(conf: Dict[str, Any]):
        movement_conf = conf[GameCardProperties.MOVEMENT]
        if GameCardProperties.MOVEMENT_FIXED in movement_conf:
            place = movement_conf[GameCardProperties.MOVEMENT_FIXED]
            GameState.move_current_player_place(place)
        elif GameCardProperties.MOVEMENT_RELATIVE in movement_conf:
            relative_movement_conf = movement_conf[GameCardProperties.MOVEMENT_RELATIVE]
            if GameCardProperties.MOVEMENT_RELATIVE_POSITION in relative_movement_conf:
                position = relative_movement_conf[GameCardProperties.MOVEMENT_RELATIVE_POSITION]
                GameState.move_current_player_position(position)
            else:
                movement_type = relative_movement_conf[GameCardProperties.MOVEMENT_RELATIVE_TYPE]
                movement_group = relative_movement_conf[GameCardProperties.MOVEMENT_RELATIVE_GROUP]
                GameState.move_current_player_group(movement_type, movement_group)
                if GameState.is_owned_by_other():
                    transaction_conf = conf[GameCardProperties.TRANSACTION]
                    if GameCardProperties.TRANSACTION_SPECIAL in transaction_conf:
                        amount_source = transaction_conf[GameCardProperties.TRANSACTION_SPECIAL][GameCardProperties.TRANSACTION_SPECIAL_AMOUNT_SOURCE]
                        factor = transaction_conf[GameCardProperties.TRANSACTION_SPECIAL][GameCardProperties.TRANSACTION_SPECIAL_FACTOR]
                        if "dice" == amount_source:
                            GameState.pay_owner_rent_by_dice(factor)
                        elif "rent" == amount_source:
                            GameState.pay_owner_rent(factor)

    @staticmethod
    def _apply_transaction_card(conf: Dict[str, Any]):
        transaction_conf = conf[GameCardProperties.TRANSACTION]
        amount = transaction_conf[GameCardProperties.TRANSACTION_AMOUNT]
        transaction_type = transaction_conf[GameCardProperties.TRANSACTION_TYPE]
        if "collect" == transaction_type:
            source = transaction_conf[GameCardProperties.TRANSACTION_SOURCE]
            if "bank" == source:
                GameState.collect_from_bank(amount)
            elif "players" == source:
                GameState.collect_from_players(amount)
        elif "pay" == transaction_type:
            destination = transaction_conf[GameCardProperties.TRANSACTION_DESTINATION]
            house_cost = transaction_conf[GameCardProperties.TRANSACTION_HOUSE_COST]
            hotel_cost = transaction_conf[GameCardProperties.TRANSACTION_HOTEL_COST]
            if "bank" == destination:
                if house_cost and hotel_cost:
                    GameState.pay_per_building(house_cost=house_cost, hotel_cost=hotel_cost)
                else:
                    GameState.pay_to_bank(amount)
            elif "players" == destination:
                GameState.pay_to_players(amount)

    @staticmethod
    def _apply_special_card(conf: Dict[str, Any]):
        special_conf = conf[GameCardProperties.SPECIAL]
        if GameCardProperties.SPECIAL_JAIL in special_conf:
            GameState.send_to_jail()
