package org.dinbur.ml.rl.monopoly.game.engine;

import java.util.Random;

public class Dice {

    private final Random random = new Random(System.currentTimeMillis());

    public int roll() {
        return 1 + random.nextInt(6);
    }



}
