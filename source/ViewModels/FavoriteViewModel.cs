using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.ViewModels
{
    public class FavoriteViewModel : GamePropertyViewModel<bool>
    {
        public FavoriteViewModel() 
            : base(nameof(Playnite.SDK.Models.Game.Favorite), g => g.Favorite, (g, v) => g.Favorite = v)
        {

        }
    }
}
