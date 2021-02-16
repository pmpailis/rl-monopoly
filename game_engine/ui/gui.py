import abc
from ui.ui import UserInterface


class GraphicalUserInterface(metaclass=abc.ABCMeta):

    def __init__(self):
        super().__init__()
