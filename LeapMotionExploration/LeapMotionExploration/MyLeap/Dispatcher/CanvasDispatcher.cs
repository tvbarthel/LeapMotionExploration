using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MyLeap.Interface;
using MyLeap.Listener;
using MyLeap.Event;
using Leap;

namespace MyLeap.Dispatcher
{
    class CanvasDispatcher
    {

        // container of all FrameworkElement
        private Canvas _canvas;

        //list of all ui element register for clap event
        private Dictionary<FrameworkElement, ILeapListenerClap> _iClapListeners;

        //Leap controller
        private Controller _controller;

        //Clap listener
        private LeapListenerClap _leapClapListener;

        public CanvasDispatcher(Canvas c)
        {
            _canvas = c;
            _controller = new Controller();
        }

        /**
         * Register clap listener for a graphic element
         */
        public void AddClapListener(FrameworkElement uiElement, ILeapListenerClap i)
        {

            //check if first element, dictonary must be created and also linked the listener
            if (_iClapListeners == null)
            {
                _iClapListeners = new Dictionary<FrameworkElement, ILeapListenerClap>();
                _leapClapListener = new LeapListenerClap();
                _leapClapListener.OnClapDetected += this.OnClapDetected;
                _controller.AddListener(_leapClapListener);
            }

            if (_iClapListeners.ContainsKey(uiElement))
            {
                //if already added, just change the registered interface
                _iClapListeners[uiElement] = i;
            }
            else
            {
                //first registration, simply add
                _iClapListeners.Add(uiElement, i);
            }
        }

        /**
         * Unregister clap listener 
         */
        public void RemoveClapListener(FrameworkElement uiElement)
        {
            if (_iClapListeners != null && _iClapListeners.ContainsKey(uiElement))
            {
                _iClapListeners.Remove(uiElement);
            }
        }

        /**
         * Called when clap gesture is recognized. 
         * Search if the event has happened on a registered item and call the right interface.
         */ 
        private void OnClapDetected(LeapEvent e)
        {
            if (_iClapListeners != null && _iClapListeners.Count > 0)
            {
                foreach (KeyValuePair<FrameworkElement, ILeapListenerClap> entry in _iClapListeners)
                {
                    if (IsCursorOnGraphicElement(entry.Key, e.Position.x, e.Position.y))
                    {
                        entry.Value.OnClapDetected(e);
                    }
                }
            }
        }

        /**
         * Usefull methode used to know if the cursor or an event happened on a graphic element
         */ 
        private Boolean IsCursorOnGraphicElement(FrameworkElement graphicElement, double posX, double posY)
        {
            //Look for a top reference
            double graphicElementTop = Canvas.GetTop(graphicElement);
            if (double.IsNaN(graphicElementTop))
            {
                graphicElementTop = _canvas.ActualHeight - Canvas.GetBottom(graphicElement) - graphicElement.ActualHeight;
            }

            //Look for a left reference
            double graphicElementLeft = Canvas.GetLeft(graphicElement);
            if (double.IsNaN(graphicElementLeft))
            {
                graphicElementLeft = _canvas.ActualWidth - Canvas.GetRight(graphicElement) - graphicElement.ActualWidth;
            }

            return !double.IsNaN(graphicElementLeft) && !double.IsNaN(graphicElementLeft) && posX > graphicElementLeft && posX < (graphicElementLeft + graphicElement.ActualWidth) && posY > graphicElementTop && posY < (graphicElementTop + graphicElement.ActualHeight);
        }

    }
}
