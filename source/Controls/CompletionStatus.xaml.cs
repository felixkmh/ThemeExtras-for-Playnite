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
        ObservableCollection<Playnite.SDK.Models.CompletionStatus> statuses = new ObservableCollection<Playnite.SDK.Models.CompletionStatus>();

        public CompletionStatus()
        {
            InitializeComponent();
            IsVisibleChanged += CompletionStatus_IsVisibleChanged;
        }

        private void CompletionStatus_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Playnite.SDK.Models.CompletionStatus completion;

            foreach (var singleStatus in Playnite.SDK.API.Instance.Database.CompletionStatuses)
            {
                completion = new Playnite.SDK.Models.CompletionStatus
                {
                    Id = singleStatus.Id,
                    Name = singleStatus.Name
                };
                statuses.Add(completion);
            }

            CompletionComboBox.ItemsSource = statuses;

            if (e!= null && !(e.NewValue is Visibility.Visible))
            {
                Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);
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
