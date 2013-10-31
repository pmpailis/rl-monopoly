using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Monopoly.Classes;
using Monopoly.MonopolyHandlers;
using Monopoly.RLClasses;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using NeuronDotNet.Core;

namespace Monopoly.RLHandlers
{
    public class RLEnvironment
    {
        public TextWriter Awriter = new StreamWriter("actions.txt");

        //Handler for the helper methods of the project
        private MonopolyHandlers.InitMethods initMethods = new MonopolyHandlers.InitMethods();

        public RLEnvironment() { }


        #region Private Fields

        //List of winners of every game
        List<int> winners = new List<int>();

        //List of the duration of every game
        List<double> times = new List<double>();

        //List of moves of every game
        List<int> moves = new List<int>();

        //Average money of every player
        int[] averageMoney;

        //Stopwatch timer to calculate duration of game
        Stopwatch timer;

        //Create stream writer for the output file
        TextWriter textWriter;

        //Int value specifying the current value
        int currentGame;

        //Int array specifying the times that each player played during a game
        int[] playerMoves;

        //Rounds for every game before it ends
        public int stepCounter;

        //Total number of games
        public int totalGames;

        //Action methods
        ActionMethods methods = new ActionMethods();
        
        //Current player and current position value
        public int currentPlayer;
        public int currentPosition;

        bool moved = false;

        //Jail-helper variables
        public int doublesInRow = 0;
        public int[] getOutOfJailTries;

        //int[] properties = ( default = -1 , otherwise the index of the player )
        public int[] properties = new int[40];

        //int[] buildings = the number of buildings on each position on the board
        public int[] buildings = new int[40];

        //int[] completedGroups = (default = -1, otherwise the index of the player )
        public int[] completedGroups = new int[10];

        //Specify the color of each group of cards ( i.e. 1st group - Dark Blue )
        public static SolidColorBrush[] groupCardColour = { Brushes.DarkBlue, Brushes.Purple, Brushes.Violet, Brushes.Orange, Brushes.Red, Brushes.Yellow, Brushes.Green, Brushes.Blue, Brushes.Black, Brushes.White };

        //Exact Position of Command Cards and Special Positions (as Jail,Go,Tax etc) on board
        //We'll use these variables later to create the board and specify the type of each position
        public int[] propertyCardsPosition = { 1, 3, 5, 6, 8, 9, 11, 12, 13, 14, 15, 16, 18, 19, 21, 23, 24, 25, 26, 27, 28, 29, 31, 33, 34, 35, 37, 39 };
        public int[] communityChestCardsPositions = { 2, 17, 33 };
        public int[] chanceCardsPositions = { 7, 22, 36 };
        public int[] specialPositions = { 0, 4, 10, 20, 30, 38 };

        //List of all the property cards of game
        public List<PropertyCard> gameCards { get; set; }

        //List of string that contain the positions on board that create a group
        //i.e. the value "1,3" -> the 1st and 3rd position of board form a group 
        public string[] gameCardsGroup = { "12,28", "5,15,25,35", "1,3", "6,8,9", "11,13,14", "16,18,19", "21,23,24", "26,27,29", "31,32,34", "37,39" };

        //List of Game Players
        public List<Player> gamePlayers { get; set; }

        //List of CommandCards (both Community Chest and Chance cards)
        public List<CommandCard> gameCommandCards { get; set; }

        //Specific sub-lists of CommandCards.
        //When a player takes a card then it goes to the last index of the list and all the rest
        //move one position to the left.
        public List<CommandCard> communityChestCards { get; set; }
        public List<CommandCard> chanceCards { get; set; }

        //Variable to determine the current state of the game ( for comparison with the const values mentioned above).
        public int currentHouses { get; set; }
        public int currentHotels { get; set; }
        public int currentPlayers { get; set; }

        //Board variable that specifies the card on every position and it's type ( as described on Board.cs)
        public Board board { get; set; }

        //Global const values
        public const int MAXPLAYERS = 4;
        public const int MAXHOUSES = 25;
        public const int MAXHOTELS = 10;
        public const int MAXAGENTACTIONS = 3;
        public const int MAXSTEPS = 2000;
        public const int REWARD = 0;
        public const int WINREWARD = 10;
        public const int DEFEATREWARD = -10;

        //Const value to calculate the sin of every position on board
        public const double sinConst = 4.61538;

        #endregion Private Fields


        #region SetMethods

        //Set the board variable ( represents info for every position of the board)
        public void setBoard(Board b)
        {
            this.board = b;
        }

        //Create lists of Chance and Community Chest Cards depending on the type of the card that we get from the xml file
        public void setCommandCards()
        {
            for (int i = 0; i < gameCommandCards.Count; i++)
            {
                if (gameCommandCards[i].getType() > 0)
                    chanceCards.Add(gameCommandCards[i]);
                else
                    communityChestCards.Add(gameCommandCards[i]);
            }

            //Shuffle cards
            chanceCards = shuffle(chanceCards);
            communityChestCards = shuffle(communityChestCards);
        }

        #endregion SetMethods


        #region GetMethods

        //Get maximum hotels
        public int getMaxHotels() { return MAXHOTELS; }

        //Get maximum houses
        public int getMaxHouses() { return MAXHOUSES; }

        //Get the colour of a group
        public SolidColorBrush getColour(string pName)
        {
            int index = 0;
            foreach (PropertyCard c in gameCards)
            {
                if (c.getName().Equals(pName))
                { index = c.getGroup(); break; }
            }
            return groupCardColour[index];
        }

        //Get specific positions of CommandCards
        public int[] getCommunityCardPositions()
        {
            return communityChestCardsPositions;
        }
        public int[] getChanceCardPositions() { return chanceCardsPositions; }

        //Get Game Property Cards
        public List<PropertyCard> getCards() { return gameCards; }

        //Get Game Command Cards
        public List<CommandCard> getCommandCards() { return gameCommandCards; }

        //Get Game Players
        public List<Player> getPlayers() { return gamePlayers; }

        //Get maximum actions available 
        public int getMaxActions() { return MAXAGENTACTIONS; }

        #endregion GetMethods


        #region MiscMethods


        //Methods to add and delete a CommandCard 
        public void addCommandCard(CommandCard pCard)
        {
            if (!gameCommandCards.Contains(pCard))
                gameCommandCards.Add(pCard);
        }
        public void deleteCommandCard(CommandCard pCard)
        {
            if (gameCommandCards.Contains(pCard))
                gameCommandCards.Remove(pCard);
        }

        //Methods to add and delete a PropertyCard
        public void addCard(PropertyCard pCard)
        {
            if (!gameCards.Contains(pCard))
                gameCards.Add(pCard);
        }
        public void deleteCard(PropertyCard pCard)
        {
            if (gameCards.Contains(pCard))
                gameCards.Remove(pCard);
        }

        //Create string variable to print information about the current state of the environment
        public string createEnvInfo()
        {
            string info = "";
            for (int i = 0; i < gamePlayers.Count; i++)
            {
                info += "Money : " + gamePlayers[i].money + Environment.NewLine;
            }
            info += Environment.NewLine + "----------------------------------------" + Environment.NewLine;
            for (int i = 0; i < gameCards.Count; i++)
            {
                info +=  "owned by " + properties[getIndexFromPosition(i)].ToString() + "  buildings " + buildings[getIndexFromPosition(i)].ToString();
                info += Environment.NewLine;
            }

            info += Environment.NewLine + "----------------------------------------" + Environment.NewLine;

            return info;
        }


        #region CreateObservation

        //Create an instance of Observation class
        //Representing the current state of the environment
        public Observation createObservation()
        {
            Observation obs = new Observation();

            //Create the specific instances of the classes-fields of the Observation
            Obs_Finance finance = createFinance();
            Obs_Position position = createPosition();
            Obs_Area area = createArea();

            obs.area = area;
            obs.finance = finance;
            obs.position = position;

            return obs;
        }

        //Create an new position instance based on the current game's data
        private Obs_Position createPosition()
        {
            Obs_Position position = new Obs_Position();


            #region DeletedParams

            /*
            #region CurrentArea

            //Current player's current area
            position.currentArea = -1;
            if (board.typeId[currentPosition].Equals(0))
            {
                position.currentArea = Math.Sin(DegreeToRadian(18 * gameCards[getIndexFromPosition(currentPosition)].getGroup()));
            }

            #endregion CurrentArea


            #region RelativePlayersPosition

            //Relative players position
            double[] relativePlayersPosition = new double[currentPlayers];

            //Calculate the relative value of every player
            int counter = 1;
            int lastAlive = 0;
            relativePlayersPosition[0] = Math.Sin(DegreeToRadian(sinConst * gamePlayers[currentPlayer].position));
            for (int i = 0; i < currentPlayers; i++)
            {
                if (!currentPlayer.Equals(i))
                {
                    relativePlayersPosition[counter] = Math.Sin(DegreeToRadian(sinConst * gamePlayers[counter].position));
                    counter++;
                    if (gamePlayers[i].isAlive)
                        lastAlive = i;
                }
            }

            //For every player that isn't alive assign to his place the value of the last ( based on index ) alive player
            counter = 0;
            for (int i = 0; i < currentPlayers; i++)
            {
                if (!i.Equals(currentPlayer))
                {
                    if (!gamePlayers[i].isAlive)
                        relativePlayersPosition[counter] = relativePlayersPosition[lastAlive];

                    counter++;
                }
            }

            position.relativePlayersPosition = relativePlayersPosition;

            #endregion RelativePlayersPosition
            */

            #endregion DeletedParams


            #region RelativePlayersArea

            double relativePlayersArea = 0;
            if (board.typeId[currentPosition].Equals(0) && gamePlayers[currentPlayer].isAlive)
                relativePlayersArea = (double)(getCardFromPosition(currentPosition).getGroup() + 1) / 10;
            else
                relativePlayersArea = 0;


            position.relativePlayersArea = relativePlayersArea;

            #endregion RelativePlayersArea


            return position;
        }

        //Create a new finance instance based on the current game's data
        private Obs_Finance createFinance()
        {
            Obs_Finance finance = new Obs_Finance();


            #region RelativeAssets

            double total = 0;

            for (int i = 0; i < gamePlayers.Count(); i++)
            {
                total += methods.mActions.caclulateAllAssets(i);
            }

            double assets = (double)((int)(methods.mActions.caclulateAllAssets(currentPlayer)));

            //Current player's money / Total money
            finance.relativeAssets = assets / total;

            #endregion RelativeAssets


            #region RelativePlayersMoney

            finance.relativePlayersMoney = smoothFunction(gamePlayers[currentPlayer].money, 1500);

            #endregion RelativePlayersMoney


            return finance;
        }

        //create a new area instance based on the current game's data
        private Obs_Area createArea()
        {
            Obs_Area area = new Obs_Area();

            #region GameGroupInfo

            double[,] groupInfo = new double[gameCardsGroup.Length, 2];
            for (int i = 0; i < gameCardsGroup.Length; i++)
            {
                //Group isn't completed
                if (completedGroups[i].Equals(-1))
                {
                    double cPlayer = 0;
                    double oPlayers = 0;

                    string[] tmp = gameCardsGroup[i].Split(',');
                    for (int j = 0; j < tmp.Length; j++)
                    {
                        if (properties[Int32.Parse(tmp[j])].Equals(currentPlayer))
                        {
                            if (gamePlayers[currentPlayer].mortgagedProperties[getIndexFromPosition(Int32.Parse(tmp[j]))].Equals(0))
                                cPlayer++;
                        }

                        else if (!properties[Int32.Parse(tmp[j])].Equals(-1))
                            oPlayers++;
                    }

                    groupInfo[i, 0] = (int)(12 / gameCardsGroup[i].Split(',').Length * cPlayer);
                    groupInfo[i, 1] = (int)(12 / gameCardsGroup[i].Split(',').Length * oPlayers);


                    if (groupInfo[i, 1].Equals(12))
                    {
                        int alivePlayers = 0;
                        for (int k = 0; k < currentPlayers; k++)
                        {
                            if (gamePlayers[k].isAlive && (!k.Equals(currentPlayer)))
                                alivePlayers++;
                        }

                        groupInfo[i, 1] = (int)groupInfo[i, 1] / alivePlayers;
                    }
                }

                //If the group is completed
                else
                {
                    string[] gr = gameCardsGroup[i].Split(',');
                    int mortCounter = 0;

                    int tmp = buildings[Int32.Parse(gameCardsGroup[i].Split(',')[gameCardsGroup[i].Split(',').Length - 1])];

                    if (completedGroups[i].Equals(currentPlayer))
                    {
                        for (int j = 0; j < gr.Length; j++)
                        {
                            if (gamePlayers[currentPlayer].mortgagedProperties[getIndexFromPosition(Int32.Parse(gr[j].ToString()))].Equals(1))
                            {
                                mortCounter++;
                            }
                        }

                        groupInfo[i, 1] = 0;
                        groupInfo[i, 0] = 12 + tmp;
                        if (mortCounter > 0)
                            groupInfo[i, 0] = 12 - (int)(12 / gameCardsGroup[i].Split(',').Length * mortCounter);

                    }

                    else
                    {
                        groupInfo[i, 0] = 0;
                        groupInfo[i, 1] = 12 + tmp;
                    }
                }
            }

            for (int i = 0; i < groupInfo.GetLength(0); i++)
            {
                for (int j = 0; j < groupInfo.GetLength(1); j++)
                {
                    groupInfo[i, j] = (double)((double)groupInfo[i, j] / 17);
                }
            }

            area.gameGroupInfo = groupInfo;


            #endregion GameGroupInfo


            #region DeletedParams

            /*

            #region CurrentPropertyOwner

            //Current's area owner
            //-1 if an opponent has it 
            // 0 if none
            // 1 if the current player has it
            int cPO = 0;
            if (board.typeId[currentPlayer].Equals(0))
            {
                if (properties[currentPosition].Equals(currentPlayer))
                    cPO = 1;
                else if (properties[currentPlayer].Equals(-1))
                    cPO = 0;
                else
                    cPO = -1;
            }

            area.currentPropertyOwner = cPO;

            #endregion CurrentPropertyOwner


            #region RelativeNumberOfPropertiesFromDifferentGroups

            //Create new list of int represanting the groups that the player has properties from
            List<int> groups = new List<int>();

            for (int i = 0; i < gameCards.Count; i++)
            {
                //If the current player has this card then add it's group to the list and increase the counter only if it doesn't already exist
                if (gamePlayers[currentPlayer].propertiesPurchased[i].Equals(1))
                {
                    if (!groups.Contains(gameCards[i].getGroup()))
                    {
                        groups.Add(gameCards[i].getGroup());
                    }
                }
            }

            //Store the number
            area.relativeNumberOfPropertiesFromDifferentGroups = (double)((double)groups.Count / 10);

            #endregion RelativeNumberOfPropertiesFromDifferentGroups

             */
            #endregion DeletedParams


            return area;

        }

        #endregion CreateObservation


        #endregion MiscMethods


        #region InternalMethods

        //Calculate Reward
        internal double calculateReward(int player)
        {

            double reward = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                if (board.typeId[i].Equals(0))
                {
                    if (properties[i].Equals(player))
                    {
                        if (gamePlayers[player].mortgagedProperties[getIndexFromPosition(i)].Equals(0))
                        {
                            reward++;
                            if (buildings[i] > 0)
                                reward += buildings[i];
                        }
                           
                    }
                    else if (!properties[i].Equals(-1))
                    {
                        reward--;
                        if (buildings[i] > 0)
                            reward -= buildings[i];
                    }
                }
            }

            for (int i = 0; i < completedGroups.Length; i++)
            {
                if (completedGroups[i].Equals(player))
                {
                    reward += (i + 1);
                }
                else if (!completedGroups[i].Equals(-1))
                    reward -= (i + 1);
            }

            double total = 0;
            double assetFactor = 0;
            int alivePlayers = 0;
            for (int i = 0; i < currentPlayers; i++)
            {
                if (gamePlayers[i].isAlive)
                {
                    alivePlayers++;
                    total += gamePlayers[i].money;
                    if (i.Equals(player))
                        assetFactor = gamePlayers[i].money;
                }
            }

            assetFactor = assetFactor / total;

            reward = smoothFunction(reward, alivePlayers * 5);  //alivePlayers * 5

            reward = reward + (1 / alivePlayers) * assetFactor;

            return reward;
 
        }

        //Calculate reward in [-1,1]
        internal double smoothFunction(double x,double factor)
        {
            return (x / factor) / (1 + Math.Abs(x / factor));
        }

        //Convert  degree to radian
        internal double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    
        //Shuffle list
        internal static List<CommandCard> shuffle(List<CommandCard> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                CommandCard value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
        
        //Load an already created agent
        internal Network loadNeural(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            Network network = (Network)formatter.Deserialize(fs);
            fs.Close();

            return network;
        }

        //Load an already created agent
        internal RLAgent loadAgent(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            RLAgent agent = (RLAgent)formatter.Deserialize(fs);
            fs.Close();

            return agent; 
        }

        //Get card from position
        internal PropertyCard getCardFromPosition(int currentPosition)
        {
            //Count all the number of property cards that are between the start of the board and player's current position
            return gameCards[getIndexFromPosition(currentPosition)];
        }

        //Get index from position
        internal int getIndexFromPosition(int currentPosition)
        { 
            //Count all the number of property cards that are between the start of the board and player's current position
            int counter = 0;

            for (int i = 0; i < currentPosition; i++)
            {
                if (propertyCardsPosition.Contains(i))
                    counter++;
            }

            return counter;
        }

        //Move current player on board
        internal void movePlayer(int playerToMove)
        {
            System.Threading.Thread.Sleep(10);

            //Calculate the positions to move
            Random rnd = new Random();
            int dice1 = rnd.Next(1, 10000) % 6 + 1;
            int dice2 = rnd.Next(1, 10000) % 6 + 1;

            int dice = dice1 + dice2;

            if ((currentPosition + dice) > 39)
            {
                if (!dice.Equals(dice2))
                    gamePlayers[playerToMove].money += 200;
                else
                {
                    if (doublesInRow < 2)
                        gamePlayers[playerToMove].money += 200;
                }
            }

            //Move the player
            currentPosition = gamePlayers[playerToMove].position + dice;
            currentPosition = currentPosition % 40;

            //Change current's player current position
            gamePlayers[playerToMove].position = currentPosition;

            if (dice1 == dice2)
                doublesInRow++;
            else
                doublesInRow = 0;
        }

        //Check whether the user has to pay rent for the current property
        internal void onPositionChanged()
        {
            //Check whether the current position is a property card
            if (board.typeId[currentPosition].Equals(0))
            {
                //If it is then check whehter it is in someone's possession
                if(!(properties[currentPosition].Equals(-1) || properties[currentPosition].Equals(currentPlayer)))
                {
                    int owner = properties[currentPosition];

                    //If the player has mortgaged the area then pay nothing
                    if (gamePlayers[owner].mortgagedProperties[getIndexFromPosition(currentPosition)].Equals(1))
                        return;

                    double amount = gamePlayers[owner].getRentPayment(getIndexFromPosition(currentPosition));

                    if (getCardFromPosition(currentPosition).getGroup() > 1)
                    {
                        if (completedGroups[getCardFromPosition(currentPosition).getGroup()].Equals(owner) && buildings[currentPosition].Equals(0))
                            amount *= 2;
                    }


                    //Utility
                    if (getCardFromPosition(currentPosition).getGroup().Equals(0))
                    {
                        Random rnd = new Random();
                        amount = amount * (rnd.Next(1, 7) + rnd.Next(1, 7));
                    }

                    //Railway
                    if (getCardFromPosition(currentPosition).getGroup().Equals(1))
                    {
                        int counter = -1;
                        foreach (string s in gameCardsGroup[1].Split(','))
                        {
                            if (properties[Int32.Parse(s)].Equals(owner))
                                counter++;
                        }

                        amount = gameCards[getIndexFromPosition(currentPosition)].rent[counter];
                    }

                    if (methods.mActions.payMoney(currentPlayer, owner,(int)amount) < 0)
                    {
                        //If the player can't pay then remove him from the game
                        removePlayer(currentPlayer);
                        gamePlayers[owner].money += gamePlayers[currentPlayer].money;
                        for (int i = 0; i < gameCards.Count; i++)
                        {
                            if (gamePlayers[currentPlayer].propertiesPurchased[i].Equals(1))
                            {
                                gamePlayers[owner].propertiesPurchased[i] = 1; 
                                properties[gameCards[i].getPosition()] = owner;
                                if (gamePlayers[currentPlayer].mortgagedProperties[i].Equals(1))
                                    gamePlayers[owner].mortgagedProperties[i] = 1;
                                methods.mActions.checkIfCompleted(owner, gameCards[i].getPosition());
                            }
                        }
                    }
                    
                }
            }
        }

        //Apply command card
        internal void onCommandCard()
        {
            //Check whether it is a community chest card or a chance card
            if (communityChestCardsPositions.Contains(currentPosition))
            {
                //Take the 1st card of the list, apply it to the game and then move it to the last position
                applyCommandCard(communityChestCards[0]);
                moveCommunityChestCard();
            }
            else
            {
                //Take the 1st card of the list, apply it to the game and then move it to the last position
                applyCommandCard(chanceCards[0]);
                moveChanceCard();
            }
        }

        //Move 1st community chest card to the last position
        internal void moveCommunityChestCard()
        {
            communityChestCards.Add(communityChestCards[0]);
            communityChestCards.RemoveAt(0);
        }

        //Move 1st chance card to the last position
        internal void moveChanceCard()
        {
            chanceCards.Add(communityChestCards[0]);
            chanceCards.RemoveAt(0);
        }

        //Apply a command card to the game state
        internal void applyCommandCard(CommandCard commandCard)
        {
            #region Fixed Move

            //Initially we check whether it's a fixed move or not
            if (!commandCard.fixedMove.Equals("NULL"))
            {
                //Specify where we're gonna move the player
                int moveTo = Int32.Parse(commandCard.fixedMove);

                //If the command card specifies him to collect money
                if (commandCard.collect > 0)
                {
                    //Then if he passes through "GO" collect some money
                    //Otherwise do nothing
                    if(moveTo-currentPosition<=0)
                        gamePlayers[currentPlayer].money += commandCard.moneyTransaction;
                }

                //Change current position
                gamePlayers[currentPlayer].position = moveTo;
                currentPosition = moveTo;
                moved = true;
            }

            #endregion Fixed Move

            #region Relative Move

            //Relative move
            else if (!commandCard.relativeMove.Equals("NULL"))
            {
                //Find the specific position to move
                //We'll move towards the nearest group
                if (Int32.Parse(commandCard.relativeMove) > 0)
                {
                    int moveTo = findNearestFromGroup(Int32.Parse(commandCard.relativeMove));

                    //If the player is to collect money then add the specific amount to his balance
                    if (commandCard.collect > 0)
                    {
                        if (moveTo - currentPosition <= 0)
                            gamePlayers[currentPlayer].money += commandCard.moneyTransaction;
                    }

                    //Change current position
                    gamePlayers[currentPlayer].position = moveTo;
                    currentPosition = moveTo;
                    moved = true;
                }
            }

            #endregion Relative Move

            #region Money Transaction

            else
            {
                //If the player is to collect money
                if (commandCard.collect > 0)
                {
                    //Check whether it is from the other players or from the bank
                    if (commandCard.playerInteraction > 0)
                    {
                        getMoneyFromPlayers(commandCard.moneyTransaction);
                    }
                    else
                        gamePlayers[currentPlayer].money += commandCard.moneyTransaction;
                }

                //Otherwise check whether he is to pay money
                if (commandCard.collect == 0)
                {
                    //Calculate the total amount that his has to pay
                   int moneyToPay = commandCard.moneyTransaction + commandCard.houseMultFactor * gamePlayers[currentPlayer].getTotalHouses() + commandCard.hotelMultFactor * gamePlayers[currentPlayer].getTotalHotels();
                  
                    //Check wheter the player has the money to pay for his fine
                   //If not then he has to declare bankruptchy and exit the game
                   if (methods.mActions.payMoney(currentPlayer, -1, moneyToPay) < 0)
                   {
                       //Remove him for the game
                       removePlayer(currentPlayer);
                       for (int i = 0; i < gameCards.Count; i++)
                       {
                           if (gamePlayers[currentPlayer].propertiesPurchased[i].Equals(1))
                           {
                               biddingWar(gameCards[i].getPosition());
                           }
                       }
                   }
                }
            }

            #endregion Money Transaction

         //   MessageBox.Show(commandCard.text);
        }

        //Get money from every player other than the current and add them to his balance
        internal void getMoneyFromPlayers(int p)
        {
            for (int i = 0; i < gamePlayers.Count; i++)
            {
                if ((!i.Equals(currentPlayer)) && gamePlayers[i].isAlive)
                {
                    if (methods.mActions.payMoney(i, currentPlayer, p) < 0)
                    {
                        //If the player can't pay then remove him from the game
                        removePlayer(i);
                        gamePlayers[currentPlayer].money += gamePlayers[i].money;
                        for (int j = 0; j < gameCards.Count; j++)
                        {
                            if (gamePlayers[i].propertiesPurchased[j].Equals(1))
                            {
                                gamePlayers[currentPlayer].propertiesPurchased[j] = 1;
                                properties[gameCards[j].getPosition()] = currentPlayer;
                                if (gamePlayers[i].mortgagedProperties[j].Equals(1))
                                    gamePlayers[currentPlayer].mortgagedProperties[j] = 1;
                                methods.mActions.checkIfCompleted(currentPlayer, gameCards[j].getPosition());
                            }
                        }
                    }
                }
            }
        }

        //Find nearest position that belongs to a specific group
        internal int findNearestFromGroup(int p)
        {
            int moveTo = 0;
            int minDist = 100;
            
            //Find the nearest utility to him and move him there
            string[] tmp = gameCardsGroup[p].Split(',');
            for (int i = 0; i < tmp.Length; i++)
            {
                if (Math.Abs(currentPosition - Int32.Parse(tmp[i])) < minDist)
                {
                    minDist = Math.Abs(currentPosition - Int32.Parse(tmp[i]));
                    moveTo = Int32.Parse(tmp[i]);
                }
            }

            return moveTo;
        }

        //Act accordingly when a player lands on a special position on board
        internal void onSpecialPosition()
        {

            //Special Position :  { 0, 4, 10, 20, 30, 38 };

            //Go
            if (currentPosition.Equals(0))
            { }

            //Income tax
            else if (currentPosition.Equals(4))
            //Pay either 10% of income or 200 - whichever is lower
            {
                int minAmount = (int)(gamePlayers[currentPlayer].money * 0.1);
                if (minAmount > 200)
                    minAmount = 200;

                if (methods.mActions.payMoney(currentPlayer, -1, minAmount) < 0)
                {
                    //Remove him from the game
                    removePlayer(currentPlayer);
                    for (int i = 0; i < gameCards.Count; i++)
                    {
                        if (gamePlayers[currentPlayer].propertiesPurchased[i].Equals(1))
                        {
                            biddingWar(gameCards[i].getPosition());
                        }
                    }
                }
            }

            //Jail ( just visiting )
            else if (currentPosition.Equals(10))
            { }

            //FreeParking 
            else if (currentPosition.Equals(20))
            { }

            //Jail Position
            else if (currentPosition.Equals(30))
            {
                //Send him to jail
                gamePlayers[currentPlayer].position = 10;
                gamePlayers[currentPlayer].inJail = true;
                currentPosition = 10;
            }

            //Luxury tax
            else
            {
                if (methods.mActions.payMoney(currentPlayer, -1, 75) < 0)
                {
                    //Remove him from game
                    removePlayer(currentPlayer);
                    for (int i = 0; i < gameCards.Count; i++)
                    {
                        if (gamePlayers[currentPlayer].propertiesPurchased[i].Equals(1))
                        {
                            biddingWar(gameCards[i].getPosition());
                        }
                    }
                }
            }

        }

        //If the player is in jail then try to get out
        internal void inJailPosition()
        {
            System.Threading.Thread.Sleep(5);
            Random rnd = new Random();
            int dice1 = rnd.Next(1, 10000) % 6 + 1;
            int dice2 = rnd.Next(1, 10000) % 6 + 1;

            //Get gim out of jail
            if (dice1.Equals(dice2))
            {
                doublesInRow = 0;
                getOutOfJailTries[currentPlayer] = 0;
                gamePlayers[currentPlayer].inJail = false;
            }
            else
            {
                getOutOfJailTries[currentPlayer]++;
                if (getOutOfJailTries[currentPlayer] < 3)
                    env_selectNextAgent();
            }

            //If maximum tries have been reached then pay the fine and get out normally
            if (getOutOfJailTries[currentPlayer].Equals(3))
            {
                if (methods.mActions.payMoney(currentPlayer, -1, 50) > 0)
                {
                    doublesInRow = 0;
                    getOutOfJailTries[currentPlayer] = 0;
                    gamePlayers[currentPlayer].inJail = false;
                }
                //Else remove him from game
                else
                {
                    removePlayer(currentPlayer);
                    for (int i = 0; i < gameCards.Count; i++)
                    {
                        if (gamePlayers[currentPlayer].propertiesPurchased[i].Equals(1))
                        {
                            biddingWar(gameCards[i].getPosition());
                        }
                    }
                    env_selectNextAgent();
                }
            }

            //If he isn't in jail then act as if it is a normal turn
            if (!gamePlayers[currentPlayer].inJail)
                playGame();
        }

        //Remove a player from the game
        internal void removePlayer(int id)
        {
            gamePlayers[id].agent_end(DEFEATREWARD);

            //Return hotels and houses to the bank
            currentHotels -= gamePlayers[id].getTotalHotels();
            currentHouses -= gamePlayers[id].getTotalHouses();

            for (int i = 0; i < 40; i++)
            {
                if (properties[i].Equals(id))
                {
                    properties[i] = -1;
                    buildings[i] = 0;
                }
            }

            for (int i = 0; i < completedGroups.Length; i++)
            {
                if (completedGroups[i].Equals(id))
                    completedGroups[i] = -1;
            }
            
            //Have to attach player's neural net also
            getOutOfJailTries[id] = 0;
            averageMoney[id] += gamePlayers[id].money;
        } 


        #endregion InternalMethods


        #region RLMethods
		
		
        #region Environment

        //Initialize environment's parameters
        public void env_init()
        {
            //Create new list of agents
            gamePlayers = new List<Player>();
            currentPlayers = 3;

            //Average money of every player during the game
            averageMoney = new int[currentPlayers];

            //Initialize agents. We'll use the same for all games during this run
            for (int i = 0; i < currentPlayers; i++)
            {
                gamePlayers.Add(new RLAgent());
                System.Threading.Thread.Sleep(100);
            
                this.gamePlayers[i].agent_init('q', false, "Agent" + i.ToString(), (23)); //agent type(random-qlearning, policyFrozen, name, input vector length

                averageMoney[i] = 0;
            }

            //Initialize stopwatch
            timer = new Stopwatch();

            //Set total games
            totalGames = 1001;

            //Start the games
            for (currentGame = 0; currentGame < totalGames; currentGame++)
            {       
                System.Threading.Thread.Sleep(100);

                Awriter.WriteLine("---------------------------------");

                //Reset and start the timer
                timer.Reset();
                timer.Start();

                //Initialize stepCounter variable to prevent it from going on forever and determine manually the winner
                stepCounter = 0;

                //Start and play the game
                env_start();

             if ((currentGame % 5).Equals(0) && (!gamePlayers[0].getType().Equals('r')))
                    gamePlayers[0].saveOnFile("agents/nn" + currentGame.ToString() + "games.dat");
            }

            //Print experiment's info
            printInfo();

            //Close the writer
            textWriter.Close();

            //Cleanup agents
            env_cleanup();

            Awriter.Close();
        }

        //Start playing the game
        public void env_start()
        {
            System.Threading.Thread.Sleep(100);

            //Start new game
            initGameParameters();

            //First player to play
            int firstPlayer = new Random().Next(0, currentPlayers);

            //Play the first moves of every agent
            for (currentPlayer = 0; currentPlayer < currentPlayers; currentPlayer++)
            {
                playFirstMoves((firstPlayer + currentPlayer) % currentPlayers);
            }

            //Set the current player
            currentPlayer = firstPlayer;

            //Start playing the game until it's finished
            while (!env_gameIsOver())
            {
                playGame();
            }

            //End of game
            env_end();
        }

        //Occurs when a game is finished
        private void env_end()
        {
            //Add moves
            moves.Add(stepCounter);

            //Stop the timer
            timer.Stop();
            double tmp = timer.ElapsedMilliseconds;
            times.Add(tmp);
            bool found = false;

            //Find the last alive agent and send his reward signal
            for (int i = 0; i < gamePlayers.Count; i++)
            {
                if (gamePlayers[i].isAlive)
                {
                    found = true;
                    winners.Add(i);
                    averageMoney[i] += gamePlayers[i].money;
                    gamePlayers[i].agent_end(WINREWARD);
                    break;
                }
            }

            if (!found)
                winners.Add(-1);
        }

        //Select next agent
        public void env_selectNextAgent()
        {
            /*
            gamePlayers[0].saveOnFile("agents/nn" + currentGame.ToString() + "games--0.dat");
            gamePlayers[1].saveOnFile("agents/nn" + currentGame.ToString() + "games--1.dat");
            gamePlayers[2].saveOnFile("agents/nn" + currentGame.ToString() + "games--2.dat");
            */

            //Check whether the maximum allowed number of steps has occurred
            //If so then end the game
            if (stepCounter >= MAXSTEPS)
            {
                //Declare all players as losers
                for (int i = 0; i < gamePlayers.Count; i++)
                {
                    if (gamePlayers[i].isAlive)
                        removePlayer(i);
                }

                return;
            }


            //For some reason it's freaking freezing...
            System.Threading.Thread.Sleep(15);

            //Since it's a new player he definately hasn't rolled any doubles yet
            doublesInRow = 0;

            int playersChecked = 0;

            //Find the id of the next alive agent
            do
            {
                currentPlayer++;
                currentPlayer = currentPlayer % currentPlayers;
                playersChecked++;
            } while (!gamePlayers[currentPlayer].isAlive && playersChecked<=currentPlayers);

            //Increase his move counter since he's been selected
            playerMoves[currentPlayer]++;
            
            stepCounter++;         
        }     
        
        //Specify whether the game is over
        public bool env_gameIsOver()
        {
            //Count how many players are still alive in the game
            int counter = 0;
            for (int i = 0; i < gamePlayers.Count; i++)
            {
                if (gamePlayers[i].isAlive)
                    counter++;
            }

            //If there are more than one alive player then the game isn't over yet
            if (counter > 1)
                return false;
            else
                return true;
        }

        //Clean up memory when the experiment is completed
        public void env_cleanup()
        {
            //Print average money of every player
            TextWriter averageMoneyWriter = new StreamWriter("txt/AverageMoney.txt");

            for (int i = 0; i < currentPlayers; i++)
                averageMoneyWriter.WriteLine((averageMoney[i] / totalGames).ToString());

            averageMoneyWriter.Close();

            //Dispose agents and save neural networks
            for (int i = 0; i < currentPlayers; i++)
            {
                gamePlayers[i].saveOnFile("agents/nnFinalNeural--"+i.ToString()+".dat");
                gamePlayers[i].agent_cleanup();
            }

            MessageBox.Show("Experiment finished");
        }

        //Initiliaze game parameters
        private void initGameParameters()
        {
            chanceCards = new List<CommandCard>();
            communityChestCards = new List<CommandCard>();

            gameCards = new List<PropertyCard>();
            gameCommandCards = new List<CommandCard>();

            currentHotels = 0;
            currentHouses = 0;

            currentPosition = 0;
            currentPlayer = 0;

            doublesInRow = 0;

            board = new Board();

            //Both CommandCards and PropertyCards implement the Card interface
            //Set Command Cards ( both Community Chest and Chance cards ) 
            initMethods.setCommandsCards();

            //Set Property Cards
            initMethods.setPropertyCards();

            //Create information for every position on board
            initMethods.setBoard();

            getOutOfJailTries = new int[currentPlayers];
            playerMoves = new int[currentPlayers];

            //Initialize arrays
            for (int i = 0; i < properties.Length; i++)
            { properties[i] = -1; buildings[i] = 0; }

            //Initialize array
            for (int i = 0; i < gameCardsGroup.Length; i++)
                completedGroups[i] = -1;

            for (int i = 0; i < gamePlayers.Count; i++)
            { getOutOfJailTries[i] = 0; playerMoves[i] = 1; }

        }

        //Play first move of the game
        public void playFirstMoves(int i)
        {
            //Move current player
            movePlayer(i);
            int group = -1;

            if (board.typeId[currentPosition].Equals(0))
                group = getCardFromPosition(currentPosition).getGroup();

            //Create an instance of the observation class
            Observation obs = createObservation();

            //Integer array to specify the actions
            int action = 0;

            //Pause thread
            System.Threading.Thread.Sleep(15);

            //If the current player is agent then sent him a message
            action = gamePlayers[currentPlayer].agent_start(obs);

            int[] actions = { action, group };

            if (group >= 0) 
                methods.receiveAction(actions);

            gamePlayers[currentPlayer].position = currentPosition;

            /* //If current property isn't owned yet start the bidding game
             if (properties[currentPosition].Equals(-1) && board.typeId[currentPosition].Equals(0))
             {
                 biddingWar(currentPosition);
             }*/
        }

        //Play the game
        public void playGame()
        {
            //Check whether the player is alive
            if (gamePlayers[currentPlayer].isAlive)
            {
                currentPosition = gamePlayers[currentPlayer].position;

                //Move player only if not in prison
                if (gamePlayers[currentPlayer].inJail)
                    inJailPosition();
                else
                {
                    movePlayer(currentPlayer);

                    //If he throws 3 times doubles in a row then send him to jail
                    if (doublesInRow == 3)
                    {
                        //Send him to jail
                        gamePlayers[currentPlayer].position = 10;
                        gamePlayers[currentPlayer].inJail = true;
                        currentPosition = 10;
                        doublesInRow = 0;

                        //If he goes to jail then select the next agent
                        env_selectNextAgent();
                    }
                    else
                    {
                        moved = true;

                        //While the player hasn't moved from a command card and his is still alive ( in case he has paid something)
                        while (moved && gamePlayers[currentPlayer].isAlive && (!gamePlayers[currentPlayer].inJail))
                        {
                            //Check where he landed
                            onPositionChanged();

                            moved = false;

                            //Check whether he is in a special Position or command card
                            if (specialPositions.Contains(currentPosition))
                                onSpecialPosition();
                            else if (chanceCardsPositions.Contains(currentPosition) || communityChestCardsPositions.Contains(currentPosition))
                                onCommandCard();
                        }

                        //If the player is still alive then procceed with the action selected
                        if (gamePlayers[currentPlayer].isAlive && (!gamePlayers[currentPlayer].inJail))
                        {
                            int tempPosition = currentPosition;
                            while (!board.typeId[tempPosition].Equals(0))
                                tempPosition++;

                            int group = getCardFromPosition(tempPosition).getGroup();

                            //List of actions
                            List<int[]> getList = new List<int[]>();
                            List<int[]> spendList = new List<int[]>();

                            for (int currentGroup = 0; currentGroup < gameCardsGroup.Length; currentGroup++)
                            {
                                group = getCardFromPosition(tempPosition).getGroup();
                                group = (group + currentGroup) % gameCardsGroup.Length;

                                #region CheckAbilityToAct

                                //Check whether the player can act on the specific group
                                bool ableToAct = false;
                                for (int i = 0; i < gameCardsGroup[group].Split(',').Length; i++)
                                {
                                    if (properties[Int32.Parse(gameCardsGroup[group].Split(',')[i])].Equals(currentPlayer))
                                        ableToAct = true;

                                    if (Int32.Parse(gameCardsGroup[group].Split(',')[i]).Equals(currentPosition))
                                    {
                                        if(properties[currentPosition].Equals(currentPlayer)||properties[currentPosition].Equals(-1))
                                            ableToAct = true;
                                    }
                                }

                                #endregion CheckAbilityToAct

                                if (ableToAct)
                                {
                                    //Integer to specify the action
                                    int action = 0;

                                    //Pause thread
                                    System.Threading.Thread.Sleep(20);

                                    //Create an instance of the observation class
                                    Observation obs = createObservation();

                                    #region ChangeCurrentObservation

                                    obs.position.relativePlayersArea = (double)(group + 1) / 10;

                                    #endregion ChangeCurrentObservation

//                                  for (int actCount = 0; actCount < MAXAGENTACTIONS; actCount++)
//                                  {
                                    action = gamePlayers[currentPlayer].agent_step(obs, calculateReward(currentPlayer));
                              
                                    if (currentPlayer.Equals(0))
                                        Awriter.WriteLine(action.ToString() + " -- " + group.ToString()); 

                                    if (!action.Equals(0))
                                    {
                                        int[] set = { action, group };
                                        if (action > 0)
                                            spendList.Add(set);
                                        else
                                            getList.Add(set);                                 
                                    }
//                                  }
                                }
                            } 

                            for (int i = 0; i < getList.Count; i++)
                            {
                                System.Threading.Thread.Sleep(5);
                                methods.receiveAction(getList[i]);
                            }

                            for (int i = 0; i < spendList.Count; i++)
                            {
                                System.Threading.Thread.Sleep(5);
                                methods.receiveAction(spendList[i]);
                            }

                            //If current property isn't owned yet start the bidding game
                            if (properties[currentPosition].Equals(-1) && board.typeId[currentPosition].Equals(0))
                            {
                                biddingWar(currentPosition);
                            }

                            //If he hasn't thrown doubles then select the next agent
                            if (doublesInRow.Equals(0))
                                env_selectNextAgent();
                        }

                       //If he is either dead or in jail select next agent
                        else
                        {
                            env_selectNextAgent();
                        }
                    }
                }
            }
            //If he isn't alive then select the next alive agent
            else
                env_selectNextAgent();

        }

        //Start the bidding
        private void biddingWar(int currentPosition)
        {
            if (properties[currentPosition].Equals(-1) && board.typeId[currentPosition].Equals(0))
            {

                //Find the group of the current card
                int group = getCardFromPosition(currentPosition).getGroup();


                //Start the bidding war until some player has outbid everyone else
                int higherBidder = -1;
                int totalBidders = 0;
                double multFactor = 0.4;
                bool finished = false;
                int maxBid = 0;

                while (!finished)
                {
                    int bid = (int)(multFactor * getCardFromPosition(currentPosition).getValue());

                    for (int i = 0; i < currentPlayers; i++)
                    {
                        if (gamePlayers[i].isAlive)
                        {
                            //Pause thread
                            System.Threading.Thread.Sleep(20);

                            Observation specObs = createObservation();

                            #region RelativeAssets

                            double total = 0;

                            for (int j = 0; j < gamePlayers.Count(); j++)
                            {
                                total += methods.mActions.caclulateAllAssets(j);
                            }

                            total += (getCardFromPosition(currentPosition).getMortgageValue() - bid);

                            double assets = (double)(((methods.mActions.caclulateAllAssets(i) + (getCardFromPosition(currentPosition).getMortgageValue() - bid))));

                            //Current player's money / Total money
                            specObs.finance.relativeAssets = assets / total;

                            #endregion RelativeAssets

                            #region RelativePlayersMoney

                            //Current player's money / Total money
                            specObs.finance.relativePlayersMoney = smoothFunction(gamePlayers[i].money - bid, 1500);


                            /*
             
                             total = 0;
                             for(int j=0;j<currentPlayers;j++)
                             {
                                    total+=gamePlayers[j].money;
                             }
             
                             assets = (double)((gamePlayers[i].money - bid ));
             
                             specObs.finance.relativePlayersMoney = assets/total ;
                             */


                            #endregion RelativePlayersMoney

                            #region RelativePlayersArea

                            //Relative players position
                            specObs.position.relativePlayersArea = (double)((double)(group + 1) / 10);

                            #endregion RelativePlayersArea

                            #region GameGroupInfo

                            specObs.area.gameGroupInfo[group, 0] += (gameCardsGroup[group].Split(',').Length / 12) / 17;

                            #endregion GameGroupInfo

                            int action = gamePlayers[i].agent_step(specObs, calculateReward(i));

                            if (i.Equals(0))
                                Awriter.WriteLine("BiddingTime " + action.ToString() + " -- " + group.ToString());

                            if (action > 0 && gamePlayers[i].money >= bid)
                            {
                                higherBidder = i;
                                totalBidders++;
                                maxBid = bid;
                            }
                        }
                    }

                    if (totalBidders > 1)
                    {
                        finished = false;
                        higherBidder = -1;
                        multFactor += 0.2;
                        totalBidders = 0;
                    }
                    else if (totalBidders == 0)
                    {
                        higherBidder = -1;
                        finished = true;
                    }
                    else
                        finished = true;
                }

                //If someone is chosen as a higher bidder then make him buy the current property
                if (!higherBidder.Equals(-1))
                {
                    properties[currentPosition] = higherBidder;
                    gamePlayers[higherBidder].propertiesPurchased[getIndexFromPosition(currentPosition)] = 1;
                    gamePlayers[higherBidder].money -= maxBid;
                    methods.mActions.checkIfCompleted(higherBidder, currentPosition);
                }
            }
        }

        //Print experiment's info
        private void printInfo()
        {
            //Check whether the directory exists or not
            if (!Directory.Exists("txt/"))
                Directory.CreateDirectory("txt/");

            //Create stream writer for the output file
            textWriter = new StreamWriter("txt/output.txt");
            TextWriter winner = new StreamWriter("txt/winners.txt");
            TextWriter move = new StreamWriter("txt/moves.txt");
         
            textWriter.WriteLine("=========== RL AGENTS =============");
            textWriter.WriteLine("Game----Time-----Winnner-----Moves-");
            for (int i = 0; i < winners.Count; i++)
            {
                textWriter.WriteLine((i + 1).ToString() + "        " + (times[i]/1000).ToString() + "        " + winners[i] + "     " + moves[i].ToString());
                winner.WriteLine(winners[i].ToString());
                move.WriteLine(moves[i].ToString());
            }

            move.Close();
            winner.Close();

        }


        #endregion Environment


        #endregion RLMethods


    }
}
