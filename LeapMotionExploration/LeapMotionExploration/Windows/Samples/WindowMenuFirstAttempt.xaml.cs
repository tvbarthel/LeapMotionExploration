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
using Leap;
using MyLeap.Listener;
using MyLeap.Event;

namespace LeapMotionExploration
{
    /// <summary>
    /// Logique d'interaction pour WindowMenuFirstAttempt.xaml
    /// </summary>
    public partial class WindowMenuFirstAttempt : Window
    {
        private TextBlock[] textBlockMenuItems;
        private Controller mController;
        private LeapListenerRotateSelection mListener;
        private int currentSelectedItem;
        
        public WindowMenuFirstAttempt()
        {
            InitializeComponent();
            textBlockMenuItems = new TextBlock[]{textBlockItem1, textBlockItem2, textBlockItem3, textBlockItem4, textBlockItem5, textBlockItem6};
            currentSelectedItem = 0;
            selectItem(currentSelectedItem);

            mController = new Controller();
            mListener = new LeapListenerRotateSelection();
            mController.AddListener(mListener);
            mListener.OnStateChange += OnSelectionChanged;
        }

        public void OnSelectionChanged(LeapEvent leapEvent)
        {
            switch (leapEvent.Type)
            {
                case LeapEvent.ROTATION_SELECTION_START:
                    setMenuVisibility(Visibility.Visible);
                    break;
                case LeapEvent.ROTATION_SELECTION_END:
                    setMenuVisibility(Visibility.Hidden);
                    break;
                case LeapEvent.ROTATION_SELECTION_NEXT:
                    selectNext();
                    break;
                case LeapEvent.ROTATION_SELECTION_PREVIOUS:
                    selectPrevious();
                    break;
            }
        }

        private void selectItem(int i)
        {
            menuContainer.RenderTransform = new RotateTransform(90 - 45 * i);
            foreach(TextBlock textBlock in textBlockMenuItems) {
                textBlock.Opacity = 0.4;
            }
            textBlockMenuItems[i].Opacity = 1;
        }

        private void selectNext()
        {
            currentSelectedItem = Math.Min(5, currentSelectedItem + 1);            
            updateCurrentSelection();
        }

        private void selectPrevious()
        {
            currentSelectedItem = Math.Max(0, currentSelectedItem - 1);
            updateCurrentSelection();
        }

        private void updateCurrentSelection()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                selectItem(currentSelectedItem);
            }));
        }


        private void setMenuVisibility(Visibility visibility)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                menuContainer.Visibility = visibility;
            }));
        }



        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            mController.RemoveListener(mListener);
            mController.Dispose();
            base.OnClosing(e);
        }
    }
}
