using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeckAssist
{
    public enum ColorIdentity
    {
        White,
        Blue,
        Black,
        Red,
        Green,
        Colorless,
        Multicolored
    }

    public enum Layout
    {
        normal, //implemented
        split, //implemented
        flip, //implemented
        transform, //implemented
        modal_dfc,
        meld,
        leveler,
        _class,
        saga,
        adventure,
        planar,
        scheme,
        vanguard,
        token,
        double_faced_token,
        emblem,
        augment,
        host,
        art_series,
        double_sided,
        unassigned
    }

    public class Card : ViewModelBase
    {
        private Deck parentDeck;
        private int qty;
        private int cmc;
        private string displayName;
        private Layout cardLayout;
        private CardFaceDetail frontFace;
        private CardFaceDetail backFace;
        private List<CardFaceDetail> faces;
        private CardFaceDetail selectedCardFaceDetail;

        public ICommand Flip { get; set; }
        public ICommand RemoveSelf { get; set; }
        public ICommand IncreaseQty { get; set; }
        public ICommand DecreaseQty { get; set; }

        public Deck ParentDeck { get => parentDeck; set => SetProperty(ref parentDeck, value); }
        public int Qty { get => qty; set => SetProperty(ref qty, value); }
        /*
         * CMC when not in stack
         */
        public int ConvertedManaCost { get => cmc; set => SetProperty(ref cmc, value); }
        public string DisplayName { get => displayName; set => SetProperty(ref displayName, value); }
        public Layout CardLayout { get => cardLayout; set => SetProperty(ref cardLayout, value); }
        public CardFaceDetail FrontFace { get => frontFace; set => SetProperty(ref frontFace, value); }
        public CardFaceDetail BackFace { get => backFace; set => SetProperty(ref backFace, value); }
        public CardFaceDetail SelectedCardFaceDetail { get => selectedCardFaceDetail; set => SetProperty(ref selectedCardFaceDetail, value); }

        public Card()
        {
            parentDeck = null;

            qty = 0;
            cmc = 0;
            displayName = "Unassigned";
            cardLayout = Layout.unassigned;
            frontFace = new CardFaceDetail();
            backFace = new CardFaceDetail();
            faces = new List<CardFaceDetail>();
            faces.Add(frontFace);
            faces.Add(backFace);
            selectedCardFaceDetail = frontFace;

            Flip = new RelayCommand(o => flip(), b => CardLayout == Layout.modal_dfc);
            RemoveSelf = new RelayCommand(o => removeSelf());
            IncreaseQty = new RelayCommand(o => increaseQty());
            DecreaseQty = new RelayCommand(o => decreaseQty(), p => qty > 1);
        }

        public override string ToString()
        {
            return String.Format("[Name: {0}]", String.Join(" // ", faces.Select(o=>o.Name)));
        }

        private ColorIdentity convertColorIdentity(string color)
        {
            ColorIdentity c = ColorIdentity.Colorless;

            switch (color)
            {
                case "W":
                    c = ColorIdentity.White;
                    break;
                case "U":
                    c = ColorIdentity.Blue;
                    break;
                case "B":
                    c = ColorIdentity.Black;
                    break;
                case "R":
                    c = ColorIdentity.Red;
                    break;
                case "G":
                    c = ColorIdentity.Green;
                    break;
                default:
                    throw new ArgumentException("Unexpected color identifier from Scryfall");
            }

            return c;
        }

        private void addIdentityToFace(ref CardFaceDetail face, JToken identities)
        {
            if (identities.Count() == 0)
                face.ColorIdentities.Add(ColorIdentity.Colorless);
            else
            {
                foreach (var identity in identities)
                {
                    face.ColorIdentities.Add(convertColorIdentity((string)identity));
                }
            }

            if (face.ColorIdentities.Count > 1)
                face.ColorIdentities.Add(ColorIdentity.Multicolored);
        }

        private int convertManaCostToCMC(string mana_cost)
        {
            if (mana_cost.Equals(String.Empty))
                return 0;

            int cmc = 0;
            string[] singleManaBlocks = new string[]
            {
                "W", "U", "B", "R", "G", "C",
                "W/U", "W/B", "B/R", "B/G", "U/B", "U/R", "R/G", "R/W", "G/W", "G/U",
                "W/P", "U/P", "B/P", "R/P", "G/P",
                "S"
            };
            string[] twoManaBlocks = new string[] { "2/W", "2/U", "2/B", "2/R", "2/G" };

            mana_cost = mana_cost.Replace("{", "").Replace("}", ",");
            mana_cost = mana_cost.Remove(mana_cost.Length - 1); //remove trailing comma

            foreach (var costBlock in mana_cost.Split(','))
            {
                int tryNum;

                if (int.TryParse(costBlock, out tryNum)) //integer
                {
                    cmc += tryNum;
                }
                else if (singleManaBlocks.Contains(costBlock)) //one mana
                {
                    cmc++;
                }
                else if (twoManaBlocks.Contains(costBlock)) //two mana
                {
                    cmc += 2;
                }
                //ignore X
            }

            return cmc;
        }

        public bool HasNonLandType()
        {
            bool hasEm = false;

            foreach (var type in faces.Select(o => o.TypeLine))
            {
                if (!type.Contains("Land"))
                {
                    hasEm = true;
                    break;
                }
            }

            return hasEm;
        }

        private bool flip()
        {
            if (!(CardLayout == Layout.modal_dfc))
                return false;

            int index = ParentDeck.GetIndexOf(this);
            ParentDeck.RemoveCard(this);

            if (SelectedCardFaceDetail == FrontFace)
                SelectedCardFaceDetail = BackFace;
            else
                SelectedCardFaceDetail = FrontFace;

            ParentDeck.AddCard(this, index);
            ParentDeck.SelectedCard = this;

            Console.WriteLine("congratu-flippin-lations!!!");

            return true;
        }

        private void removeSelf()
        {
            //Qty = 0;
            ParentDeck.RemoveCard(this);
        }

        public void increaseQty(int amount = 1)
        {
            int index = ParentDeck.GetIndexOf(this);
            ParentDeck.RemoveCard(this);
            Qty += amount;
            ParentDeck.AddCard(this, index);
        }

        public void decreaseQty(int amount = 1)
        {
            int index = ParentDeck.GetIndexOf(this);
            ParentDeck.RemoveCard(this);
            Qty -= amount;
            ParentDeck.AddCard(this, index);
        }

        public Card(string response, int qty) : this()
        {
            JObject card = JObject.Parse(response);
            string strLayout = (string)card.SelectToken("layout");
            JToken jFaces = card.SelectToken("card_faces");

            this.qty = qty;
            DisplayName = ((string)card.SelectToken("name")).Replace(" // ", Environment.NewLine);

            strLayout = strLayout.Equals("class") ? "_class" : strLayout;
            Layout layout;

            if (!Enum.TryParse(strLayout, out layout))
            {
                throw new ArgumentException(String.Format("Scryfall returned an unimplemented card layout: {0}", strLayout));
            }

            CardLayout = layout;

            if (new Layout[] { Layout.normal }.Contains(layout))
            {
                frontFace.Name = (string)card.SelectToken("name");

                frontFace.ImageURI = (string)card.SelectToken("image_uris.normal");

                addIdentityToFace(ref frontFace, card.SelectToken("colors"));

                frontFace.TypeLine = (string)card.SelectToken("type_line");

                cmc = (int)card.SelectToken("cmc");
                frontFace.ConvertedManaCost = cmc;
            }
            else if (layout == Layout.flip)
            {
                frontFace.Name = (string)jFaces[0].SelectToken("name");
                backFace.Name = (string)jFaces[1].SelectToken("name");

                frontFace.ImageURI = (string)card.SelectToken("image_uris.normal");

                addIdentityToFace(ref frontFace, card.SelectToken("colors"));

                frontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                backFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                cmc = (int)card.SelectToken("cmc");
                frontFace.ConvertedManaCost = cmc;
            }
            else if (layout == Layout.split)
            {
                frontFace.Name = (string)jFaces[0].SelectToken("name");
                backFace.Name = (string)jFaces[1].SelectToken("name");

                frontFace.ImageURI = (string)card.SelectToken("image_uris.normal");

                addIdentityToFace(ref frontFace, card.SelectToken("colors"));
                addIdentityToFace(ref backFace, card.SelectToken("colors"));

                frontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                backFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                cmc = (int)card.SelectToken("cmc");
                frontFace.ConvertedManaCost = convertManaCostToCMC((string)jFaces[0].SelectToken("mana_cost"));
                backFace.ConvertedManaCost = convertManaCostToCMC((string)jFaces[1].SelectToken("mana_cost"));
            }
            else if(new Layout[] { Layout.transform }.Contains(layout)) 
            {
                frontFace.Name = ((string)jFaces[0].SelectToken("name"));
                backFace.Name = (string)jFaces[1].SelectToken("name");

                addIdentityToFace(ref frontFace, jFaces[0].SelectToken("colors"));
                addIdentityToFace(ref backFace, jFaces[1].SelectToken("colors"));

                frontFace.ImageURI = (string)jFaces[0].SelectToken("image_uris.normal");
                backFace.ImageURI = (string)jFaces[1].SelectToken("image_uris.normal");

                frontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                backFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                cmc = (int)card.SelectToken("cmc");
                frontFace.ConvertedManaCost = cmc;
            }
            else if (new Layout[] { Layout.modal_dfc }.Contains(layout))
            {
                frontFace.Name = ((string)jFaces[0].SelectToken("name"));
                backFace.Name = (string)jFaces[1].SelectToken("name");

                addIdentityToFace(ref frontFace, jFaces[0].SelectToken("colors"));
                addIdentityToFace(ref backFace, jFaces[1].SelectToken("colors"));

                frontFace.ImageURI = (string)jFaces[0].SelectToken("image_uris.normal");
                backFace.ImageURI = (string)jFaces[1].SelectToken("image_uris.normal");

                frontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                backFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                cmc = (int)card.SelectToken("cmc");
                frontFace.ConvertedManaCost = convertManaCostToCMC((string)jFaces[0].SelectToken("mana_cost"));
                backFace.ConvertedManaCost = convertManaCostToCMC((string)jFaces[1].SelectToken("mana_cost"));
            }
        } 
    }
}
