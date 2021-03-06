using DeckAssist.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DeckAssist.View
{
    /// <summary>
    /// Converter class for XAML data binding, Converts a card to a string representation of its name
    /// </summary>
    [ValueConversion(typeof(Card), typeof(string))]
    internal class CardNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Card c = value as Card;
            return
                c.CardLayout == Layout.modal_dfc ?
                c.SelectedCardFaceDetail.Name : c.DisplayName.Replace(" // ", Environment.NewLine);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}