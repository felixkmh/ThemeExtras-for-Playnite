using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Extras.ViewModels.Objects
{
    public class ItemsGenerator<TIn, TOut> : ObservableCollection<TOut>, IDisposable 
        where TIn : class 
        where TOut : class
    {
        private INotifyCollectionChanged _source;
        private Func<TIn, TOut> _generator;
        private bool _paused = false;

        public ItemsGenerator(INotifyCollectionChanged source, Func<TIn, TOut> generator) 
        {
            _source = source;
            _generator = generator;
            _source.CollectionChanged += _source_CollectionChanged;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_source != null)
                {
                    _source.CollectionChanged -= _source_CollectionChanged;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (_source is IList source && !_paused)
            {
                _source.CollectionChanged -= _source_CollectionChanged;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        source.RemoveAt(e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        var temp = source[e.OldStartingIndex];
                        source[e.OldStartingIndex] = source[e.NewStartingIndex];
                        source[e.NewStartingIndex] = temp;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        source.Clear();
                        break;
                    default:
                        break;
                }

                _source.CollectionChanged += _source_CollectionChanged;
            }
        }

        private void _source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _paused = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(_generator.Invoke(e.NewItems[0] as TIn));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this[e.OldStartingIndex] = _generator.Invoke(e.NewItems[0] as TIn);
                    break;
                case NotifyCollectionChangedAction.Move:
                    var temp = this[e.OldStartingIndex];
                    this[e.OldStartingIndex] = this[e.NewStartingIndex];
                    this[e.NewStartingIndex] = temp;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    break;
                default:
                    break;
            }

            _paused = false;
        }
    }
}
