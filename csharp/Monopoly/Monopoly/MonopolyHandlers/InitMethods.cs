using System.Xml;
using System.IO;
using System.Windows;
using System;
using Monopoly.Classes;
using System.Collections.Generic;

namespace Monopoly.MonopolyHandlers
{
    class InitMethods
    {
        public InitMethods() { }

        #region SetMethods

        internal bool setCommandsCards()
        {
            try
            {
                //Check if file exists
                if (File.Exists("Data/CommandCards.xml"))
                {
                    //Create Xml reader to store the commmandCards
                    XmlDocument doc = new XmlDocument();
                    doc.Load("Data/CommandCards.xml");
                    XmlElement root = doc.DocumentElement;
                    XmlNodeList nodes = root.SelectNodes("//CommandCard");

                    //For every Command Card in the xml file add a new item to the global list ( in App.xaml.cs)
                    foreach (XmlNode node in nodes)
                    {
                        ((App)Application.Current).app.addCommandCard(new CommandCard(Int32.Parse(node["TypeOfCard"].InnerText.ToString()), node["Text"].InnerText.ToString(), node["FixedMove"].InnerText.ToString(), node["RelativeMove"].InnerText.ToString(), Int32.Parse(node["Collect"].InnerText.ToString()), Int32.Parse(node["MoneyTransaction"].InnerText.ToString()), Int32.Parse(node["PlayersInteraction"].InnerText.ToString()), Int32.Parse(node["HouseMultFactor"].InnerText.ToString()), Int32.Parse(node["HotelMultFactor"].InnerText.ToString())));
                    }

                    ((App)Application.Current).app.setCommandCards();

                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return false; }

            return true;
        }

        internal bool setPropertyCards()
        {
            try
            {
                //Check if file exists
                if (File.Exists("Data/Properties.xml"))
                {
                    //Create Xml reader to store the commmandCards
                    XmlDocument doc = new XmlDocument();
                    doc.Load("Data/Properties.xml");
                    XmlElement root = doc.DocumentElement;
                    XmlNodeList nodes = root.SelectNodes("//Property");

                    //For every Property Card in the xml file add a new item to the global list ( in App.xaml.cs)
                    foreach (XmlNode node in nodes)
                    {
                        //Since the rent differs for groups (0-7), 8 and 9
                        //create a list and not an array in order to store dynamically the desired values
                        //In the xml file, each value of rent is seperated by a comma ( , ) 
                        List<int> rent = new List<int>();
                        string[] rentString = node["Rent"].InnerText.ToString().Split(',');
                        foreach (string s in rentString)
                        {
                            rent.Add(Int32.Parse(s));
                        }

                        ((App)Application.Current).app.addCard(new PropertyCard(node["Name"].InnerText.ToString(), Int32.Parse(node["Position"].InnerText.ToString()), Int32.Parse(node["Price"].InnerText.ToString()), rent, Int32.Parse(node["Mortage"].InnerText.ToString()), Int32.Parse(node["HouseCost"].InnerText.ToString()), Int32.Parse(node["HotelCost"].InnerText.ToString()), Int32.Parse(node["Group"].InnerText.ToString())));

                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return false; }

            return true;
        }

        internal void setBoard()
        {
            Card[] b = new Card[40];
            int[] t = new int[40];

            for (int i = 0; i < 40; i++)
                t[i] = -1;


            //Add PropertyCards
            for (int i = 0; i < ((App)Application.Current).app.getCards().Count; i++)
            {
                b[((App)Application.Current).app.getCards()[i].getPosition()] = ((App)Application.Current).app.getCards()[i];
                t[((App)Application.Current).app.getCards()[i].getPosition()] = 0;
            }

            //Add ComunityChestCards
            for (int i = 0; i < ((App)Application.Current).app.getCommunityCardPositions().Length; i++)
            {
                b[((App)Application.Current).app.getCommunityCardPositions()[i]] = new CommandCard();
                t[((App)Application.Current).app.getCommunityCardPositions()[i]] = 1;
            }

            //Add ChanceCards
            for (int i = 0; i < ((App)Application.Current).app.getChanceCardPositions().Length; i++)
            {
                b[((App)Application.Current).app.getChanceCardPositions()[i]] = new CommandCard();
                t[((App)Application.Current).app.getChanceCardPositions()[i]] = 2;
            }

            //Specify that every position left on board is a special position ( GO, Jail, etc... )
            //We'll take care of what occurs on every case in a different method
            for (int i = 0; i < b.Length; i++)
            {
                if(t[i]<0)
                {
                    t[i] = 3;
                    b[i] = new SpecialPositionCard();
                }
            }

            //Set the global board parameter
            ((App)Application.Current).app.setBoard(new Board(b, t));

        }

        #endregion SetMethods

    }
}