using DeckAssist.Http;
using DeckAssist.ViewModel;
using Serilog;
using System;
using System.Windows;

namespace DeckAssist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //initialize logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logfile.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //use proper security protocol
            HttpClientManager.UseProperTLS();

            //data context
            var viewModel = new MainViewModel();
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}