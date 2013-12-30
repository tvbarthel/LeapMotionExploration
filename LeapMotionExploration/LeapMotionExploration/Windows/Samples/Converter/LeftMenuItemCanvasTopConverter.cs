using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace LeapMotionExploration.Windows.Samples.Converter
{
    class LeftMenuItemCanvasTopConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Count() < 3)
            {
                throw new ArgumentException("LeftMenuItemPositionConverter expects 3 double values to be passed in this order -> canvasHeight, itemHeight, margin", "values");
            }

            double canvasHeight = (double) values[0];
            double itemHeight = (double) values[1];
            double margin = (double) values[2];

            return (object)((canvasHeight - itemHeight) / 2 + margin);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
