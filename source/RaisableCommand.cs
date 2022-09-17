using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Extras
{
    public class RaisableCommand : ICommand
    {
        private readonly Func<bool> canExecute;
        private readonly Action execute;

        public event EventHandler CanExecuteChanged;

        public RaisableCommand(Action execute)
            : this(execute, null)
        {
        }

        public RaisableCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter = null)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute();
        }

        public void Execute(object parameter = null)
        {
            execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RaisableCommand<T> : ICommand
    {
        private readonly Func<T, bool> canExecute;
        private readonly Action<T> execute;

        public event EventHandler CanExecuteChanged;

        public RaisableCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RaisableCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter = null)
        {
            if (canExecute == null)
            {
                return true;
            }

            if (parameter is T value)
            {
                return canExecute(value);
            }

            return false;
        }

        public void Execute(object parameter = null)
        {
            if (parameter is T value)
            {
                execute(value);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
