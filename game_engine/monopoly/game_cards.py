from typing import Any, Dict


class GameCardCommands:

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


class GameCardProperties:

    NAME = "name"
    DISPLAY_NAME = "display_name"
    POSITION = "position"
    PRICE = "price"
    RENT = "rent"
    MORTGAGE = "mortgage"
    HOUSE_COST = "house_cost"
    HOTEL_COST = "hotel_cost"
    GROUP = "group"
    TYPE = "type"


class GameCard(object):

    def __init__(self, conf: Dict[str, Any]):
        self._conf = conf
