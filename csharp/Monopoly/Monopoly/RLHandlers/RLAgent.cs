using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monopoly.Classes;
using Monopoly.RLClasses;
using System.Windows;
using Monopoly.MonopolyHandlers;
using NeuronDotNet.Core.Backpropagation;
using NeuronDotNet;
using NeuronDotNet.Core.Initializers;
using NeuronDotNet.Core;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Monopoly.RLHandlers
{
    [Serializable]
    public class RLAgent : Player
    {

        #region Fields

        //Last observation
        public Observation lastState { get; set; }
        public int lastAction { get; set; }

        //Traces
        public List<EligibilityTrace> traces = new List<EligibilityTrace>();

        //Neural Network
        NeuronDotNet.Core.Network network;

        //Current epoch - used only for training the nn
        public int currentEpoch;

        //RL - parameters
        public double epsilon { get; set; }
        public double alpha { get; set; }
        public double gamma { get; set; }
        public double lamda { get; set; }

        //Agent's type - random,qlearning or sarsa
        public char agentType { get; set; }

        #endregion Fields
        
        public RLAgent () { }


        #region RLMethods


        //Initialize agent's parameters
        public override void agent_init(char type, bool policy, string agentName, int inputCount)
        {

            //Initialize neural net
            LinearLayer inputLayer = new LinearLayer(inputCount + 1);
            SigmoidLayer hiddenLayer = new SigmoidLayer(150);
            LinearLayer outputLayer = new LinearLayer(1);

            new BackpropagationConnector(inputLayer, hiddenLayer).Initializer = new RandomFunction(-0.5, 0.5);
            new BackpropagationConnector(hiddenLayer, outputLayer).Initializer = new RandomFunction(-0.5, 0.5);

            this.network = new BackpropagationNetwork(inputLayer, outputLayer);

            this.network.SetLearningRate(0.2);
            this.network.Initialize();


            #region Initialize_parameters

            this.name = agentName;
            this.id = Int32.Parse(agentName.Last().ToString());

            this.agentType = type;
            this.policyFrozen = policy;

            if (policy)
            {
                this.epsilon = 0;
                this.alpha = 0;
            }
            else
            {
                this.epsilon = 0.5;
                this.alpha = 0.2;
            }

            this.gamma = 0.95;
            this.lamda = 0.8;

            currentEpoch = 1;

            initParams();

            #endregion Initialize_parameters

        }

        //First action of the agent, where no reward is to be expected from the environment
        public override int agent_start(Observation observation)
        {
            //Increase currentEpoch paramater ( used only in nn training)
            currentEpoch++;

            //Initialize agent's parameters
            initParams();

            //Create new array for action
            int action = 0;

            if (!agentType.Equals('r'))
            {
                ///Calculate Qvalues
                double[] QValues = calculateQValues(observation);

                //Select final action based on the ε-greedy algorithm
                action = e_greedySelection(QValues);

                //Update local values
                lastAction = action;
                lastState = observation;

                traces.Add(new EligibilityTrace(observation, new RLClasses.Action(action), 1));

                return action;
            }
            else
            {
                return  randomAction();
            }

        }
    
        //Receive an observation and a reward from the environment and send the appropriate action
        public override int agent_step(Observation observation, double reward)
        {      
            //If this isn't a random agent calculate the Q values for every possible action
            int action = 0;
            if (!agentType.Equals('r'))
            {
                //Calculate Qvalues
                double [] QValues = calculateQValues(observation);

                //Select action 
                action = e_greedySelection(QValues);

                //If the policy of the agent isn't frozen then train the neural network
                if (!policyFrozen)
                {
                    //If the agent is learning then update it's qValue for the selected action
                    double QValue = 0;
                    bool exists = false;

                    //Calculate the qValue either using the Q-learning or the SARSA algorithm
                    if (this.agentType.Equals('q'))
                    {
                        exists = updateQTraces(observation, new Monopoly.RLClasses.Action(action), reward);
                        QValue = Qlearning(lastState, new Monopoly.RLClasses.Action(lastAction), observation, new Monopoly.RLClasses.Action(findMaxValues(QValues)), reward);
                    }
                    else
                    {
                        exists = updateSTraces(observation, new Monopoly.RLClasses.Action(action));
                        QValue = Sarsa(lastState, new Monopoly.RLClasses.Action(lastAction), observation, new Monopoly.RLClasses.Action(action), reward);
                    }

                    trainNeural(createInput(lastState, lastAction), QValue);

                    //Add trace to list
                    if (!exists)
                        traces.Add(new EligibilityTrace(lastState, new RLClasses.Action(lastAction), 1));

                }

                //Update local values
                lastAction = action;
                lastState = observation;

                return action;
            }
            //Random action
            else
            {
                return randomAction();
            }
        }

        //End of current game
        public override void agent_end(double reward)
        {
            //Mark this agent as dead
            this.isAlive = false;

            //If this isn't a random agent
            if (!agentType.Equals('r') && (!this.policyFrozen))
            {
                //Update Traces
                if (agentType.Equals('q'))
                    updateQTraces(lastState, new RLClasses.Action(lastAction), reward);
                else
                { } //updateSTraces();
            }

            //Reduce RL-parameters values
            epsilon *= 0.99;
            alpha *= 0.99;
        }

        //Occurs when the experiment ( total set of games ) is finished
        public override void agent_cleanup()
        {
            //If this isn't a random agent and hasn't a frozen policy then store the agent to a file
            if (!this.agentType.Equals('r')&&(!this.policyFrozen))
            {
                if (!Directory.Exists("agents/"))
                    Directory.CreateDirectory("agents/");

                FileStream fs = new FileStream("agents/" + this.name + ".dat", FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, this);
                fs.Close();
            }
        }

        //Save network  on file
        public override void saveOnFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, this.network);
            fs.Close();
        }


         #endregion RLMethods


        #region MiscMethods

        public override Network getNeural()
        {
            return this.network;
        }

        //Return type of agent
        public override char getType()
        {
            return this.agentType;
        }
        
        //Set agent's neural network
        public override void setNeural(Network net)
        {
            this.network = net;
        }

        //Create input for the neural network
        public double[] createInput(Observation observation, int action)
        {
            List<double> input = new List<double>();

            //Add action
            input.Add((((double)(action+2))/3));

            //Add every variable of the observation to the input list
            for (int k = 0; k < observation.area.gameGroupInfo.GetLength(0); k++)
            {
                for (int kk = 0; kk < observation.area.gameGroupInfo.GetLength(1); kk++)
                    input.Add(observation.area.gameGroupInfo[k, kk]);
            }

            input.Add(observation.finance.relativeAssets);
            input.Add(observation.finance.relativePlayersMoney);

            input.Add(observation.position.relativePlayersArea);

            //Return the input array
            return input.ToArray();
        }

        //Calculate payment for a specific property
        public override int getRentPayment(int cp)
        {
            return ((App)Application.Current).app.gameCards[cp].rent[buildingsBuilt[cp]];
        }

        //Return a random action for the
        private int randomAction()
        {
            Random rnd = new Random();
            int i = rnd.Next(0, 10000) % 3;
            return i-1;
        }

        //Initialize local parameters for a new game
        public void initParams()
        {
            if (this.policyFrozen)
            {
                this.alpha = 0;
                this.epsilon = 0;
                this.lamda = 0;
                this.gamma = 0;
            }

            //numberOfProperties = 28
            base.propertiesPurchased = new int[28];
            base.mortgagedProperties = new int[28];
            base.buildingsBuilt = new int[28];

            this.agent_changeCurrentState(new Observation());

            //Initialize arrays
            for (int i = 0; i < 28; i++)
            {
                propertiesPurchased[i] = 0;
                mortgagedProperties[i] = 0;
                buildingsBuilt[i] = 0;
            }

            this.isAlive = true;
            base.inJail = false;

            base.money = 1500;
            base.position = 0;

            lastAction = 0;
            lastState = new Observation();

            traces = new List<EligibilityTrace>();
        }

        //Change agent's current observation based on what the agent receives
        public void agent_changeCurrentState(Observation obs) 
        {
            this.lastState = obs; 
        }

        //Train agent's neural network for specific input and desired output
        private void trainNeural(double[] input, double output)
        {
            double[] tmp = { output };

            //Create the training sample for the neural network 
            TrainingSample sample = new TrainingSample(input, tmp);

            //Train nn
            network.Learn(sample, 0, currentEpoch);

        }

        //ε-greedy Selection Algorithm
        private int e_greedySelection(double[] QValues)
        {
            //Create new action array
            int actionSelected = 0;

            //Calculate a new random value between 0-1
            Random rnd = new Random();
            double val = rnd.NextDouble();

            //Based on that value select either a random action or the best possible
            if (val >= this.epsilon)
            {
                //Select best action
                actionSelected = findMaxValues(QValues);
            }
            else
            {
                //Select random action
                actionSelected = randomAction();
            }

            return actionSelected;
        }

        //Return the best value of a 2-d array. Ties are being broken randomly
        private int findMaxValues(double[] tempQ)
        {
            //Create a new action and set the first qValue as max
            int selectedValue = -1;
            double maxValue = tempQ[0];

            //Search through the whole Q array to find the maximum value
            for (int i = 0; i < tempQ.Length; i++)
            {
                if (tempQ[i] > maxValue)
                {
                    selectedValue = i -1;
                    maxValue = tempQ[i];
                }

               //Break ties randomly
                else if (tempQ[i].Equals(maxValue))
                {
                    Random rnd = new Random();
                    double prValue = rnd.NextDouble();
                    double curValue = rnd.NextDouble();
                    if (curValue > prValue)
                    {
                        selectedValue = i - 1;
                        maxValue = tempQ[i];
                    }
                }
            }
            return selectedValue;
        }

        //Q learning algorithm
        private double Qlearning(Observation p_lastState, Monopoly.RLClasses.Action p_lastAction, Observation newState, Monopoly.RLClasses.Action bestAction, double reward)
        {
            double QValue = network.Run(createInput(p_lastState, p_lastAction.action)).First();

            //run network for last state and last action
            double previousQ = QValue;

            //run network for new state and best action
            double newQ = network.Run(createInput(newState, bestAction.action)).First();

            QValue += alpha * (reward + gamma * newQ - previousQ);

            return QValue;
        }

        //Sarsa algorithm
        private double Sarsa(Observation lastState, Monopoly.RLClasses.Action lastAction, Observation newState, Monopoly.RLClasses.Action newAction, double reward)
        {
            double QValue = network.Run(createInput(lastState, lastAction.action)).First();

            //run network for last state and last action
            double previousQ = QValue;

            //run network for new state and best action
            double newQ = network.Run(createInput(newState, newAction.action)).First();

            QValue += alpha * (reward + gamma * newQ - previousQ);

            return QValue;
        }

        //Calculate network's output
        private double[] calculateQValues(Observation obs)
        {
            double[] tempQ = new double[3];

            for (int i = 0; i < tempQ.Length; i++)
            {
                //Run netowrk for action i,j to given observation
                double[] input = createInput(obs, i - 1);

                tempQ[i] = network.Run(input)[0];
            }
            return tempQ;
        }

        //Update traces -- qlearning---Peng's Q(λ)
        private bool updateQTraces(Observation obs, Monopoly.RLClasses.Action a, double reward)
        {
            bool found = false;

            //Since the state space is huge we'll use a similarity function to decide whether two states are similar enough
            for (int i = 0; i < traces.Count; i++)
            {
                if (checkStateSimilarity(obs,traces[i].observation) && (!a.action.Equals(traces[i].action.action)))
                {
                    traces[i].value = 0;
                    traces.RemoveAt(i);
                    i--;

                }
                else if (checkStateSimilarity(obs, traces[i].observation) && (a.action.Equals(traces[i].action.action)))
                {
                    found = true;

                    traces[i].value = 1 ;

                    //Q[t] (s,a)
                    double qT = network.Run(createInput(traces[i].observation, traces[i].action.action))[0];

                    //maxQ[t] (s[t+1],a) 
                    int act = findMaxValues(calculateQValues(obs));
                    double maxQt = network.Run(createInput(obs, act))[0];

                    //maxQ[t] (s[t],a)
                    act = findMaxValues(calculateQValues(lastState));
                    double maxQ = network.Run(createInput(lastState, act))[0];

                    //Q[t+1] (s,a) = Q[t] (s,a) + alpha * ( trace[i].value ) * ( reward + gamma * maxQ[t] (s[t+1],a) * maxQ[t] (s[t],a))
                    double qVal = qT + alpha * (traces[i].value) * (reward + gamma * maxQt - maxQ);

                    trainNeural(createInput(traces[i].observation, traces[i].action.action), qVal);

                }
                else
                {
                    traces[i].value = gamma * lamda * traces[i].value;

                    //Q[t] (s,a)
                    double qT = network.Run(createInput(traces[i].observation, traces[i].action.action))[0];

                    //maxQ[t] (s[t+1],a) 
                    int act = findMaxValues(calculateQValues(obs));
                    double maxQt = network.Run(createInput(obs, act))[0];

                    //maxQ[t] (s[t],a)
                    act = findMaxValues(calculateQValues(lastState));
                    double maxQ = network.Run(createInput(lastState, act))[0];

                    //Q[t+1] (s,a) = Q[t] (s,a) + alpha * ( trace[i].value ) * ( reward + gamma * maxQ[t] (s[t+1],a) * maxQ[t] (s[t],a))
                    double qVal = qT + alpha * (traces[i].value) * (reward + gamma * maxQt - maxQ);

                    trainNeural(createInput(traces[i].observation, traces[i].action.action), qVal);
                }
            }

            return found;
        }

        //Update traces  -- sarsa
        private bool updateSTraces(Observation obs, Monopoly.RLClasses.Action a)
        {
            return false;
        }

        //Calculate similarity of states
        private bool checkStateSimilarity(Observation obs1, Observation obs2)
        {
            bool similar = true;

            //Check money similarity
            double moneyDif = Math.Abs(obs1.finance.relativeAssets - obs2.finance.relativeAssets) + Math.Abs(obs1.finance.relativePlayersMoney - obs2.finance.relativePlayersMoney);
            if (moneyDif >= 0.1)
                similar = false;

            //Check area similarity
            if (!obs1.position.relativePlayersArea.Equals(obs2.position.relativePlayersArea))
                similar = false;

            double countDif = 0;
            for (int i = 0; i < obs1.area.gameGroupInfo.GetLength(0); i++)
            {
                if (!similar)
                    break;

                countDif = 0;
                for (int j = 0; j < obs1.area.gameGroupInfo.GetLength(1); j++)
                {
                    if (!obs1.area.gameGroupInfo[i, j].Equals(obs2.area.gameGroupInfo[i, j]))
                    {
                        countDif += Math.Abs(obs1.area.gameGroupInfo[i, j] - obs2.area.gameGroupInfo[i, j]);
                        if (countDif >= 0.1)
                        { similar = false; break; }
                    }                    
                }
            }

            return similar;
        }


        #endregion MiscMethods

    }
}
