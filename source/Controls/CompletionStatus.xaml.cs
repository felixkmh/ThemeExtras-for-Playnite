using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// For CompletionStatus.xaml
    /// </summary>
    public partial class CompletionStatus : Playnite.SDK.Controls.PluginUserControl
    {
        private ObservableCollection<Playnite.SDK.Models.CompletionStatus> completionStatuses;

        public CompletionStatus()
        {
            InitializeComponent();
            IsVisibleChanged += CompletionStatus_IsVisibleChanged;
        }

        private void CompletionStatus_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (e!= null && !(e.NewValue is Visibility.Visible))
            {
                Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {

            if (completionStatuses == null || !API.Instance.Database.CompletionStatuses.IsListEqual(completionStatuses.DefaultIfEmpty()))
            {
                completionStatuses = new ObservableCollection<Playnite.SDK.Models.CompletionStatus>(API.Instance.Database.CompletionStatuses);

            }

            base.GameContextChanged(oldContext, newContext);
            CompletionComboBox.ItemsSource = completionStatuses;
            CompletionComboBox.DataContext = newContext;
        }

        private void CompletionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Playnite.SDK.Models.CompletionStatus status1 = new Playnite.SDK.Models.CompletionStatus();

            foreach (Playnite.SDK.Models.CompletionStatus comp in Playnite.SDK.API.Instance.Database.CompletionStatuses)
            {
                if (comp.Name.Equals(e.AddedItems[0].ToString()))
                {
                    status1.Id = comp.Id;
                    break;
                }
            }

            GameContext.CompletionStatusId = status1.Id;

            Playnite.SDK.API.Instance.Database.Games.Update(GameContext);

        }
    }
}
