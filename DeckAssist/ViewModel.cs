using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Web;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using System.Windows;

namespace DeckAssist
{
    class ViewModel : ViewModelBase
    {     
        private string deckText;
        private string status;
        private string manaCurveString;
        private string errorMessages;
        private bool isImporting;
        private Visibility frontBorder;
        private Visibility backBorder;

        private Deck deck;
        
        public ICommand ImportDeck { get; private set; }
        public ICommand AddToDeck { get; private set; }

        public string DeckText { get => deckText; set => SetProperty(ref deckText, value); }
        public string Status { get => status; set => SetProperty(ref status, value); }
        public string ManaCurveString { get => manaCurveString; set => SetProperty(ref manaCurveString, value); }
        public string ErrorMessages { get => errorMessages; set => SetProperty(ref errorMessages, value); }
        public Deck Deck { get => deck; set => SetProperty(ref deck, value); }
        public Visibility FrontBorder { get => frontBorder; set => SetProperty(ref frontBorder, value); }
        public Visibility BackBorder { get => backBorder; set => SetProperty(ref frontBorder, value); }

        public ViewModel()
        {
            errorMessages = String.Empty;
            manaCurveString = String.Empty;
            deckText = "Paste Deck Here....";
            status = "Idle";
            isImporting = false;
            ImportDeck = new RelayCommand(async o => await importDeck(), o=>!isImporting);
            AddToDeck = new RelayCommand(async o => await addToDeck(), o => !isImporting);
            frontBorder = Visibility.Hidden;
            backBorder = Visibility.Hidden;
            deck = new Deck();
        }

        private void addError(string s)
        {
            if (!ErrorMessages.Equals(string.Empty))
                ErrorMessages += Environment.NewLine;

            ErrorMessages += s;
        }

        private async Task addToDeck()
        {
            isImporting = true;
            ErrorMessages = String.Empty;

            if (DeckText.Equals(String.Empty))
            {
                Status = "ERROR - Nothing to import";
                addError(Status);
                isImporting = false;
                return;
            }

            DeckText = DeckText.Replace(Environment.NewLine, "\n").Trim();

            var x = DeckText.Split('\n');
            foreach (string listing in x)
            {
                Status = "Retrieving...";
                string qtyStr = listing.Split(' ')[0];
                int qty;
                if (!Int32.TryParse(qtyStr, out qty) || qty < 1)
                {
                    Status = String.Format("ERROR - A listing did not begin with an appropriate integer \"{0}\"", listing);
                    addError(Status);
                    continue;
                }

                string name = listing.Substring(qtyStr.Length + 1);
                name = Encoding.UTF8.GetString(Encoding.Default.GetBytes(name));
                Status = String.Format("Retrieving {0}...", name);

                var matchingRecord = Deck.Cards.Where(o => o.FrontFace.Name.Equals(name));

                if (matchingRecord.Count() == 1)
                    matchingRecord.First().increaseQty();
                else
                {
                    HttpResponseMessage response = await HttpClientManager.GetRequest("https://api.scryfall.com/cards/named?exact=" + name);
                    string responseString = await HttpClientManager.GetResponseContent(response);
                    System.Threading.Thread.Sleep(100);

                    if (response.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        Status = String.Format("ERROR - {0} was not found.", name);
                        addError(Status);
                        continue;
                    }
                    Card newCard = new Card(responseString, qty);
                    Deck.AddCard(newCard);
                }
            }

            if (Deck.Cards.Count >= 1)
                Deck.SelectedCard = deck.Cards.First();

            DeckText = String.Empty;
            Status = String.Format("Cards added {0}", ErrorMessages.Equals(String.Empty) ? "successfully" : "with errors");
            isImporting = false;
        }

        private async Task importDeck()
        {
            Deck.ClearDeck();
            await addToDeck();
        }
    }
}
