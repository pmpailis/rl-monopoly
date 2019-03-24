package org.dinbur.ml.rl.monopoly.state;

public class EligibilityTrace {

    private final Observation observation;
    private final Action action;
    private final double reward;

    public EligibilityTrace(final Observation observation, final Action action, final double reward) {
        this.observation = observation;
        this.action = action;
        this.reward = reward;
    }

    public Observation getObservation() {
        return observation;
    }

    public Action getAction() {
        return action;
    }

    public double getReward() {
        return reward;
    }
}
