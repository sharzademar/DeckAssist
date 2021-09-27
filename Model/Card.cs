using DeckAssist.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DeckAssist.Model
{
    /// <summary>
    /// Represents a card, its faces, and any other data associated with the card
    /// </summary>
    public class Card : ViewModelComponent
    {
        private static readonly string[] singleManaBlocks = new string[]
        {
            "W", "U", "B", "R", "G", "C",
            "W/U", "W/B", "B/R", "B/G", "U/B", "U/R", "R/G", "R/W", "G/W", "G/U",
            "W/P", "U/P", "B/P", "R/P", "G/P",
            "S"
        };

        private static readonly string[] twoManaBlocks = new string[] { "2/W", "2/U", "2/B", "2/R", "2/G" };

        private readonly JObject cardJson;

        private CardFaceDetail backFace;
        private Layout cardLayout;
        private int cmc;
        private string displayName;
        private CardFaceDetail frontFace;
        private int qty;
        private CardFaceDetail selectedCardFaceDetail;

        /// <summary>
        /// Return a new <c>Card</c> with default values
        /// </summary>
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

        /// <summary>
        /// Return a card populated by a scryfall response to <c>cards/named?exact=</c>
        /// </summary>
        /// <param name="response">A response from the scryfall api at cards/named?exact=</param>
        /// <param name="qty">The number of cards this card entry object represents</param>
        public Card(string response, int qty) : this()
        {
            cardJson = JObject.Parse(response);
            Qty = qty;

            string strLayout = (string)cardJson.SelectToken("layout");
            strLayout = strLayout.Equals("class") ? "_class" : strLayout;
            DisplayName = ((string)cardJson.SelectToken("name"));

            if (!EnumHelper.TryParse(strLayout, out Layout layout))
            {
                throw new ArgumentException(String.Format("Scryfall returned an unimplemented card layout: {0}", strLayout), "response");
            }

            CardLayout = layout;

            LayoutProperties properties = EnumHelper.GetLayoutProperties(CardLayout);

            SetTarget(properties.Naming, JSONTokens.name, (x, y) =>
            {
                FrontFace.Name = (string)x[0];
                if (y == PropertyMode.Double)
                    BackFace.Name = (string)x[1];
            });
            SetTarget(properties.Type, JSONTokens.type_line, (x, y) =>
            {
                FrontFace.TypeLine = (string)x[0];
                if (y == PropertyMode.Double)
                    BackFace.TypeLine = (string)x[1];
            });
            SetTarget(properties.ArtFaces, JSONTokens.image_uris_normal, (x, y) =>
            {
                FrontFace.ImageURI = (string)x[0];
                if (y == PropertyMode.Double)
                    BackFace.ImageURI = (string)x[1];
            });
            SetTarget(properties.ColorIdentities, JSONTokens.colors, (x, y) =>
            {
                AddIdentityToFace(FrontFace, x[0]);
                if (y == PropertyMode.Double)
                    AddIdentityToFace(BackFace, x[1]);
            });
            SetTarget(properties.ConvertedManaCost, JSONTokens.cmc,
                (x, y) =>
                {
                    int front = y == PropertyMode.Single ? (int)x[0] : ConvertManaCostToCMC((string)x[0]);
                    FrontFace.ConvertedManaCost = front;
                    ConvertedManaCost = FrontFace.ConvertedManaCost;
                    if (y == PropertyMode.Double)
                        BackFace.ConvertedManaCost = ConvertManaCostToCMC((string)x[1]);
                });
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

        private void AddIdentityToFace(CardFaceDetail face, JToken identities)
        {
            if (identities.Count() == 0)
                face.ColorIdentities |= ColorIdentity.Colorless;
            else
            {
                foreach (var identity in identities)
                {
                    face.ColorIdentities |= ConvertColorIdentity((string)identity);
                }
            }

            /*if (face.ColorIdentities.Count > 1)
                face.ColorIdentities.Add(ColorIdentity.Multicolored);*/
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
                //this logic will ignore X symbol, which is intended
            }

            return cmc;
        }

        //SetTarget(properties.naming, JSONTokens.name, x => { FrontFace.Name = x[0]; BackFace.Name = x[1]; }
        private void SetTarget(PropertyMode mode, JSONTokens token, Action<JToken[], PropertyMode> action)
        {
            JToken[] tokens = new JToken[2];

            string safeToken = token.ToString();
            if (safeToken == JSONTokens.image_uris_normal.ToString())
                safeToken = "image_uris.normal";

            string subToken = safeToken;
            if (subToken == JSONTokens.cmc.ToString() && mode == PropertyMode.Double)
                subToken = "mana_cost";

            if (mode == PropertyMode.Single)
            {
                tokens[0] = cardJson.SelectToken(safeToken);
            }
            else if (mode == PropertyMode.Double)
            {
                JToken jFaces = cardJson.SelectToken("card_faces");

                tokens[0] = jFaces[0].SelectToken(subToken);
                tokens[1] = jFaces[1].SelectToken(subToken);
            }

            action(tokens, mode);
        }
    }
}