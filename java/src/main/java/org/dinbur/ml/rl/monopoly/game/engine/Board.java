package org.dinbur.ml.rl.monopoly.game.engine;

import org.dinbur.ml.rl.monopoly.game.player.Player;

class Board {

    private final Dice dice;

    Board() {
        dice = new Dice();
    }

    public void positionSpecificActions(Player player) {
    }

    public void updatePlayerPosition(Player player) {
        int diceRoll = dice.roll();
    }
}
