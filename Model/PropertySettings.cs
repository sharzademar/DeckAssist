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
        public LayoutProperty Name { get; set; }
        public LayoutProperty Type { get; set; }
        public LayoutProperty ArtFaces { get; set; }
        public LayoutProperty ColorIdentities { get; set; }
        public LayoutProperty ConvertedManaCost { get; set; }
    }
}
