
namespace Monopoly.Classes
{
    public class Board
    {
        public Card[] boardCards { get; set; }

        // 0 - Property Card
        // 1 - Community Chest Card
        // 2 - Chance Card
        // 3 - SpecialPosition ( Go,Jail,etc)
        public int[] typeId { get; set; }

        public Board() { }
        public Board(Card[] b,int[] t)
        {
            this.boardCards = b;
            this.typeId = t;
        }
    }
}