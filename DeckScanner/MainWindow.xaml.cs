using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using HtmlAgilityPack;

namespace DeckScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string baseUrl = "http://metastats.net";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            //var slowTask = Task<int>.Factory.StartNew(WriteTest);
            //await slowTask;
            
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            doc = hw.Load(textBox.Text);
            //var urlList = new List<string>();

            var classSites = doc.DocumentNode.SelectNodes("//div[@id='meta-nav']/ul/li/a/@href");
            
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                tbResult.AppendText(String.Format("Found {0} classSites\r\n", classSites.Count));
                tbResult.ScrollToEnd();
            }));
            //tbResult.Refresh();
            //Console.WriteLine(String.Format("Found {0} classSites", classSites.Count));

            var tasks = new List<Task<IList<Deck>>>();
            var decks = new List<Deck>();

            foreach (HtmlNode link in classSites)
            {
                //HtmlNode link = classSites[0];
                // Get the value of the HREF attribute
                string hrefValue = link.GetAttributeValue("href", string.Empty);
                var task = Task.Run(() => GetClassDecks(baseUrl + hrefValue));
                tasks.Add(task);
                //await GetClassDecks(baseUrl + hrefValue, decks);
            }
            await Task.WhenAll(tasks);
            foreach (var t in tasks)
            {
                decks.AddRange(t.Result);
            }
            
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { 
                tbResult.AppendText(String.Format("FINISHED! Created {0} decks.", decks.Count));
                tbResult.ScrollToEnd();
            }));
            //tbResult.Refresh();
            //Console.WriteLine(String.Format("FINISHED! Created {0} decks.", decks.Count));

            button.IsEnabled = true;
        }

        /// <summary>
        /// Gets all decks for a given class URL.
        /// </summary>
        /// <param name="url">The URL of the class</param>
        /// <param name="decks">The list of decks to be filled</param>
        /// <returns></returns>
        private async Task<IList<Deck>> GetClassDecks(string url)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            doc = hw.Load(url);

            var deckSites = doc.DocumentNode.SelectNodes("//div[@class='decklist']/div/h4/a/@href");
            //var deckUrls = new List<string>();
            
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                tbResult.AppendText(String.Format("-Found {0} deckSites\r\n", deckSites.Count));
                tbResult.ScrollToEnd();
            }));
            //tbResult.Refresh();
            //Console.WriteLine(String.Format("-Found {0} deckSites", deckSites.Count));

            var decks = new List<Deck>();

            foreach (HtmlNode link in deckSites)
            {
                string hrefValue = link.GetAttributeValue("href", string.Empty);
                var result = await Task.Run(() => GetDeck(baseUrl + hrefValue));
                decks.Add(result);
            }

            return decks;
        }

        /// <summary>
        /// Gets a deck from the meta description of a website.
        /// </summary>
        /// <param name="url">The URL to the website</param>
        /// <returns></returns>
        private async Task<Deck> GetDeck(string url)
        {
            var result = await MetaTagImporter.TryFindDeck(url);
            
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                tbResult.AppendText(String.Format("--Created {0}: {1} deck with {2} cards\r\n", result.Name, result.Class, result.Cards.Count));
                tbResult.ScrollToEnd();
            }));
            //tbResult.Refresh();
            //Console.WriteLine(String.Format("--Created {0}: {1} deck with {2} cards", result.Name, result.Class, result.Cards.Count));

            return result;
        }
    }
}
