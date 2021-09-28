using DeckAssist.Extensions;
using DeckAssist.Http;
using DeckAssist.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeckAssist.ViewModel
{
    internal class MainViewModel : ViewModelComponent
    {
        private readonly static string scryfallExactURI = "https://api.scryfall.com/cards/named?exact=";

        /// <summary>
        /// the mana curve is updated when these properties are changed
        /// </summary>
        private readonly static string[] watchedCardProperties = new string[]
        {
            nameof(Card.Qty),
            nameof(Card.SelectedCardFaceDetail)
        };

        private Visibility backBorder;
        private Deck deck;
        private string deckText;
        private string errorMessages;
        private Visibility frontBorder;
        private bool isImporting;
        private ManaCurveCollection manaCurve;
        private Card selectedCard;
        private string status;
        /// <summary>
        /// Initialize the view model
        /// </summary>
        public MainViewModel()
        {
            errorMessages = String.Empty;
            deckText = "Paste Deck Here....";
            status = "Idle";
            isImporting = false;
            ImportDeck = new RelayCommand(async o => await OnImportDeck(), o => !isImporting);
            AddToDeck = new RelayCommand(async o => await OnAddToDeck(), o => !isImporting);
            frontBorder = Visibility.Hidden;
            backBorder = Visibility.Hidden;
            deck = new Deck();
            manaCurve = deck.ManaCurve;
            selectedCard = new Card();

            RemoveSelf = new RelayCommand(o => OnRemoveSelf((Card)o));
            IncreaseQty = new RelayCommand(o => OnIncreaseQty((Card)o));
            DecreaseQty = new RelayCommand(o => OnDecreaseQty((Card)o), o => CanDecrease((Card)o));
            Flip = new RelayCommand(o => OnFlip((Card)o), o => CanFlip((Card)o));

            deck.Cards.CollectionChanged += OnCardsChanged;
            PropertyChanged += OnSelectedCardChanged;
        }
        /// <summary>
        /// The Command for adding a set of cards to an existing deck
        /// </summary>
        public ICommand AddToDeck { get; private set; }
        /// <summary>
        /// The visibility status of the back face modal border
        /// </summary>
        public Visibility BackBorder { get => backBorder; set => SetProperty(ref backBorder, value); }
        /// <summary>
        /// A reference to the active deck.
        /// </summary>
        public Deck Deck { get => deck; set => SetProperty(ref deck, value); }
        /// <summary>
        /// The deck text to attempt to import into the deck
        /// </summary>
        public string DeckText { get => deckText; set => SetProperty(ref deckText, value); }
        /// <summary>
        /// The Command for decreasing the Qty of a card
        /// </summary>
        public ICommand DecreaseQty { get; private set; }
        /// <summary>
        /// A collection of the error messages raised by the view model
        /// </summary>
        public string ErrorMessages { get => errorMessages; set => SetProperty(ref errorMessages, value); }
        /// <summary>
        /// The command to flip a modal card
        /// </summary>
        public ICommand Flip { get; private set; }
        /// <summary>
        /// The visibility status of the front face modal border
        /// </summary>
        public Visibility FrontBorder { get => frontBorder; set => SetProperty(ref frontBorder, value); }
        /// <summary>
        /// The Command to import a new deck
        /// </summary>
        public ICommand ImportDeck { get; private set; }
        /// <summary>
        /// The Command to increase the Qty of a card
        /// </summary>
        public ICommand IncreaseQty { get; private set; }
        /// <summary>
        /// A reference to the Deck's ManaCurveCollection
        /// </summary>
        public ManaCurveCollection ManaCurve { get => manaCurve; set => SetProperty(ref manaCurve, value); }
        /// <summary>
        /// The Command for a card to delete itself
        /// </summary>
        public ICommand RemoveSelf { get; private set; }
        /// <summary>
        /// A reference to the currently selected card
        /// </summary>
        public Card SelectedCard { get => selectedCard; set => SetProperty(ref selectedCard, value); }
        /// <summary>
        /// A reference to the Status text
        /// </summary>
        public string Status { get => status; set => SetProperty(ref status, value); }
        private void AddError(string s)
        {
            if (!ErrorMessages.Equals(string.Empty))
                ErrorMessages += Environment.NewLine;

            ErrorMessages += s;
        }

        private bool CanDecrease(Card card)
        {
            if (card != null)
                return card.Qty > 1;
            else
                return false;
        }

        private bool CanFlip(Card card)
        {
            if (card == null)
                return false;

            return card.CardLayout == Layout.modal_dfc;
        }

        private async Task OnAddToDeck()
        {
            string[] listings;

            string qtyStr,
                   name,
                   listing;

            int index;

            Card match,
                 newCard,
                 existingBackCard;

            //clear error messages
            ErrorMessages = String.Empty;

            //return if nothing to import
            if (DeckText.Equals(String.Empty))
            {
                Status = "ERROR - Nothing to import";
                AddError(Status);
                isImporting = false;
                return;
            }

            //standardize newline
            DeckText = DeckText.Replace(Environment.NewLine, "\n").Trim(); //exceptions never throw :)

            //get listings
            listings = DeckText.Split('\n');

            //flag start importing
            isImporting = true;

            foreach (string listingUntrimmed in listings)
            {
                //trim the listing
                listing = listingUntrimmed.Trim();

                //if the string is empty, add no error, but move to next iteration
                if (listing.Equals(String.Empty))
                    continue;

                //get the first part of a listing
                qtyStr = listing.Split(' ')[0];

                // if the first part isnt an int, write to errors and move to next iteration
                if (!Int32.TryParse(qtyStr, out int qty) || qty < 1)
                {
                    Status = String.Format("ERROR - A listing did not begin with an appropriate integer \"{0}\"", listing);
                    AddError(Status);
                    continue;
                }
                //do the same if there isnt anything following the integer
                if (qtyStr.Length == listing.Length)
                {
                    Status = String.Format("ERROR - No name is following the quantity of this listing: {0}", listing);
                    AddError(Status);
                    continue;
                }

                //retrieves as name the substring after the space that follows the quantity of a listing
                name = listing.Substring(qtyStr.Length + 1);
                //encode string to UTF8
                name = Encoding.UTF8.GetString(Encoding.Default.GetBytes(name));
                //update status
                Status = String.Format("Retrieving {0}...", name);


                //if card of same name already exists, update quantity of existing entry
                match = Deck.FindCardByName(name);
                if (match != null)
                    OnIncreaseQty(match, qty);
                else
                {
                    //query the server for cards matching the name
                    HttpResponseMessage response = await HttpClientManager.GetRequest(scryfallExactURI + name);
                    string responseString = await HttpClientManager.GetResponseContent(response);
                    //rate limit requests per the request of scryfall
                    System.Threading.Thread.Sleep(100);

                    //if the response failed
                    if (!response.IsSuccessStatusCode)
                    {
                        //display the response code and offending argument
                        Status = String.Format("ERROR - {0} {1} Card name passed: \"{2}\"",
                            (int)response.StatusCode, response.ReasonPhrase, listing);
                        AddError(Status);
                        continue;
                    }

                    //initialize new card with scryfall response
                    newCard = new Card(responseString, qty);

                    //if the user passed the back name of the card, and it's modal, select the back face of this card
                    if (newCard.BackFace.Name.EqualsIgnoreCase(name) && newCard.CardLayout == Layout.modal_dfc)
                        newCard.SelectedCardFaceDetail = newCard.BackFace;

                    //if new card is modal and new card is the front side
                    if (newCard.CardLayout == Layout.modal_dfc && newCard.SelectedCardFaceDetail == newCard.FrontFace)
                    {
                        //if back card isnt found, null
                        existingBackCard = Deck.FindCardByName(newCard.BackFace.Name);
                        //get index of card if not null, otherwise -1
                        index = Deck.GetIndexOf(existingBackCard);
                        //append if index -1, otherwise insert at index
                        Deck.AddCard(newCard, index);
                    }
                    //otherwise, add as normal
                    else
                    {
                        Deck.AddCard(newCard);
                    }
                }
            }

            // if cards were added, select the first card
            if (Deck.Cards.Count >= 1)
                SelectedCard = deck.Cards.First();

            //clear the user inout field
            DeckText = String.Empty;
            //update user
            Status = String.Format("Cards added {0}", ErrorMessages.Equals(String.Empty) ? "successfully" : "with errors");
            //unset importing flag
            isImporting = false;
        }

        private void OnCardPropertyChange(object sender, PropertyChangedEventArgs args)
        {
            if (watchedCardProperties.Contains(args.PropertyName))
            {
                ManaCurve.AnalyzeCurve();
                RaisePropertyChanged("ManaCurve");
            }
        }

        private void OnCardsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                foreach (INotifyPropertyChanged added in args.NewItems)
                    added.PropertyChanged += OnCardPropertyChange;
            }

            if (args.OldItems != null)
            {
                foreach (INotifyPropertyChanged removed in args.OldItems)
                    removed.PropertyChanged -= OnCardPropertyChange;
            }

            ManaCurve.AnalyzeCurve();
            RaisePropertyChanged("ManaCurve");
        }

        private void OnDecreaseQty(Card card, int amount = 1)
        {
            card.Qty -= amount;
        }

        
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void AddNewFlippedCard(Card nextCard, int index)
        {
            nextCard.SelectedCardFaceDetail =
                    nextCard.SelectedCardFaceDetail == nextCard.FrontFace ? nextCard.BackFace : nextCard.FrontFace;
            nextCard.Qty = 1;
            Deck.AddCard(nextCard, index);
        }

        /// <exception cref="ArgumentOutOfRangeException">This shouldn't throw as the index is checked for</exception>
        private void FlipCard(Card card)
        {
            bool isFrontFace,
                 isMoreThanLastPosition,
                 isLessThanZero,
                 isAdjacentCardOtherSide;

            int adjacentIndex,
                cardIndex,
                insertIndex;

            Card nextCard;

            cardIndex = Deck.GetIndexOf(card);
            isFrontFace = card.SelectedCardFaceDetail == card.FrontFace;
            adjacentIndex = isFrontFace ? cardIndex + 1 : cardIndex - 1;
            insertIndex = isFrontFace ? adjacentIndex : cardIndex;
            isMoreThanLastPosition = adjacentIndex > Deck.Cards.Count - 1;
            isLessThanZero = adjacentIndex < 0;

            // if index of next card is in bounds
            if (!isMoreThanLastPosition && !isLessThanZero)
            {
                //get adjacent card
                nextCard = Deck.Cards[adjacentIndex];
                //is adjacent card the corresponding face
                isAdjacentCardOtherSide = nextCard.DisplayName.Equals(card.DisplayName);
                //if so, increase Qty
                if (isAdjacentCardOtherSide) 
                {
                    nextCard.Qty++;
                }
                //otherwise, add new card to adjecent position
                else
                {
                    nextCard = card.Copy();
                    AddNewFlippedCard(nextCard, insertIndex);
                }
            }
            //if out of bounds, add new card to start or end of list
            else
            {
                //if index is more than count, -1 to default to append, else start of list
                insertIndex = isMoreThanLastPosition ? -1 : 0;

                nextCard = card.Copy();
                AddNewFlippedCard(nextCard, insertIndex);
            }

            card.Qty--;

            //if no more cards of this entry remain, remove the card entry from the deck and select the new card
            if (card.Qty == 0)
            {
                SelectedCard = nextCard;
                Deck.RemoveCard(card);
            }
        }

        private void OnFlip(Card card)
        {
            if (!(card.CardLayout == Layout.modal_dfc))
                return;

            FlipCard(card);

            Console.WriteLine("congratu-flippin-lations!!!");
        }
        private async Task OnImportDeck()
        {
            SelectedCard = new Card();
            Deck.ClearDeck();
            await OnAddToDeck();
        }

        private void OnIncreaseQty(Card card, int amount = 1)
        {
            card.Qty += amount;
        }

        private void OnRemoveSelf(Card card)
        {
            Deck.RemoveCard(card);
            
            if (SelectedCard == null)
            {
                BackBorder = Visibility.Hidden;
                FrontBorder = Visibility.Hidden;
            }
        }
        private void OnSelectedCardChanged(object sender, PropertyChangedEventArgs args)
        {
            if (SelectedCard == null)
                return;

            if (args.PropertyName.Equals("SelectedCard"))
            {
                if (Layout.modal_dfc == SelectedCard.CardLayout)
                {
                    Console.WriteLine("Modal selected");
                    if (SelectedCard.SelectedCardFaceDetail == SelectedCard.BackFace)
                    {
                        BackBorder = Visibility.Visible;
                        FrontBorder = Visibility.Hidden;
                    }
                    else
                    {
                        BackBorder = Visibility.Hidden;
                        FrontBorder = Visibility.Visible;
                    }
                }
                else
                {
                    BackBorder = Visibility.Hidden;
                    FrontBorder = Visibility.Hidden;
                }
            }
        }
    }
}