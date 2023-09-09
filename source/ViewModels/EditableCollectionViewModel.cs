using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.ViewModels
{
    public abstract class EditableCollectionViewModel<T> : ObservableObject where T : DatabaseObject
    {
        private IItemCollection<T> itemPool = null;
        public IItemCollection<T> ItemPool { get => itemPool; set => SetValue(ref itemPool, value); }

        public EditableCollectionViewModel(IItemCollection<T> collection) 
        { 
            itemPool = collection;
            itemPool.ItemCollectionChanged += ItemPool_ItemCollectionChanged;
            itemPool.ItemUpdated += ItemPool_ItemUpdated;
        }

        private void ItemPool_ItemUpdated(object sender, ItemUpdatedEventArgs<T> e)
        {
            
        }

        private void ItemPool_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<T> e)
        {
            
        }
    }
}
