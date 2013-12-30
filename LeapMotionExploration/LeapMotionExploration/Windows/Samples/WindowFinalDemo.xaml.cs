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
using MyLeap.Event;

namespace LeapMotionExploration.Windows.Samples
{
    /// <summary>
    /// Logique d'interaction pour WindowDemo.xaml
    /// </summary>
    public partial class WindowFinalDemo : Window
    {
        public WindowFinalDemo()
        {
            InitializeComponent();
        }

        private void OnHandClosed(HandCloseEvent e)
        {
            switch (e.Type)
            {
                case HandCloseEvent.OPEN:
                    //TODO if closed & dragging called DragFinished
                    break;
                case HandCloseEvent.CLOSE:
                    //TODO check the event position to know if an Ui element has been selected
                    break;
            }
        }

        private void DragStarted()
        {
            //TODO inform drag started
            //TODO store original position of the element

        }

        private void DragMoved()
        {
            //TODO get the cursor position
            //TODO calculate the offset
            //TODO update the element position

        }

        private void DragFinished(bool cancelled)
        {
            //TODO inform drag stop
            //if cancelled reset position with original position
        }
    }
}
