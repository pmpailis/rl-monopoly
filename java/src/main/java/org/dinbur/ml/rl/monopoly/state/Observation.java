package org.dinbur.ml.rl.monopoly.state;

import java.util.Arrays;
import java.util.Objects;

public class Observation {

    private final double[][] gameState;
    private final double relativeAssets;
    private final double relativeMoney;
    private final double relativeArea;

    private Observation(final double[][] gameState, final double relativeAssets, final double relativeMoney, final double relativeArea) {
        this.gameState = gameState;
        this.relativeAssets = relativeAssets;
        this.relativeMoney = relativeMoney;
        this.relativeArea = relativeArea;
    }

    @Override
    public int hashCode() {
        return Objects.hash(gameState, relativeArea, relativeMoney, relativeAssets);
    }

    @Override
    public boolean equals(Object other) {
        if (!(other instanceof Observation)) return false;
        Observation otherObservation = (Observation) other;
        return Arrays.equals(gameState, otherObservation.gameState)
                && relativeAssets == otherObservation.relativeAssets
                && relativeMoney == otherObservation.relativeMoney
                && relativeArea == otherObservation.relativeArea;
    }

    public static class Builder{

        private double[][] state;
        private double assets;
        private double money;
        private double area;
        public Builder(){}

        public Builder withAssets(double assets){
            this.assets= assets;
            return this;
        }

        public Builder withMoney(double money){
            this.money = money;
            return this;
        }

        public Builder withArea(double area){
            this.area = area;
            return this;
        }

        public Builder withState(double[][] state){
            this.state = state;
            return this;
        }

        public Observation build() {
            return new Observation(state, assets, money, area);
        }

    }
}
