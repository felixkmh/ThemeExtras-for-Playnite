using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Extras.ViewModels
{
    public class GamePropertyViewModel<T> : ObservableObject, IStylableViewModel
    {
        public GamePropertyViewModel(string propertyName, Func<Game, T> getValue = null, Action<Game, T> setValue = null)
        {
            this.propertyName = propertyName;
            PropertyInfo property = null; 
            if (getValue == null || setValue == null)
            {
                typeof(Game).GetProperty(propertyName);
            }
            if (getValue != null)
            {
                SetGameProperty = setValue;
            } else
            {
                SetGameProperty = (g, v) => property.SetValue(g, v);
            }
            if (setValue != null)
            {
                GetGameProperty = getValue;
            } else
            {
                GetGameProperty = g => (T)property.GetValue(g);
            }
            PropertyChanged += GamePropertyViewModel_PropertyChanged;
        }

        private void GamePropertyViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == propertyName)
            {
                SetGameProperty(Game, Value);
            }
        }

        private string propertyName;

        private Game game;
        public Game Game
        {
            get => game;
            set { var oldValue = game; SetValue(ref game, value); GameChanged(oldValue, value); }
        }

        private T _value;
        public T Value
        {
            get => _value;
            set { var oldValue = _value; SetValue(ref _value, value); ChangeGameProperty(oldValue, value); }
        }

        private Func<Game, T> GetGameProperty;

        private Action<Game, T> SetGameProperty;

        private void ChangeGameProperty(T oldValue, T newValue)
        {
            if (Game is Game)
            {
                if (!Equals(oldValue, newValue))
                {
                    var prevSelected = Playnite.SDK.API.Instance.MainView.SelectedGames.ToList();
                    SetGameProperty(Game, newValue);
                    Playnite.SDK.API.Instance.Database.Games.Update(Game);
                    var newSelected = Playnite.SDK.API.Instance.MainView.SelectedGames.ToList();
                    if (!Enumerable.SequenceEqual(prevSelected, newSelected))
                    {
                        Playnite.SDK.API.Instance.MainView.SelectGames(prevSelected.Select(g => g.Id));
                    }
                }

            }
        }

        protected virtual void GameChanged(Game oldValue, Game newValue)
        {
            if (oldValue is Game)
            {
                oldValue.PropertyChanged -= Game_PropertyChanged;
                // Playnite.SDK.API.Instance.Database.Games.Update(oldValue);
            }
            if (newValue is Game)
            {
                newValue.PropertyChanged += Game_PropertyChanged;
                _value = GetGameProperty(Game);
                OnPropertyChanged(nameof(Value));
            }
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Game game)
            {
                if (e.PropertyName == propertyName)
                {
                    _value = GetGameProperty(Game);
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public virtual void Dispose()
        {
            if (Game is Game)
            {
                Game.PropertyChanged -= Game_PropertyChanged;
            }
            GC.SuppressFinalize(this);
        }
    }
}
