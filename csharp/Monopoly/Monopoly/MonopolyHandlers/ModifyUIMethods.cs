using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Monopoly.RLClasses;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace Monopoly.MonopolyHandlers
{
    //Helper class to modify the UI
    class ModifyUIMethods
    {
        public ModifyUIMethods() { }

        //Set the currentCardGrid's UI. Display the info of the current active card ( changes every round ).
        public void setCurrentCard(string name, string info, string owner)
        {
            MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
            mainWindow.backgroundTb.Background = ((App)Application.Current).app.getColour(name);
            mainWindow.cardInfoTb.Text = name + System.Environment.NewLine + info;
            if (!owner.Equals("none")) mainWindow.cardOwnerTb.Text = "Owner : " + owner;
            else mainWindow.cardOwnerTb.Text = "Not purchased yet";
        }

        //Set info about agent ( full details )
        public void setDetailedInfo(string agentName, Observation obs, int[] action, int position)
        {
            string info = Environment.NewLine + "---------------------------------" + Environment.NewLine + agentName + " is next.Currently on position : " + position.ToString();
            info += Environment.NewLine + "Observation received : " + obs.printInfo();
        //    info += Environment.NewLine + "Action selected :  ";
            for (int i = 0; i < action.Length; i++)
            { info += action[i].ToString() + ","; }

            info.Remove(info.Length - 1);
            MainWindow mw = (MainWindow)Application.Current.MainWindow;
            mw.obsInfo.Text = info;

         //   MessageBox.Show(info);
          
        }

        //Set info about agent
        public void setInfo(string agentName, int position)
        {
           string info = Environment.NewLine + "---------------------------------" + Environment.NewLine + agentName + " is next. Currently on position : " + position.ToString();

           MessageBox.Show(info);
        }

        //Set images
        public void setImages(int id, int position)
        {
             MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
              foreach (Image image in mainWindow.boardGrid.Children)
              {
                  if (image.Name.Equals("pos"+position.ToString()))
                      image.Source = new BitmapImage(new Uri("/Images/Players/" + (id+1).ToString() + ".jpg", UriKind.Relative));
              }
        }

        //Reset all images
        public void resetImages()
        {
            MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
            foreach (Image image in mainWindow.boardGrid.Children)
            {
                if (!image.Name.Equals("mainBackground"))
                    image.Source = null;
            } 
        }

    }
}