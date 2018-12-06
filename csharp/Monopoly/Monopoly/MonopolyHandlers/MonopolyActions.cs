using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Monopoly.RLHandlers;

namespace Monopoly.MonopolyHandlers
{
    public class MonopolyActions
    {
        public MonopolyActions() { }

        //Try to buy a property on board
        public int buyProperty(int player, int property)
        {
            //If the specific position isn't a property ( Command Card or Special Position) then don't do anything
            if (((App)Application.Current).app.board.typeId[property].Equals(0))
            {
                //If the player already has this property to his possession then return -1;
                if (((App)Application.Current).app.getPlayers()[player].propertiesPurchased[((App)Application.Current).app.getIndexFromPosition(property)].Equals(1))
                    return -1;

                //If the property isn't currently owned by anyone 
                if (((App)Application.Current).app.properties[property] < 0)
                {
                    if (((App)Application.Current).app.gamePlayers[player].money >= ((App)Application.Current).app.gameCards[((App)Application.Current).app.getIndexFromPosition(property)].getValue())
                    {
                        //Then assign this property to the player ( both on his fields and on the global public variables
                        ((App)Application.Current).app.getPlayers()[player].propertiesPurchased[((App)Application.Current).app.getIndexFromPosition(property)] = 1;

                        ((App)Application.Current).app.properties[property] = player;

                        ((App)Application.Current).app.gamePlayers[player].money -= ((App)Application.Current).app.gameCards[((App)Application.Current).app.getIndexFromPosition(property)].getValue();

                        //Check whether with this buy he has just completed the form of a group.
                        checkIfCompleted(player, property);

                        //Success
                        return 1;
                    }
                }
                else return -1;
            }
            return -1;
        }

        //Unmortgage a property
        public int unmortgageProperty(int player, int property)
        {
            //If the current position on the board refers to a property card
            if (((App)Application.Current).app.board.typeId[property].Equals(0))
            {
                if (!(((App)Application.Current).app.properties[property].Equals(player)))
                    return -1;

                //Then check whether the user has mortgaged this property,if not then return with an error code (-1).
                if (((App)Application.Current).app.getPlayers()[player].mortgagedProperties[((App)Application.Current).app.getIndexFromPosition(property)].Equals(0))
                    return -1;

                if (((App)Application.Current).app.gamePlayers[player].money >= (int)(1.1 * ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(property)].getMortgageValue()))
                {
                    //Otherwise mark this area as unmortgaged
                    ((App)Application.Current).app.getPlayers()[player].mortgagedProperties[((App)Application.Current).app.getIndexFromPosition(property)] = 0;
                    
                    //Check whether with this buy he has just completed the form of a group.
                    checkIfCompleted(player, property);

                    ((App)Application.Current).app.gamePlayers[player].money -= (int)(1.1 * ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(property)].getMortgageValue());

                    //Success
                    return 1;
                }
            }

            return -1;
        }

        //Build on an area
        public int buildOnArea(int player, int group)
        {
            //If the group is smaller than 8 ( not Railways or Electric/Water Works ) and the player has completed this group
            if (group > 1 && ((App)Application.Current).app.completedGroups[group] == player)
            {     
                //Try to find the property in that specific group with the fewest buildings on it
                int minBuilding = 6;
                int propertyToBuild = -1;

                string[] temp = ((App)Application.Current).app.gameCardsGroup[group].Split(',');
                for (int i = 0; i < temp.Length; i++)
                {
                    if (((App)Application.Current).app.gamePlayers[player].mortgagedProperties[((App)Application.Current).app.getIndexFromPosition(Int32.Parse(temp[i]))].Equals(1))
                        return -1;

                    if (((App)Application.Current).app.buildings[Int32.Parse(temp[i])] <= minBuilding)
                    {
                        minBuilding = ((App)Application.Current).app.buildings[Int32.Parse(temp[i])];
                        propertyToBuild = Int32.Parse(temp[i]);
                    }
                }

                //If the minimum number of buildings that we found are less than 5 ( so that we can build a hotel at least ) procceed with the build
                if (minBuilding < 5)
                {
                    //Check whether we can build another house
                    if (((App)Application.Current).app.currentHouses.Equals(((App)Application.Current).app.getMaxHouses()))
                        return -1;

                    //Check whether the player's building a hotel or a house
                    if (((App)Application.Current).app.buildings[propertyToBuild] < 5)
                    {
                        if (((App)Application.Current).app.gamePlayers[player].money >= ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(propertyToBuild)].getHouseCost())
                        {
                            ((App)Application.Current).app.currentHouses++;

                            //Add the info to the player's private fields
                            ((App)Application.Current).app.getPlayers()[player].buildingsBuilt[((App)Application.Current).app.getIndexFromPosition(propertyToBuild)]++;
                            ((App)Application.Current).app.buildings[propertyToBuild]++;

                            ((App)Application.Current).app.gamePlayers[player].money -= ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(propertyToBuild)].getHouseCost();
                        }
                    }
                    else
                    {
                        //Check whether we can build another hotel
                        if (((App)Application.Current).app.currentHouses.Equals(((App)Application.Current).app.getMaxHotels()))
                            return -1;

                        if (((App)Application.Current).app.gamePlayers[player].money >= ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(propertyToBuild)].getHotelCost())
                        {

                            ((App)Application.Current).app.currentHotels++;
                            ((App)Application.Current).app.currentHouses -= 4;

                            //Add the info to the player's private fields
                            ((App)Application.Current).app.getPlayers()[player].buildingsBuilt[((App)Application.Current).app.getIndexFromPosition(propertyToBuild)]++;
                            ((App)Application.Current).app.buildings[propertyToBuild]++;

                            ((App)Application.Current).app.gamePlayers[player].money -= ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(propertyToBuild)].getHotelCost();
                        }

                    }

                    return 1;
                }
            }

            return -1;
        }

        //Mortgage a property
        public int mortgageProperty(int player, int property)
        {
            //If the current position refers to a property card then procceed
            if (((App)Application.Current).app.board.typeId[property].Equals(0))
            {
                if (!(((App)Application.Current).app.properties[property].Equals(player)))
                    return -1;

                int group = ((App)Application.Current).app.gameCards[((App)Application.Current).app.getIndexFromPosition(property)].getGroup();
                for (int i = 0; i < ((App)Application.Current).app.gameCardsGroup[group].Split(',').Length; i++)
                {
                    int tmpProp = Int32.Parse(((App)Application.Current).app.gameCardsGroup[group].Split(',')[i]);
                    if (((App)Application.Current).app.buildings[tmpProp] > 0)
                        return -1;
                }

                //If the user has already mortgaged this area then return an error code (-1)
                if (((App)Application.Current).app.getPlayers()[player].mortgagedProperties[((App)Application.Current).app.getIndexFromPosition(property)].Equals(1))
                    return -1;

                //If the player has this property under his possesion procceed with the mortgage
                if (((App)Application.Current).app.getPlayers()[player].propertiesPurchased[((App)Application.Current).app.getIndexFromPosition(property)].Equals(1))
                {
                    //Mark this property as mortgaged
                    ((App)Application.Current).app.getPlayers()[player].mortgagedProperties[((App)Application.Current).app.getIndexFromPosition(property)] = 1;

                    //Add the money to his balance
                    ((App)Application.Current).app.getPlayers()[player].money += ((App)Application.Current).app.getCardFromPosition(property).getMortgageValue();

                    //Update the completed groups
                    checkIfCompleted(player, property);

                    //Success
                    return 1;
                }
                return -1;

            }
            return -1;
        }

        //Sell on an area
        public int sellOnArea(int player, int group)
        {
            //If the group is smaller than 8 ( not Railways or Electric/Water Works ) and the player has completed this group
            if (group > 1 && ((App)Application.Current).app.completedGroups[group] == player)
            {
                //We'll try to find the property with the maximum number of buildings built on it
                int maxBuilding = 0;
                int propertyToSell = -1;

                string[] temp = ((App)Application.Current).app.gameCardsGroup[group].Split(',');
                for (int i = 0; i < temp.Length; i++)
                {
                    if (((App)Application.Current).app.buildings[Int32.Parse(temp[i])] >= maxBuilding)
                    {
                        maxBuilding = ((App)Application.Current).app.buildings[Int32.Parse(temp[i])];
                        propertyToSell = Int32.Parse(temp[i]);
                    }
                }

                //If there are buildings available to sell then procceed
                if (maxBuilding > 0)
                {
                    //Update the variable
                    ((App)Application.Current).app.buildings[propertyToSell]--;

                    //Check whether it was a hotel or a house to add the proper amount of money to the player's balance
                    if (((App)Application.Current).app.buildings[propertyToSell] < 4)
                    {
                        ((App)Application.Current).app.currentHouses--;
                        ((App)Application.Current).app.getPlayers()[player].money += (int)(0.5 * ((App)Application.Current).app.getCardFromPosition(propertyToSell).getHouseCost());
                    }
                    else
                    {
                        ((App)Application.Current).app.currentHotels--;
                        ((App)Application.Current).app.currentHouses += 4;
                        ((App)Application.Current).app.getPlayers()[player].money += (int)(0.5 * ((App)Application.Current).app.getCardFromPosition(propertyToSell).getHotelCost());
                    }

                    //Update player's personal fields
                    ((App)Application.Current).app.getPlayers()[player].buildingsBuilt[((App)Application.Current).app.getIndexFromPosition(propertyToSell)]--;

                    //Success
                    return 1;
                }
            }

            //try to mortgage the most expensive 
            else
            {
                string[] tmp = ((App)Application.Current).app.gameCardsGroup[group].Split(',');
                for (int i = 0; i < tmp.Length; i++)
                {
                    if(((App)Application.Current).app.properties[Int32.Parse(tmp[i])].Equals(player))
                        mortgageProperty(player,Int32.Parse(tmp[i]));
                }
            }

            return -1;
        }

        //Helper method to check whether a group is completed or not
        public void checkIfCompleted(int player, int cp)
        {
            //If the position refers to a property card
            if (((App)Application.Current).app.board.typeId[cp].Equals(0))
            {
                //We firstly identify the group
                int group = ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(cp)].getGroup();

                //boolean variable to determine whether the group is completed
                bool isCompleted = true;
                string[] tmp = ((App)Application.Current).app.gameCardsGroup[group].Split(',');

                //If we find at least one property where it doesn't belong to the current player then the group isn't complete.
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (((App)Application.Current).app.getPlayers()[player].propertiesPurchased[((App)Application.Current).app.getIndexFromPosition(Int32.Parse(tmp[i]))].Equals(0))
                        isCompleted = false;
                }

                //Update the specified info
                if (isCompleted)
                    ((App)Application.Current).app.completedGroups[group] = player;
                else
                    ((App)Application.Current).app.completedGroups[group] = -1;
            }
        }

        //Pay to either the bank or to another player
        public int payMoney(int pFrom, int pTo, int amount)
        {
            if (!(((App)Application.Current).app.gamePlayers[pFrom].isAlive))
                return -1;

            //If pTo = -1 then it is the bank
            //Check wheter the current player has the required amount of money to pay
            if (!checkPayment(pFrom, amount))
            {
                //If he can't pay then return -1
                return -1;
            }
            else
            {
/*                if (((App)Application.Current).app.gamePlayers[pFrom].money < amount)
                {
                    int infBlocker = 0;
                    //If he doesn't have that cash available starting selling his property until he is able to pay
                    while (((App)Application.Current).app.gamePlayers[pFrom].money < amount)
                    {
                        infBlocker++; if (infBlocker > 100) { if (!checkPayment(pFrom, amount))return -1; }

                        if (!((App)Application.Current).app.gamePlayers[pFrom].isAlive)
                            return -1;

                        sellProperty(pFrom);

                    }
                }
 */

                //Procceed with the payment
                ((App)Application.Current).app.gamePlayers[pFrom].money -= amount;
                if (pTo > -1)
                {
                    ((App)Application.Current).app.gamePlayers[pTo].money += amount;
                }

                return 1;
            }
        }

        //Sell a property ( or a building ) from player in order to generate more cash
        private void sellProperty(int player)
        {
            //Here we'll use the selloOnArea method created above in order to get more cash by either 
            //selling buildings or mortgaging properties starting from the most expensive group
            int area = 9;
            bool done= false;

            while (!done)
            {
                if (sellOnArea(player, area) > 0)
                    done = true;
                if (!done)
                    area--;
                if (area < 2)
                    done = true;
            }

            int pos = 39;
            done = false;
            while (!done)
            { 
                if (mortgageProperty(player, pos) > 0)
                        done = true;
                
                if (!done)
                    pos--;
                if (pos < 0)
                    done = true;
            }
            
        }

        //Check whether the user can pay the amount
        public bool checkPayment(int player, int amount)
        {
            if (((App)Application.Current).app.gamePlayers[player].money >= amount)
                return true;
            else
                return false;
        }

        //Calculate value of all the player's assets
        public int caclulateAllAssets(int player)
        {
            double total = 0;
            
            //Start by adding the player's cash to the total amount
            total += ((App)Application.Current).app.gamePlayers[player].money;

            //Add all the value of all the buildings and the potential mortgages he has
            for (int i = 0; i < ((App)Application.Current).app.gamePlayers[player].buildingsBuilt.Length; i++)
            {
                if (((App)Application.Current).app.gamePlayers[player].buildingsBuilt[i] < 4)
                    total += ((App)Application.Current).app.gamePlayers[player].buildingsBuilt[i] * (0.5 * ((App)Application.Current).app.gameCards[i].getHouseCost());
                else
                {
                    total += (4 * (0.5 * ((App)Application.Current).app.gameCards[i].getHouseCost()));
                    total += (0.5 * ((App)Application.Current).app.gameCards[i].getHotelCost());
                }

                if (((App)Application.Current).app.gamePlayers[player].propertiesPurchased[i].Equals(1) && ((App)Application.Current).app.gamePlayers[player].mortgagedProperties[i].Equals(0))
                    total += ((App)Application.Current).app.gameCards[i].getMortgageValue();
            }

            return (int)total;
        }
    }
}
