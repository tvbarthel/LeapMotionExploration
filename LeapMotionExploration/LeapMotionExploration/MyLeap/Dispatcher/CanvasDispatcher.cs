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
using MyLeap.Utils;
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

        //Cursor listenr
        private LeapListenerOneHandPosition _leapCursorListener;

        //Cursor element
        private FrameworkElement _leapCursor;

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
         * Register ui element wich act as a cursor
         */
        public void SetCursor(FrameworkElement cursor, int hand)
        {
            if (_leapCursorListener == null)
            {
                _leapCursorListener = new LeapListenerOneHandPosition(hand);
                _controller.AddListener(_leapCursorListener);
                _leapCursorListener.OnStateChange += this.OnPositionChange;
            }

            _leapCursor = cursor;
        }

        /**
         * Use to remove all Listener and sipose leap
         */
        public void OnClose()
        {
            if (_controller != null)
            {

                if (_leapClapListener != null) _controller.RemoveListener(_leapClapListener);

                if (_leapCursorListener != null) _controller.RemoveListener(_leapCursorListener);

                _controller.Dispose();
            }

        }

        private void OnPositionChange(LeapEvent leapEvent)
        {
            Leap.Vector position = leapEvent.Position;
            if (_leapCursor != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {

                    double x = _canvas.ActualWidth * position.x;
                    double y = _canvas.ActualHeight * (1 - position.y);
                    _leapCursor.SetValue(Canvas.TopProperty, y - _leapCursor.Height / 2);
                    _leapCursor.SetValue(Canvas.LeftProperty, x - _leapCursor.Width / 2);


                }));
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
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (KeyValuePair<FrameworkElement, ILeapListenerClap> entry in _iClapListeners)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Position.x + " " + e.Position.y);

                        if (IsCursorOnGraphicElement(entry.Key, Canvas.GetLeft(_leapCursor), Canvas.GetTop(_leapCursor)))
                        {
                            e.SetSource(entry.Key);
                            entry.Value.OnClapDetected(e);
                            System.Diagnostics.Debug.WriteLine("Clap on entry !");
                        }
                    }
                }));
            }
        }

        /**
         * Usefull methode used to know if the cursor or an event happened on a graphic element
         */
        private Boolean IsCursorOnGraphicElement(FrameworkElement graphicElement, double posX, double posY)
        {
            Boolean res = false;
            //Look for a top reference
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
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

                res = !double.IsNaN(graphicElementLeft) && !double.IsNaN(graphicElementLeft) && posX > graphicElementLeft && posX < (graphicElementLeft + graphicElement.ActualWidth) && posY > graphicElementTop && posY < (graphicElementTop + graphicElement.ActualHeight);
            }));

            return res;
        }

    }
}
