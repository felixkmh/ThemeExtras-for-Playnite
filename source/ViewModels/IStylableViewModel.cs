using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.ViewModels
{
    public interface IStylableViewModel : IDisposable
    {
        Game Game { get; set; }
    }
}
