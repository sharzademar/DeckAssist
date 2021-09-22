using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DeckAssist
{
    public class ManaCurveCollection : ObservableConcurrentDictionary<int, ManaCurveDataPoint>
    {
        public int NumLands { get; set; }
        public int Sum { get; set; }
    }
}
