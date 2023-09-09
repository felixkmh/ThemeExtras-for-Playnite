using Extras.Models.NamedItems;
using Extras.ViewModels.Objects;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Interaktionslogik für EditableTags.xaml
    /// </summary>
    public partial class EditableTags
    {

        public ObservableCollection<Tag> Collection { get; set; }

        public ICommand AcceptCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand AutoCompleteCommand { get; set; }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (oldContext is Game old)
            {
                foreach(var tag in _volatileTags)
                {
                    if (old.Tags?.FirstOrDefault(id => tag.Id == id.Id) is null)
                    {
                        API.Instance.Database.Tags.Remove(tag);
                    }
                }
            }

            _volatileTags.Clear();

            if (newContext is Game && oldContext is null)
            {
                API.Instance.Database.Tags.ItemCollectionChanged += Tags_ItemCollectionChanged;
                Collection.Clear();
                API.Instance.Database.Tags.ForEach(t => Collection.Add(t));
            } else if (newContext is null)
            {
                API.Instance.Database.Tags.ItemCollectionChanged -= Tags_ItemCollectionChanged;
            }
            
        }

        private List<Tag> _volatileTags { get; set; } = new List<Tag>();

        public EditableTags()
        {
            InitializeComponent();
            Collection = new ObservableCollection<Tag>(Playnite.SDK.API.Instance.Database.Tags);
            AcceptCommand = new RelayCommand<EditableItemsControl.MatchData>(match =>
            {
                if (match?.Matches?.OfType<Tag>().FirstOrDefault() is Tag tag)
                {
                    GameContext.TagIds = GameContext.TagIds.Concat(new[] { tag.Id }).Distinct().ToList();
                    Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
                } else if (!string.IsNullOrWhiteSpace(match.Filter))
                {
                    var newTag = API.Instance.Database.Tags.Add(match.Filter);
                    _volatileTags.Add(newTag);
                    GameContext.TagIds = GameContext.TagIds.Concat(new[] { newTag.Id }).Distinct().ToList();
                    Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
                }
            });

            RemoveCommand = new RelayCommand<DatabaseObject>(o =>
            {
                if (o is Tag tag)
                {
                    GameContext.TagIds = GameContext.TagIds.Where(t => t != o.Id).ToList();
                    Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
                }
            });

            EditableItemsControl.AutoCompleteCommand = new RelayCommand<TextBox>(textbox =>
            {
                EditableItemsControl.FilterText = EditableItemsControl.BestMatch;
                if (textbox != null)
                    textbox.CaretIndex = textbox.Text.Length;
            }, textbox => !string.IsNullOrWhiteSpace(EditableItemsControl.BestMatch));

            DataContext = this;
        }

        private void Tags_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Tag> e)
        {
            Collection.Clear();
            API.Instance.Database.Tags.ForEach(t => Collection.Add(t));
        }
    }
}
