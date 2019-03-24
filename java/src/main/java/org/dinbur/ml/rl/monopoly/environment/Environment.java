package org.dinbur.ml.rl.monopoly.environment;

public interface Environment {

    void env_init();

    void env_start();

    void env_step();

    void env_cleanup();
}
