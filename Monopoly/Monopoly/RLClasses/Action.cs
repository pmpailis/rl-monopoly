using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monopoly.RLClasses
{
    [Serializable]
    public class Action
    {
        //Int array to specify the actions that need to be taken
        public int action;

        //Constructor of class
        public Action() 
        {
            this.action = 0;
        }

        public Action(int v)
        {
            this.action = v;
        }
    }
}
