package org.dinbur.ml.rl.monopoly.game.properties;

public class UtilityProperty extends MonopolyProperty {


    @Override
    public int computeRent(int propertiesOwned) {
        return 0;
    }


    @Override
    public int getHouseCost(){
        throw new UnsupportedOperationException("You cannot build houses on utilities");
    }


    @Override
    public int getHotelCost() {
        throw new UnsupportedOperationException("You cannot build hotels on utilities");
    }
}
