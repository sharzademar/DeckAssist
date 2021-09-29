using DeckAssist.Http;
using DeckAssist.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeckAssist.Model
{
    /// <summary>
    /// Represents the specific layout of the card
    /// </summary>
    public enum Layout
    {
        normal, //implemented
        split, //implemented
        flip, //implemented
        transform, //implemented
        modal_dfc, //implemented
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

    /// <summary>
    /// represents the card's role in a meld
    /// </summary>
    public enum MeldComponentType
    {
        nonmeld,
        meld_result,
        meld_part
    }

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

        private JObject cardJson;

        private ObservableCollection<Card> relatedCards;
        private CardFaceDetail backFace;
        private Layout cardLayout;
        private int cmc;
        private string displayName;
        private CardFaceDetail frontFace;
        private int qty;
        private CardFaceDetail selectedCardFaceDetail;
        private MeldComponentType meldType;

        /// <summary>
        /// The cards related to this card by any particular mechanic; includes tokens
        /// </summary>
        public ObservableCollection<Card> RelatedCards { get => relatedCards; set => SetProperty(ref relatedCards, value); }

        /// <summary>
        /// Return a new <c>Card</c> with default values
        /// </summary>
        public Card()
        {
            relatedCards = new ObservableCollection<Card>();
            qty = 0;
            cmc = 0;
            displayName = "Unassigned";
            cardLayout = Layout.unassigned;
            frontFace = new CardFaceDetail();
            backFace = new CardFaceDetail();
            selectedCardFaceDetail = frontFace;
            meldType = MeldComponentType.nonmeld;
        }

        //private constructor for initializing name field to delay running async command
        private Card(string name) : this()
        {
            displayName = name;
        }

        /// <summary>
        /// Makes an exact request to scryfall based on displayname of card, and initializes object with response
        /// </summary>
        public async Task PopulateFromName()
        {
            string response = await ScryfallBridge.FindExact(DisplayName);
            ReadJson(response);
        }

        //read a json reponse
        private void ReadJson(string response)
        {
            bool isCMCSingle;

            string strLayout,
                   matchingComponentType;

            int frontCMC;

            JToken meldToken;

            //read response json
            cardJson = JObject.Parse(response);

            //read the layout, replace json token class with enum property _class
            strLayout = (string)cardJson.SelectToken("layout");
            strLayout = strLayout.Equals("class") ? "_class" : strLayout;
            //if scryfall returns an unknown layout type, throw
            if (!EnumUtil.TryParse(strLayout, out Layout layout))
            {
                throw new ArgumentException(String.Format("Scryfall returned an unimplemented card layout: {0}", strLayout), "response");
            }

            //set high level properties
            DisplayName = ((string)cardJson.SelectToken("name"));
            Qty = qty;
            CardLayout = layout;

            //if meld
            if (CardLayout == Layout.meld)
            {
                //get all meld compenents
                meldToken = cardJson.SelectToken("all_parts");

                var related = meldToken
                    .Select
                    (
                        x => new Card
                             (
                                (string)x.SelectToken("name")
                             )
                    );
                RelatedCards = new ObservableCollection<Card>(related);


                //get meld component that is this card
                var mmeldToken = meldToken
                    .Where(x => ((string)x.SelectToken("name")).Equals(DisplayName))
                    .Select(x => (string)x.SelectToken("component"))
                    .First();

                if (!EnumUtil.TryParse(mmeldToken, out MeldComponentType meld))
                {
                    throw new ArgumentException(String.Format("Scryfall return unexpected meld component type: {0}", meldToken), response);
                }

                MeldType = meld;
            }

            //get properties of layout
            PropertySettings properties = EnumUtil.GetPropertySettings(CardLayout);
            isCMCSingle = properties.ConvertedManaCost.PropertyMode == PropertyMode.Single;

            SetTarget(properties.Name, (x, y) =>
            {
                FrontFace.Name = (string)x[0];
                if (y == PropertyMode.Double)
                    BackFace.Name = (string)x[1];
                else
                    DisplayName = (string)x[0];
            });
            SetTarget(properties.Type, (x, y) =>
            {
                FrontFace.TypeLine = (string)x[0];
                if (y == PropertyMode.Double)
                    BackFace.TypeLine = (string)x[1];
            });
            SetTarget(properties.ArtFaces, (x, y) =>
            {
                FrontFace.ImageURI = (string)x[0];
                if (y == PropertyMode.Double)
                    BackFace.ImageURI = (string)x[1];
            });
            SetTarget(properties.ColorIdentities, (x, y) =>
            {
                AddIdentityToFace(FrontFace, x[0]);
                if (y == PropertyMode.Double)
                    AddIdentityToFace(BackFace, x[1]);
            });
            SetTarget(properties.ConvertedManaCost, (x, y) =>
            {
                //assign front cmc to upper cmc token if single, or convert from child mana_cost tokens if double
                frontCMC = isCMCSingle ? (int)x[0] : ConvertManaCostToCMC((string)x[0]);
                FrontFace.ConvertedManaCost = frontCMC;
                ConvertedManaCost = FrontFace.ConvertedManaCost;
                if (y == PropertyMode.Double)
                    BackFace.ConvertedManaCost = ConvertManaCostToCMC((string)x[1]);
            });
        }

        /// <summary>
        /// Return a card populated by a scryfall response to <c>cards/named?exact=</c>
        /// </summary>
        /// <param name="response">A response from the scryfall api at cards/named?exact=</param>
        /// <param name="qty">The number of cards this card entry object represents</param>
        /// <exception cref="JsonReaderException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public Card(string response, int qty) : this()
        {
            Qty = qty;
            ReadJson(response);
        }

        /// <summary>
        /// Reference to the back face of the card
        /// </summary>
        public CardFaceDetail BackFace { get => backFace; set => SetProperty(ref backFace, value); }

        /// <summary>
        /// The layout type of the card
        /// </summary>
        public Layout CardLayout { get => cardLayout; set => SetProperty(ref cardLayout, value); }

        /// <summary>
        /// The card's converted mana cost
        /// </summary>
        public int ConvertedManaCost { get => cmc; set => SetProperty(ref cmc, value); }

        /// <summary>
        /// The display name of the entire card
        /// </summary>
        public string DisplayName { get => displayName; set => SetProperty(ref displayName, value); }

        /// <summary>
        /// Reference to the front face of a card
        /// </summary>
        public CardFaceDetail FrontFace { get => frontFace; set => SetProperty(ref frontFace, value); }

        /// <summary>
        /// The number of cards associated with this card entry
        /// </summary>
        public int Qty { get => qty; set => SetProperty(ref qty, value); }

        /// <summary>
        /// A reference to the currently selected face
        /// </summary>
        public CardFaceDetail SelectedCardFaceDetail { get => selectedCardFaceDetail; set => SetProperty(ref selectedCardFaceDetail, value); }

        /// <summary>
        /// The role this card plays in a meld
        /// </summary>
        public MeldComponentType MeldType { get => meldType; set => SetProperty(ref meldType, value); }

        /// <summary>
        /// Memberwise clone a Card object
        /// </summary>
        /// <returns>A new Card object with the same values as this Card object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
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

        /// <exception cref="ArgumentException"></exception>
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
        }

        /// <exception cref="ArgumentException"></exception>
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
                    throw new ArgumentException("Unexpected color identifier from Scryfall", "color");
            }

            return c;
        }

        private int ConvertManaCostToCMC(string mana_cost)
        {
            //stop execution if no mana cost string
            if (mana_cost.Equals(String.Empty))
                return 0;

            int cmc = 0;

            //change {x}{y} format to x,y,
            mana_cost = mana_cost.Replace("{", "").Replace("}", ",");
            //remove trailing comma
            mana_cost = mana_cost.Remove(mana_cost.Length - 1);

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

        private void SetTarget(LayoutProperty property, Action<JToken[], PropertyMode> action)
        {
            JToken jFaces;
            JToken[] tokens = new JToken[2];
            string safeToken = property.JSONToken.ToString();

            //compensate for tokens named with any '.'
            if (safeToken == JSONToken.image_uris_normal.ToString())
                safeToken = "image_uris.normal";

            if (property.PropertyMode == PropertyMode.Single)
            {
                tokens[0] = cardJson.SelectToken(safeToken);
            }
            else if (property.PropertyMode == PropertyMode.Double)
            {
                jFaces = cardJson.SelectToken("card_faces");

                tokens[0] = jFaces[0].SelectToken(safeToken);
                tokens[1] = jFaces[1].SelectToken(safeToken);
            }

            action(tokens, property.PropertyMode);
        }
    }
}