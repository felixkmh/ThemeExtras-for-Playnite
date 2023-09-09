using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.ViewModels
{
    public class EditableFeaturesViewModel : EditableCollectionViewModel<GameFeature>
    {
        public EditableFeaturesViewModel(IItemCollection<GameFeature> collection) : base(collection)
        {

        }
    }
}
