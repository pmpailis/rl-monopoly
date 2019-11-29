package org.dinbur.ml.rl.monopoly.game.player;

import java.util.Objects;

public class Player {

    private final String name;
    private final String id;
    private final String game;

    public Player(final String id, String name, String game) {
        this.id = id;
        this.name = name;
        this.game = game;
    }

    @Override
    public boolean equals(Object another) {
        if (another == null) {
            return false;
        }
        if (!(another instanceof Player)) {
            return false;
        }
        Player otherPlayer = (Player) another;
        return this.id.equals(otherPlayer.id)
                && this.name.equals(otherPlayer.name);
    }

    @Override
    public int hashCode(){
        return Objects.hash(id, name, game);
    }

    @Override
    public String toString(){
        return this.id + "-" + this.name;
    }

    public void applyActions() {
    }
}
