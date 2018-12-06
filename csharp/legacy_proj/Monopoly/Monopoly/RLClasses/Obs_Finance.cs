using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monopoly.RLClasses
{
    [Serializable]
    public class Obs_Finance
    {
        //Private fields of the Obs_Finance class
        public double relativeAssets;
        public double relativePlayersMoney;

        //Constructor of the Obs_Finance class
        public Obs_Finance() { }
    }
}
