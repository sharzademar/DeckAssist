using DeckAssist.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DeckAssist.View
{
    /// <summary>
    /// Converter class for XAML data binding. Converts a ManaCurveColllection to a basic string representation.
    /// </summary>
    [ValueConversion(typeof(ManaCurveCollection), typeof(String))]
    public class ManaCurveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ManaCurveCollection mcc = (ManaCurveCollection)value;

            string mcs = String.Join(Environment.NewLine, mcc.OrderBy(o => o.Value.ConvertedManaCost).Select(x => x.Value));
            mcs += Environment.NewLine + String.Format("Total cards in manaCurve: {0}", mcc.Sum(o => o.Value.Qty));
            mcs += Environment.NewLine + String.Format("Total lands: {0}", mcc.NumLands);
            mcs += Environment.NewLine + String.Format("Total cards: {0}", mcc.Sum);

            return mcs.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}