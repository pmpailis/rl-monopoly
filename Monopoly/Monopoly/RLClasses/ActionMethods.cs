using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Monopoly.Classes;
using Monopoly.MonopolyHandlers;

namespace Monopoly.RLClasses
{
    //Helper class for determining and applying actions
    public class ActionMethods
    {
        public MonopolyActions mActions;

        //Constructor of the class
        public ActionMethods() { mActions = new MonopolyActions(); }

        //Receive observation and an action array
        public void receiveAction(int[] actions)
        {
            //Current position of the current player
            int cp = ((App)Application.Current).app.currentPosition;
            int action = actions[0];
            int group = actions[1];

            //Spend Money to specified group
            if (action > 0)
            {
                //If the current position of the user refer to a property card 
                if ((((App)Application.Current).app.board.typeId[cp].Equals(0)))
                {
                    //If his current position is in one of the selected groups then act on this specific position of the group
                    if (((App)Application.Current).app.getCardFromPosition(cp).getGroup().Equals(group))
                        spendMoneyOnPosition(cp);
                    else
                        spendMoneyOnArea(group);
                }

                //Otherwise spend money on the area that has selected
                else
                    spendMoneyOnArea(group);
            }

           //Get money from the specified group
            else if (action < 0)
            {
                getMoneyFromArea(group);
            }
        }

        #region SpendMethods

        //Spend money on specific position on board
        private void spendMoneyOnPosition(int cp)
        {
            //Get money from area based on a priority list (maximum earning)
            //We firstly try to unmortgage the property, then to buy it and final to build on the selected area
            if (mActions.buyProperty((((App)Application.Current).app.currentPlayer), cp) < 0)
            {
                if (mActions.unmortgageProperty((((App)Application.Current).app.currentPlayer), cp) < 0)
                    mActions.buildOnArea((((App)Application.Current).app.currentPlayer), ((App)Application.Current).app.getCards()[((App)Application.Current).app.getIndexFromPosition(cp)].getGroup());
            }
        }


        //Spend money on a specific area
        private void spendMoneyOnArea(int area)
        {
            //Spend money on area based on a priority list ( potential rent)
            bool done = false;
            for (int i = 0; i < ((App)Application.Current).app.gameCardsGroup[area].Split(',').Count(); i++)
            {
                int pos = Int32.Parse(((App)Application.Current).app.gameCardsGroup[area].Split(',')[i].ToString());
                if (mActions.unmortgageProperty(((App)Application.Current).app.currentPlayer, pos) > 0)
                {
                    done = true;
                    break;
                }
            }

            if (!done)
                mActions.buildOnArea((((App)Application.Current).app.currentPlayer), area);
        }


        #endregion SpendMethods


        #region GetMethods


        //Get money from specific area
        private void getMoneyFromArea(int area)
        {
            //Get money from area based on a priority list (maximum earning)
            if (mActions.sellOnArea((((App)Application.Current).app.currentPlayer), area) < 0)
            {
                //If we can't sell on the specific group then try to mortgage an area of the group
                string[] tmp = ((App)Application.Current).app.gameCardsGroup[area].Split(',');
                for (int j = 0; j < tmp.Length; j++)
                {
                    if (((App)Application.Current).app.getPlayers()[((App)Application.Current).app.currentPlayer].propertiesPurchased[((App)Application.Current).app.getIndexFromPosition(Int32.Parse(tmp[j]))].Equals(1))
                    {
                        if (((App)Application.Current).app.getPlayers()[((App)Application.Current).app.currentPlayer].mortgagedProperties[((App)Application.Current).app.getIndexFromPosition(Int32.Parse(tmp[j]))].Equals(0))
                        {
                            if (mActions.mortgageProperty((((App)Application.Current).app.currentPlayer), Int32.Parse(tmp[j])) > 0)
                                break;
                        }
                    }
                }
            }
        }


        #endregion GetMethods


    }
}

