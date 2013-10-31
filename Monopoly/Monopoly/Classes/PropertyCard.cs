using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monopoly.Classes
{
    public class PropertyCard : Card
    {
        #region Fields

        public string cardName { get; set; }
        public int position { get; set; }
        public int price { get; set; }
        public List<int> rent { get; set; }
        public int mortage { get; set; }
        public int houseCost { get; set; }
        public int hotelCost { get; set; }
        public int group { get; set; }

        #endregion Fields

        public PropertyCard() {}
        public PropertyCard(string pName, int pPosition, int pPrice, List<int> pRent, int pMortage, int pHouseCost, int pHotelCost, int pGroup)
        {
            this.cardName = pName; this.position = pPosition; this.price = pPrice; this.rent = pRent; this.mortage = pMortage;
            this.houseCost = pHouseCost; this.hotelCost = pHotelCost; this.group = pGroup;
        }

        #region GetMethods

        //Return the position of the current card on the board
        public int getPosition() { return this.position; }

        //Return the name of the current card 
        public string getName() { return this.cardName; }

        //Return the group that the current card belongs to
        public int getGroup() { return this.group; }

        //Return the value of the current card
        public int getValue() { return this.price; }

        //Return the cost to build a house for the specific card
        public int getHouseCost() { return houseCost; }

        //Return the position of the current card on the board
        public int getHotelCost() { return hotelCost; }

        //Return the position of the current card on the board
        public int getMortgageValue() { return this.mortage; }
        
        #endregion GetMethods

    }
}