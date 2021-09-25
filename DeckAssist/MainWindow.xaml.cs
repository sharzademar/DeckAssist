using System.Windows;
using DeckAssist.ViewModel;
using DeckAssist.Http;

namespace DeckAssist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var viewModel = new MainViewModel();
            
            DataContext = viewModel;
            HttpClientManager.UseProperTLS();
            InitializeComponent();
        }
    }
}
