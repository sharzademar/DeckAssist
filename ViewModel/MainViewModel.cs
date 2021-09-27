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

        public ICommand AddToDeck { get; private set; }
        public Visibility BackBorder { get => backBorder; set => SetProperty(ref backBorder, value); }
        public Deck Deck { get => deck; set => SetProperty(ref deck, value); }
        public string DeckText { get => deckText; set => SetProperty(ref deckText, value); }
        public ICommand DecreaseQty { get; private set; }
        public string ErrorMessages { get => errorMessages; set => SetProperty(ref errorMessages, value); }
        public ICommand Flip { get; private set; }
        public Visibility FrontBorder { get => frontBorder; set => SetProperty(ref frontBorder, value); }
        public ICommand ImportDeck { get; private set; }
        public ICommand IncreaseQty { get; private set; }
        public ManaCurveCollection ManaCurve { get => manaCurve; set => SetProperty(ref manaCurve, value); }
        public ICommand RemoveSelf { get; private set; }
        public Card SelectedCard { get => selectedCard; set => SetProperty(ref selectedCard, value); }
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
            isImporting = true;
            ErrorMessages = String.Empty;

            if (DeckText.Equals(String.Empty))
            {
                Status = "ERROR - Nothing to import";
                AddError(Status);
                isImporting = false;
                return;
            }

            DeckText = DeckText.Replace(Environment.NewLine, "\n").Trim();

            var x = DeckText.Split('\n');
            foreach (string listing in x)
            {
                Status = "Retrieving...";
                string qtyStr = listing.Split(' ')[0];
                if (!Int32.TryParse(qtyStr, out int qty) || qty < 1)
                {
                    Status = String.Format("ERROR - A listing did not begin with an appropriate integer \"{0}\"", listing);
                    AddError(Status);
                    continue;
                }

                string name = listing.Substring(qtyStr.Length + 1);
                name = Encoding.UTF8.GetString(Encoding.Default.GetBytes(name));
                Status = String.Format("Retrieving {0}...", name);

                Card match = Deck.FindCardByUserString(name);

                if (match != null)
                    OnIncreaseQty(match, qty); //this probably doesnt work with double sided cards
                else
                {
                    HttpResponseMessage response = await HttpClientManager.GetRequest(scryfallExactURI + name);
                    string responseString = await HttpClientManager.GetResponseContent(response);
                    System.Threading.Thread.Sleep(100);

                    if (response.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        Status = String.Format("ERROR - {0} was not found.", name);
                        AddError(Status);
                        continue;
                    }
                    Card newCard = new Card(responseString, qty);

                    if (newCard.BackFace.Name.EqualsIgnoreCase(name))
                        newCard.SelectedCardFaceDetail = newCard.BackFace;

                    if (newCard.CardLayout == Layout.modal_dfc && newCard.SelectedCardFaceDetail == newCard.FrontFace)
                    {
                        Card existingBackCard = Deck.FindCardByUserString(newCard.BackFace.Name);
                        int index = Deck.GetIndexOf(existingBackCard);
                        Deck.AddCard(newCard, index);
                    }
                    else
                    {
                        Deck.AddCard(newCard);
                    }
                }
            }

            if (Deck.Cards.Count >= 1)
                SelectedCard = deck.Cards.First();

            DeckText = String.Empty;
            Status = String.Format("Cards added {0}", ErrorMessages.Equals(String.Empty) ? "successfully" : "with errors");
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

        private void AddNewFlippedCard(Card nextCard, int index)
        {
            nextCard.SelectedCardFaceDetail =
                    nextCard.SelectedCardFaceDetail == nextCard.FrontFace ? nextCard.BackFace : nextCard.FrontFace;
            nextCard.Qty = 1;
            Deck.AddCard(nextCard, index);
        }

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
            try
            {
                SelectedCard = new Card();
                Deck.ClearDeck();
                await OnAddToDeck();
            }
            catch (KeyNotFoundException e)
            {
                Log.Fatal(e.StackTrace);
                throw;
            }
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