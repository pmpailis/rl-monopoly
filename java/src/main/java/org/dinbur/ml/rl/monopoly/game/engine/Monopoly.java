package org.dinbur.ml.rl.monopoly.game.engine;

import org.dinbur.ml.rl.monopoly.game.player.Player;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;

public class Monopoly {

    private final List<Player> players;
    private final Board board;
    private static final Logger log = LoggerFactory.getLogger(Monopoly.class);

    public Monopoly() {
        this.players = new ArrayList<>();
        this.board = new Board();
    }

    public void startGame() {
        prepareNewGame();
        while (!gameIsFinished()) {
            prepareRound();
            playRound();
            postRound();
        }
    }

    private void prepareNewGame() {

    }


    private void prepareRound() {
    }

    private void playRound() {
        for(Player player: players) {
            log.info("Player {} is next", player.toString());
            movePlayer(player);
            positionSpecificActions(player);
            playerSpecificActions(player);
        }
    }

    private void postRound() {
    }

    private boolean gameIsFinished() {
        return false;
    }

    private void movePlayer(Player player) {
        board.updatePlayerPosition(player);
    }

    private void positionSpecificActions(Player player){
        board.positionSpecificActions(player);

    }

    private void playerSpecificActions(Player player){
        player.applyActions();
    }

}
