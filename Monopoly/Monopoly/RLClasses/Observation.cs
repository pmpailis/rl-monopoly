using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monopoly.RLClasses
{
    [Serializable]
    public class Observation
    {
        public Obs_Area area { get; set; }
        public Obs_Position position { get; set; }
        public Obs_Finance finance { get; set; }

        //Constructor of the class
        public Observation() { }

        //Create new instance of the class
        public Observation(Obs_Area p_area, Obs_Position p_position, Obs_Finance p_finance)
        {
            this.area = p_area; this.finance = p_finance; this.position = p_position;
        }

        //Return a string represanting the current observation
        public string printInfo()
        {
            string info = "";
            info += "Game Group Info";
            info += Environment.NewLine;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    info += this.area.gameGroupInfo[i, j].ToString()+ "  ";
                }
                info+=Environment.NewLine;
            }

            info += "Position";
            info += Environment.NewLine;
            info += this.position.relativePlayersArea.ToString();
            info += Environment.NewLine;

            info += "Finance";
            info += Environment.NewLine;
            info += "Relative Assets : " +  this.finance.relativeAssets.ToString();
            info += Environment.NewLine; 
            info += "Relative Money : " + this.finance.relativePlayersMoney.ToString();
            info += Environment.NewLine;

            return info; 
        }
    }
}
