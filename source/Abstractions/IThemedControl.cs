using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Extras.Abstractions
{
    public interface IThemedControl
    {
        LocalValueEnumerator GetLocalValueEnumerator();
        void SetCurrentValue(DependencyProperty dp, object value);
        BindingExpressionBase SetBinding(DependencyProperty dp, BindingBase binding);
    }
}
