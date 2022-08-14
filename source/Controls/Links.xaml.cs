using Extras.Models;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
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

namespace Extras.Controls
{
    /// <summary>
    /// Interaktionslogik für Links.xaml
    /// </summary>
    public partial class Links : PluginUserControl
    {
        private ObservableCollection<LinkExt> links = new ObservableCollection<LinkExt>();

        public Links()
        {
            InitializeComponent();
            LinksItemsControl.ItemsSource = links;
        }

        public override async void GameContextChanged(Game oldContext, Game newContext)
        {
            if (oldContext is Game)
            {
                oldContext.PropertyChanged -= Game_PropertyChanged;
                if (oldContext.Links is ObservableCollection<Link>)
                {
                    oldContext.Links.CollectionChanged -= Links_CollectionChanged;
                }
            }
            if (newContext is Game game)
            {
                game.PropertyChanged += Game_PropertyChanged;
                if (game.Links is ObservableCollection<Link>)
                {
                    game.Links.CollectionChanged += Links_CollectionChanged;
                }
                await UpdateLinks(game);
            }
        }

        private async Task UpdateLinks(Game game)
        {
            links.Clear();
            using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(1) })
            {
                if (game.Links is ObservableCollection<Link>)
                {
                    foreach (var l in game.Links)
                    {
                        LinkExt link = new LinkExt(l);
                        link.Icon = await LinkExt.GetIconAsync(httpClient, link.Url);
                        links.Add(link);
                    }
                }
            };
        }

        private async void Links_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await UpdateLinks(GameContext);
        }

        private async void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Game.Links))
            {
                if (GameContext.Links is ObservableCollection<Link>)
                {
                    GameContext.Links.CollectionChanged -= Links_CollectionChanged;
                    GameContext.Links.CollectionChanged += Links_CollectionChanged;
                }
                await UpdateLinks(GameContext);
            }
        }
    }
}
