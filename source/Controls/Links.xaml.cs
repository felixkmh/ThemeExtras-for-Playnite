using Extras.Models;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (oldContext is Game)
            {
                oldContext.PropertyChanged -= Game_PropertyChanged;
                if (oldContext.Links is ObservableCollection<Link>)
                {
                    oldContext.Links.CollectionChanged -= Links_CollectionChanged;
                }
            }
            links.Clear();
            if (newContext is Game game)
            {
                game.PropertyChanged += Game_PropertyChanged;
                if (game.Links is ObservableCollection<Link>)
                {
                    game.Links.CollectionChanged += Links_CollectionChanged;
                }
                game.Links?.ForEach(l =>
                {
                    links.Add(new LinkExt(l));
                });
            }
        }

        private void Links_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Game.Links))
            {
                links.Clear();
                GameContext.Links?.ForEach(l =>
                {
                    links.Add(new LinkExt(l));
                });
            }
        }
    }
}
