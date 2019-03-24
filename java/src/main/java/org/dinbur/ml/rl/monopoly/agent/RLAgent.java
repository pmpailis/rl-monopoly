package org.dinbur.ml.rl.monopoly.agent;

interface RLAgent {


    void agent_init();

    void agent_start();

    void agent_step();

    void agent_end();

    void agent_cleanup();
}