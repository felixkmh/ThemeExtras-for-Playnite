using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Extras.ViewModels
{
    public class CompletionStatusViewModel : GamePropertyViewModel<CompletionStatus>
    {
        private static List<CompletionStatus> completionStatusOptions = null;
        public List<CompletionStatus> CompletionStatusOptions
        {
            get => completionStatusOptions;
            set => SetValue(ref completionStatusOptions, value);
        }

        public CompletionStatusViewModel() : base(
                nameof(Playnite.SDK.Models.Game.CompletionStatus),
                g => g.CompletionStatus,
                (g, v) => g.CompletionStatusId = v.Id)
        {
            var completionStatuses = Playnite.SDK.API.Instance.Database.CompletionStatuses;
            completionStatuses.ItemCollectionChanged += CompletionStatuses_ItemCollectionChanged;
        }

        protected override void GameChanged(Game oldValue, Game newValue)
        {
            var completionStatuses = Playnite.SDK.API.Instance.Database.CompletionStatuses;
            if (CompletionStatusOptions is null)
            {
                CompletionStatusOptions = new CompletionStatus[] { Playnite.SDK.Models.CompletionStatus.Empty }.Concat(completionStatuses.OfType<CompletionStatus>()).OrderBy(st => st?.Name ?? "").ToList();
            }
            base.GameChanged(oldValue, newValue);
        }

        private void CompletionStatuses_ItemCollectionChanged(object sender, Playnite.SDK.ItemCollectionChangedEventArgs<Playnite.SDK.Models.CompletionStatus> e)
        {
            var completionStatuses = Playnite.SDK.API.Instance.Database.CompletionStatuses;
            CompletionStatusOptions = new CompletionStatus[] { Playnite.SDK.Models.CompletionStatus.Empty }.Concat(completionStatuses.OfType<CompletionStatus>()).OrderBy(st => st?.Name ?? "").ToList();
        }

        public override void Dispose()
        {
            var completionStatuses = Playnite.SDK.API.Instance?.Database?.CompletionStatuses;
            if (completionStatuses != null)
            {
                completionStatuses.ItemCollectionChanged -= CompletionStatuses_ItemCollectionChanged;
            }
            base.Dispose();
        }
    }
}