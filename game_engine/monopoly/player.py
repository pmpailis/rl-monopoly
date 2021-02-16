import logging


class Player(object):

    def __init__(self, name):
        self._name = name
        self.position = 0
        self.money = 0
        self.in_jail = False
        self.properties = []
        self.buildings = {}

    def move_to(self, position: int) -> None:
        self.position = int(position)

    def add_amount(self, amount: int) -> None:
        logging.info(("Adding " if int(amount) > 0 else "Subtracting ") + str(amount) + " to player " + str(self._name))
        self.money += amount

    def owns(self, position):
        return position in [x.position for x in self.properties]

    def get_name(self):
        return self._name

    def print(self):
        return self._name + " - $" + str(self.money) + " - at position: " + str(self.position)
