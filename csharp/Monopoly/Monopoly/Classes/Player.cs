using Monopoly.RLClasses;
using System;
using NeuronDotNet.Core.Backpropagation;

namespace Monopoly.Classes
{
    [Serializable]
    public class Player
    {

        #region Fields

        public int id { get; set; }
        public int position { get; set; }
        public string name { get; set; }
        public int money { get; set; }
        public int[] propertiesPurchased { get; set; } 
        public int[] mortgagedProperties { get; set; }
        public int[] buildingsBuilt { get; set; }
        public bool inJail { get; set; } 
        public bool isAlive { get; set; }
        public bool policyFrozen { get; set; }

        #endregion Fields

        public Player() { }

        //Initialize current player' fields
        public virtual void agent_init(char c,bool l,string agentName, int inputCount) { }

        //Receive the first observation of the game
        //No reward is expected now 
        //Send an action back to the environment
        public virtual int agent_start(Observation obs) { return 0; }

        //Receive observation and reward and send an action back to the environment
        public virtual int agent_step(Observation obs, double reward)
        { return 0; }

        //End of game
        public virtual void agent_end(double reward) { }

        //End of experiment
        public virtual void agent_cleanup() { }


        #region HelperMethods


        //Return type of agent
        public virtual char getType() { return 'r'; }

        //Get total number of houses
        public int getTotalHouses()
        {
            int counter = 0;

            for (int i = 0; i < buildingsBuilt.Length; i++)
            {
                if (buildingsBuilt[i] > 0 && buildingsBuilt[i] < 5)
                    counter++;
            }

            return counter;
        }

        //Get total number of hotels
        public int getTotalHotels()
        {
            int counter = 0;

            for (int i = 0; i < buildingsBuilt.Length; i++)
            {
                if (buildingsBuilt[i] == 5)
                    counter++;
            }

            return counter;
        }

        //Calculate payment for current position
        public virtual int getRentPayment(int currentPosition) { return 0; }

        //Set neural network
        public virtual void setNeural(NeuronDotNet.Core.Network network) { }

        //Get neural network
        public virtual NeuronDotNet.Core.Network getNeural()
        {
            LinearLayer inputLayer = new LinearLayer(23);
            SigmoidLayer outputLayer = new SigmoidLayer(100);
            return new BackpropagationNetwork(inputLayer, outputLayer);
        }

        //Save agent on file
        public virtual void saveOnFile(string p) { }

        #endregion HelperMethods


    }
}
