using DeckAssist.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DeckAssist.Model
{
    public class Card : ViewModelComponent
    {
        private CardFaceDetail backFace;
        private Layout cardLayout;
        private int cmc;
        private string displayName;
        private CardFaceDetail frontFace;
        private int qty;
        private CardFaceDetail selectedCardFaceDetail;

        public Card()
        {
            qty = 0;
            cmc = 0;
            displayName = "Unassigned";
            cardLayout = Layout.unassigned;
            frontFace = new CardFaceDetail();
            backFace = new CardFaceDetail();
            selectedCardFaceDetail = frontFace;
        }

        public Card(string response, int qty) : this()
        {
            JObject card = JObject.Parse(response);
            string strLayout = (string)card.SelectToken("layout");
            JToken jFaces = card.SelectToken("card_faces");

            Qty = qty;
            DisplayName = ((string)card.SelectToken("name"));

            strLayout = strLayout.Equals("class") ? "_class" : strLayout;

            if (!Enum.TryParse(strLayout, out Layout layout))
            {
                throw new ArgumentException(String.Format("Scryfall returned an unimplemented card layout: {0}", strLayout));
            }

            CardLayout = layout;

            if (new Layout[] { Layout.normal }.Contains(CardLayout))
            {
                FrontFace.Name = (string)card.SelectToken("name");

                FrontFace.ImageURI = (string)card.SelectToken("image_uris.normal");

                AddIdentityToFace(ref frontFace, card.SelectToken("colors"));

                FrontFace.TypeLine = (string)card.SelectToken("type_line");

                ConvertedManaCost = (int)card.SelectToken("cmc");
                FrontFace.ConvertedManaCost = ConvertedManaCost;
            }
            else if (CardLayout == Layout.flip)
            {
                FrontFace.Name = (string)jFaces[0].SelectToken("name");
                BackFace.Name = (string)jFaces[1].SelectToken("name");

                FrontFace.ImageURI = (string)card.SelectToken("image_uris.normal");

                AddIdentityToFace(ref frontFace, card.SelectToken("colors"));

                FrontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                BackFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                ConvertedManaCost = (int)card.SelectToken("cmc");
                FrontFace.ConvertedManaCost = ConvertedManaCost;
            }
            else if (CardLayout == Layout.split)
            {
                FrontFace.Name = (string)jFaces[0].SelectToken("name");
                BackFace.Name = (string)jFaces[1].SelectToken("name");

                FrontFace.ImageURI = (string)card.SelectToken("image_uris.normal");

                AddIdentityToFace(ref frontFace, card.SelectToken("colors"));
                AddIdentityToFace(ref backFace, card.SelectToken("colors"));

                FrontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                BackFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                ConvertedManaCost = (int)card.SelectToken("cmc");
                FrontFace.ConvertedManaCost = ConvertManaCostToCMC((string)jFaces[0].SelectToken("mana_cost"));
                BackFace.ConvertedManaCost = ConvertManaCostToCMC((string)jFaces[1].SelectToken("mana_cost"));
            }
            else if (new Layout[] { Layout.transform }.Contains(CardLayout))
            {
                FrontFace.Name = ((string)jFaces[0].SelectToken("name"));
                BackFace.Name = (string)jFaces[1].SelectToken("name");

                AddIdentityToFace(ref frontFace, jFaces[0].SelectToken("colors"));
                AddIdentityToFace(ref backFace, jFaces[1].SelectToken("colors"));

                FrontFace.ImageURI = (string)jFaces[0].SelectToken("image_uris.normal");
                BackFace.ImageURI = (string)jFaces[1].SelectToken("image_uris.normal");

                FrontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                BackFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                ConvertedManaCost = (int)card.SelectToken("cmc");
                FrontFace.ConvertedManaCost = ConvertedManaCost;
            }
            else if (new Layout[] { Layout.modal_dfc }.Contains(CardLayout))
            {
                FrontFace.Name = ((string)jFaces[0].SelectToken("name"));
                BackFace.Name = (string)jFaces[1].SelectToken("name");

                AddIdentityToFace(ref frontFace, jFaces[0].SelectToken("colors"));
                AddIdentityToFace(ref backFace, jFaces[1].SelectToken("colors"));

                FrontFace.ImageURI = (string)jFaces[0].SelectToken("image_uris.normal");
                BackFace.ImageURI = (string)jFaces[1].SelectToken("image_uris.normal");

                FrontFace.TypeLine = (string)jFaces[0].SelectToken("type_line");
                BackFace.TypeLine = (string)jFaces[1].SelectToken("type_line");

                ConvertedManaCost = (int)card.SelectToken("cmc");
                FrontFace.ConvertedManaCost = ConvertManaCostToCMC((string)jFaces[0].SelectToken("mana_cost"));
                BackFace.ConvertedManaCost = ConvertManaCostToCMC((string)jFaces[1].SelectToken("mana_cost"));
            }
        }

        public CardFaceDetail BackFace { get => backFace; set => SetProperty(ref backFace, value); }
        public Layout CardLayout { get => cardLayout; set => SetProperty(ref cardLayout, value); }
        public int ConvertedManaCost { get => cmc; set => SetProperty(ref cmc, value); }
        public string DisplayName { get => displayName; set => SetProperty(ref displayName, value); }
        public CardFaceDetail FrontFace { get => frontFace; set => SetProperty(ref frontFace, value); }
        public int Qty { get => qty; set => SetProperty(ref qty, value); }
        public CardFaceDetail SelectedCardFaceDetail { get => selectedCardFaceDetail; set => SetProperty(ref selectedCardFaceDetail, value); }

        public Card Copy()
        {
            CardFaceDetail copiedFront = frontFace.Copy();
            CardFaceDetail copiedBack = backFace.Copy();
            CardFaceDetail selectedCopy = SelectedCardFaceDetail == FrontFace ? copiedFront : copiedBack;
            return new Card
            {
                ConvertedManaCost = cmc,
                DisplayName = displayName,
                CardLayout = cardLayout,
                FrontFace = copiedFront,
                BackFace = copiedBack,
                SelectedCardFaceDetail = selectedCopy
            };
        }

        public override string ToString()
        {
            return String.Format("[Name: {0}]", DisplayName);
        }

        private void AddIdentityToFace(ref CardFaceDetail face, JToken identities)
        {
            if (identities.Count() == 0)
                face.ColorIdentities.Add(ColorIdentity.Colorless);
            else
            {
                foreach (var identity in identities)
                {
                    face.ColorIdentities.Add(ConvertColorIdentity((string)identity));
                }
            }

            if (face.ColorIdentities.Count > 1)
                face.ColorIdentities.Add(ColorIdentity.Multicolored);
        }

        private ColorIdentity ConvertColorIdentity(string color)
        {
            ColorIdentity c;

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

        private int ConvertManaCostToCMC(string mana_cost)
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
                if (int.TryParse(costBlock, out int tryNum)) //integer
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
    }
}