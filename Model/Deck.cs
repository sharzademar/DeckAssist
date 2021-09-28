using DeckAssist.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeckAssist.Model
{
    /// <summary>
    /// Represents a collection of Card objects
    /// </summary>
    public class Deck
    {
        /// <summary>
        /// Initialize properties
        /// </summary>
        public Deck()
        {
            Cards = new ObservableCollection<Card>();
            ManaCurve = new ManaCurveCollection(Cards);
            ClearDeck();
        }

        /// <summary>
        /// The collection of Cards this Deck represents
        /// </summary>
        public ObservableCollection<Card> Cards { get; set; }

        /// <summary>
        /// The mana curve of deck. Can only be read.
        /// </summary>
        public ManaCurveCollection ManaCurve { get; private set; }

        /// <summary>
        /// Add a card to the specified index, or append if no index is provided, or if passed -1.
        /// </summary>
        /// <param name="card">The Card to add to this Deck</param>
        /// <param name="index">The index to insert the Card. -1 signals an append. Defaults to -1.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AddCard(Card card, int index = -1)
        {
            if (index == -1)
                Cards.Add(card);
            else
                Cards.Insert(index, card);
        }

        /// <summary>
        /// Clears the Cards property of the Deck, along with the ManaCurve
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void ClearDeck()
        {
            Cards.Clear();
            ManaCurve.Keys.ToList().ForEach(x => ManaCurve.Remove(x));
        }

        //userstr is expecting either a full display name or ONE of the card face names
        /// <summary>
        /// Return a Card in this Deck that has a display name or face name equal to the passed name. Returns null if no card found.
        /// </summary>
        /// <param name="name">The card name to search for.</param>
        /// <returns>The matching card. Returns null if no card if found.</returns>
        public Card FindCardByName(string name)
        {
            IEnumerable<Card> matchDisplay,
                              matchSelectedFace;
            Card c = null;

            matchDisplay = Cards
                .Where(x => x.DisplayName.EqualsIgnoreCase(name));

            matchSelectedFace = Cards
                .Where(x => x.SelectedCardFaceDetail.Name.EqualsIgnoreCase(name));

            if (matchDisplay.Count() == 1)
                c = matchDisplay.First();
            else if (matchSelectedFace.Count() == 1)
                c = matchSelectedFace.First();

            return c;
        }

        /// <summary>
        /// Get the index of the Card object within the Deck.Cards collection. Returns -1 if not found,
        /// </summary>
        /// <param name="card">The card to search for.</param>
        /// <returns>The index of the card within the list. -1 if not found.</returns>
        public int GetIndexOf(Card card)
        {
            return Cards.IndexOf(card);
        }

        /// <summary>
        /// Remove the specified card object from the Deck.Cards collection. Returns true if succeeded, false otherwise.
        /// </summary>
        /// <param name="card">The card to remove</param>
        /// <returns>Whether or not the remove operation succeeded.</returns>
        public bool RemoveCard(Card card)
        {
            return Cards.Remove(card);
        }
    }
}