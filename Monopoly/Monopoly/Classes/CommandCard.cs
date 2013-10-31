
namespace Monopoly.Classes
{
    public class CommandCard : Card
    {
        #region Fields

        public int typeOfCard { get; set; }
        public string text { get; set; }
        public string fixedMove { get; set; }
        public string relativeMove { get; set; }
        public int collect { get; set; }
        public int moneyTransaction { get; set; }
        public int playerInteraction { get; set; }
        public int houseMultFactor { get; set; }
        public int hotelMultFactor { get; set; }

        #endregion Fields

        public CommandCard() { }
        public CommandCard(int pType, string pText, string pFixedMove, string pRelativeMove, int pCollect, int pMoneyTransaction, int pPlayerInteraction, int pHouseMult, int pHotelMult)
        {
            this.text = pText; this.typeOfCard = pType; this.relativeMove = pRelativeMove; this.playerInteraction = pPlayerInteraction; this.moneyTransaction = pMoneyTransaction;
            this.houseMultFactor = pHouseMult; this.hotelMultFactor = pHotelMult; this.fixedMove = pFixedMove; this.collect = pCollect;
        }

        //Get Type of card ( 0 - Community Chest, 1 - Chance )
        public int getType() { return typeOfCard; }

        //Return value of card
        public int getValue() { return 0; }
    }
}
