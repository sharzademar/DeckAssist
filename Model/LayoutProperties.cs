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
        public LayoutProperty Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The types of a card can't be Combined</exception>
        public LayoutProperty Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The art faces of a card can't be Combined</exception>
        public LayoutProperty ArtFaces { get; set; }
        public LayoutProperty ColorIdentities { get; set; }
        public LayoutProperty ConvertedManaCost { get; set; }
    }
}
