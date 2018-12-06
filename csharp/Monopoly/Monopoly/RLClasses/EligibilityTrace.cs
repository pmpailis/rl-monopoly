using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monopoly.RLClasses
{
    [Serializable]
    public class EligibilityTrace
    {
        public Observation observation { get; set; }
        public Action action { get; set; }
        public double value { get; set; }

        public EligibilityTrace() { }

        public EligibilityTrace(Observation o, Action a, double v)
        {
            this.observation = o;
            this.action = a;
            this.value = v;
        }
    }
}
