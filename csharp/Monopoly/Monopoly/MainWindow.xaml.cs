using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Monopoly.Classes;

namespace Monopoly
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Initialize global variables of the app
            Loaded += new RoutedEventHandler(MainWindow_Loaded);

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Initialize main parameters, so we can use them 
            // ((App)Application.Current).app.env_init();    
        }

        private void moveBt_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).app.env_init();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).app.playGame();
        }
    }
}