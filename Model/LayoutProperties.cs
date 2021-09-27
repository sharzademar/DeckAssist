using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeckAssist.Model
{
    public enum PropertyMode
    {
        Single,
        Double,
        Combined
    }

    public class LayoutProperties
    {
        private PropertyMode naming;
        private PropertyMode type;
        private PropertyMode artFaces;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The names of a card can't be combined</exception>
        public PropertyMode Naming { get => naming;
            set
            {
                if (value == PropertyMode.Combined)
                    throw new ArgumentOutOfRangeException("value", PropertyMode.Combined,
                        "The names of a card can't be combined");
                naming = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The types of a card can't be Combined</exception>
        public PropertyMode Type
        {
            get => type;
            set
            {
                if (value == PropertyMode.Combined)
                    throw new ArgumentOutOfRangeException("value", PropertyMode.Combined,
                        "The types of a card can't be combined");
                type = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The art faces of a card can't be Combined</exception>
        public PropertyMode ArtFaces
        {
            get => artFaces;
            set
            {
                if (value == PropertyMode.Combined)
                    throw new ArgumentOutOfRangeException("value", PropertyMode.Combined, "The art faces of a card can't be combined");
                artFaces = value;
            }
        }
        public PropertyMode ColorIdentities { get; set; }
        public PropertyMode ConvertedManaCost { get; set; }
        public Dictionary<string, PropertyMode> PropertiesFromTokenName { get; private set; }

        public LayoutProperties()
        {
            PropertiesFromTokenName = new Dictionary<string, PropertyMode>
            {
                { "name", Naming },
                { "image_uris.normal", ArtFaces },
                { "colors", ColorIdentities},
                { "cmc", ConvertedManaCost }
            };
        }
    }
}
