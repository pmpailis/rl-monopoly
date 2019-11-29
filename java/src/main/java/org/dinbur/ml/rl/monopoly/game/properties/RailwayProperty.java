package org.dinbur.ml.rl.monopoly.game.properties;

public class RailwayProperty extends MonopolyProperty {

    @Override
    public int getHouseCost() {
        throw new UnsupportedOperationException("You cannot build houses on railway properties");
    }


    @Override
    public int getHotelCost() {
        throw new UnsupportedOperationException("You cannot build hotels on railway properties");
    }
}
