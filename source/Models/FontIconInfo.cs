using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.Models
{
    public class FontIconInfo
    {
        public string Name { get; set; }
        public UInt16 Code { get; set; }
        [DontSerialize]
        public char Char => System.Convert.ToChar(Code);
    }
}
