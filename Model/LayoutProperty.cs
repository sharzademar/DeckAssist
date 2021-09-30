using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeckAssist.Model
{
    /// <summary>
    /// Represents the name of a particular JSON token
    /// </summary>
    public enum JSONToken
    {
        name,
        image_uris_normal,
        colors,
        type_line,
        cmc,
        mana_cost,
    }

    /// <summary>
    /// Represents the ways a LayoutProperty can behave
    /// </summary>
    public enum PropertyMode
    {
        Single,
        Double
    }

    /// <summary>
    /// Represents the name of the json token associated this property, along with its mode
    /// </summary>
    public class LayoutProperty
    {
        /// <summary>
        /// The mode of the property
        /// </summary>
        public PropertyMode PropertyMode { get; set; }
        /// <summary>
        /// Represents the name of the json token of the property
        /// </summary>
        public JSONToken JSONToken { get; set; }
    }
}
