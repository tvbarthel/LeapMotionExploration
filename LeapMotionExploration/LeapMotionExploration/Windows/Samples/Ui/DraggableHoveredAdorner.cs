using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LeapMotionExploration.Windows.Samples.Ui
{
    class DraggableHoveredAdorner : Adorner
    {

        private Rectangle _child = null;
        private double _leftOffset=0;
        private double _topOffset = 0;

        // Be sure to call the base class constructor.
        public DraggableHoveredAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            VisualBrush _brush = new VisualBrush(adornedElement);

            _child = new Rectangle();
            _child.Width = adornedElement.RenderSize.Width;
            _child.Height = adornedElement.RenderSize.Height;
           

            DoubleAnimation animation = new DoubleAnimation(0.3, 1, new Duration(TimeSpan.FromSeconds(1)));
            animation.AutoReverse = true;
            animation.RepeatBehavior =  System.Windows.Media.Animation.RepeatBehavior.Forever;
            _brush.BeginAnimation(System.Windows.Media.Brush.OpacityProperty, animation);

            _child.Fill = _brush;
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout subsystem as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Black);
            renderBrush.Opacity = 0.8;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Black), 1.5);

            // draw overlay
            double desiredWidth = this.AdornedElement.DesiredSize.Width;
            double desiredHeight = this.AdornedElement.DesiredSize.Height;

            double leftMargin = -5;
            double rightMargin = desiredWidth + 5 - 3;
            double topMargin = - 5;
            double bottomMargin = desiredHeight + 5 - 3;

            //top left corner
            Rect topLeft1 = new Rect(leftMargin, topMargin, 3, 10);
            Rect topLeft2 = new Rect(leftMargin, topMargin, 10, 3);
            drawingContext.DrawRectangle(renderBrush, renderPen, topLeft1);
            drawingContext.DrawRectangle(renderBrush, renderPen, topLeft2);

            //top right corner
            Rect topRight1 = new Rect(rightMargin, topMargin, 3, 10);
            Rect topRight2 = new Rect(rightMargin - 7, topMargin, 10, 3);
            drawingContext.DrawRectangle(renderBrush, renderPen, topRight1);
            drawingContext.DrawRectangle(renderBrush, renderPen, topRight2);

            //bottom left corner
            Rect bottomLeft1 = new Rect(leftMargin, bottomMargin -7, 3, 10);
            Rect bottomLeft2 = new Rect(leftMargin, bottomMargin , 10, 3);
            drawingContext.DrawRectangle(renderBrush, renderPen, bottomLeft1);
            drawingContext.DrawRectangle(renderBrush, renderPen, bottomLeft2);

            //bottom right corner
            Rect bottomRight1 = new Rect(rightMargin, bottomMargin - 7, 3, 10);
            Rect bottomRight2 = new Rect(rightMargin - 7, bottomMargin, 10, 3);
            drawingContext.DrawRectangle(renderBrush, renderPen, bottomRight1);
            drawingContext.DrawRectangle(renderBrush, renderPen, bottomRight2);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        public double LeftOffset
        {
            get
            {
                return _leftOffset;
            }
            set
            {
                _leftOffset = value;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get
            {
                return _topOffset;
            }
            set
            {
                _topOffset = value;
                UpdatePosition();

            }
        }

        private void UpdatePosition()
        {
            AdornerLayer adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null)
            {
                adornerLayer.Update(AdornedElement);
            }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset,_topOffset));
            return result;
        }

    }
}
