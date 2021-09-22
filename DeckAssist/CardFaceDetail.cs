using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeckAssist
{
    public class CardFaceDetail : ViewModelBase
    {
        private const string cardBack = "https://c1.scryfall.com/file/scryfall-card-backs/normal/59/597b79b3-7d77-4261-871a-60dd17403388.jpg";

        private int cmc;
        private string name;
        private string imageURI;
        private string typeLine;
        private ObservableCollection<ColorIdentity> colorIdentities;

        public string Name { get => name; set => SetProperty(ref name, value); }
        public string ImageURI { get => imageURI; set => SetProperty(ref imageURI, value); }
        public string TypeLine { get => typeLine; set => SetProperty(ref typeLine, value); }
        public ObservableCollection<ColorIdentity> ColorIdentities { get => colorIdentities; set => SetProperty(ref colorIdentities, value); }
        public int ConvertedManaCost { get => cmc; set => SetProperty(ref cmc, value); }

        public CardFaceDetail()
        {
            //initialize with unusable values to indicate value not relevant unless manually assigned
            cmc = -1;
            name = "Unassigned";
            typeLine = "Unassigned";
            imageURI = cardBack; //default to card back
            colorIdentities = new ObservableCollection<ColorIdentity>(); //default to accept new id
        }
    }
}
