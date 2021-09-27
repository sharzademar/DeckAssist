using DeckAssist.Extensions;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeckAssist.Model
{
    public class Deck
    {
        public Deck()
        {
            Cards = new ObservableCollection<Card>();
            ManaCurve = new ManaCurveCollection(Cards);
            ClearDeck();
        }

        public ObservableCollection<Card> Cards { get; set; }
        public ManaCurveCollection ManaCurve { get; set; }
        public void AddCard(Card card, int index = -1)
        {
            if (index == -1)
                Cards.Add(card);
            else
                Cards.Insert(index, card);
        }

        public void ClearDeck()
        {
            Cards.Clear();
            ManaCurve.Keys.ToList().ForEach(x => ManaCurve.Remove(x));
        }

        //userstr is expecting either a full display name or ONE of the card face names
        public Card FindCardByUserString(string userStr)
        {
            Card c = null;

            var matchDisplay = Cards
                .Where(x => x.DisplayName.EqualsIgnoreCase(userStr));

            var matchSelectedFace = Cards
                .Where(x => x.SelectedCardFaceDetail.Name.EqualsIgnoreCase(userStr));

            if (matchDisplay.Count() == 1)
                c = matchDisplay.First();
            else if (matchSelectedFace.Count() == 1)
                c = matchSelectedFace.First();

            return c;
        }
        public int GetIndexOf(Card card)
        {
            return Cards.IndexOf(card);
        }

        public void RemoveCard(Card card)
        {
            Cards.Remove(card);
        }
    }
}