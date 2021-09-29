using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeckAssist.Model
{
    /// <summary>
    /// Defines the settings for the properties of a card or card face
    /// </summary>
    public class PropertySettings
    {
        private void InitSettings(PropertyMode mode)
        {
            Name = new LayoutProperty
            {
                JSONToken = JSONToken.name,
                PropertyMode = mode
            };
            Type = new LayoutProperty
            {
                JSONToken = JSONToken.type_line,
                PropertyMode = mode
            };
            ArtFaces = new LayoutProperty
            {
                JSONToken = JSONToken.image_uris_normal,
                PropertyMode = mode
            };
            ColorIdentities = new LayoutProperty
            {
                JSONToken = JSONToken.colors,
                PropertyMode = mode
            };
            ConvertedManaCost = new LayoutProperty
            {
                JSONToken = JSONToken.cmc,
                PropertyMode = mode
            };
        }

        /// <summary>
        /// Initializes the propety mode of each property as the provided mode
        /// </summary>
        /// <param name="mode">The PropertyMode the property should contain</param>
        public PropertySettings(PropertyMode mode = PropertyMode.Single)
        {
            InitSettings(mode);
        }

        public LayoutProperty Name { get; set; }
        public LayoutProperty Type { get; set; }
        public LayoutProperty ArtFaces { get; set; }
        public LayoutProperty ColorIdentities { get; set; }
        public LayoutProperty ConvertedManaCost { get; set; }
    }
}
