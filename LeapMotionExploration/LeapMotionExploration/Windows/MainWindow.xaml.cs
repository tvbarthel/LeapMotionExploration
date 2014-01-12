using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LeapMotionExploration.Windows.Samples;
using MyLeap.Utils;

namespace LeapMotionExploration.Windows
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            buttonColorExploration.Click += buttonClick;
            buttonRightMostPointer.Click += buttonClick;
            buttonLeftMostPointer.Click += buttonClick;
            buttonOpenCloseHand.Click += buttonClick;
            buttonRotatingMenuFirstAttempt.Click += buttonClick;
            buttonTwoHandManipulation.Click += buttonClick;
            buttonClapMusic.Click += buttonClick;
            buttonClap.Click += buttonClick;
            buttonFinalDemoRightHanded.Click += buttonClick;
            buttonFinalDemoLeftHanded.Click += buttonClick;
            buttonDispatcherTest.Click += buttonClick;
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(buttonColorExploration))
            {
                new WindowColorExplorer().Show();
            }
            else if (sender.Equals(buttonRightMostPointer))
            {
                new WindowCursorDemo(LeapUtils.RIGHT_MOST_HAND).Show();
            }
            else if (sender.Equals(buttonLeftMostPointer))
            {
                new WindowCursorDemo(LeapUtils.LEFT_MOST_HAND).Show();
            }
            else if (sender.Equals(buttonOpenCloseHand))
            {
                new WindowOpenCloseHand().Show();
            }
            else if (sender.Equals(buttonRotatingMenuFirstAttempt))
            {
                new WindowMenuFirstAttempt().Show();
            }
            else if (sender.Equals(buttonTwoHandManipulation))
            {
                new WindowTwoHandManipulation().Show();
            }
            else if (sender.Equals(buttonClap))
            {
                new WindowClap().Show();
            }
            else if(sender.Equals(buttonClapMusic))
            {
                new WindowClapSound().Show();
            }
            else if (sender.Equals(buttonFinalDemoRightHanded))
            {
                new WindowFinalDemo(LeapUtils.RIGHT_MOST_HAND).Show();
            }
            else if(sender.Equals(buttonFinalDemoLeftHanded))
            {
                new WindowFinalDemo(LeapUtils.LEFT_MOST_HAND).Show();
            }
            else if (sender.Equals(buttonDispatcherTest))
            {
                new WindowDispatcher().Show();
            }
        }
    }
}
