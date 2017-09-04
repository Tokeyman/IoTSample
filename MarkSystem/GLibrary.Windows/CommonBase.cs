using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows
{
    public abstract class CommonBase : IDisposable
    {
        public abstract void Dispose();

        public event EventHandler<ExceptionArgs> OnException;

        internal void RaiseOnException(object sender, Exception Exception)
        {
            OnException?.Invoke(sender, new ExceptionArgs(Exception));
        }

    }

    public class ExceptionArgs : EventArgs
    {
        public Exception Exception;
        public ExceptionArgs(Exception Exception)
        {
            this.Exception = Exception;
        }
    }
}
