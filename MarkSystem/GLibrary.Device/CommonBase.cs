using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GLibrary.Device
{
    public abstract class CommonBase : IDisposable
    {
        public abstract void Dispose();

        public event TypedEventHandler<object, ExceptionArgs> OnException;

        internal void RaiseOnException(object sender,Exception Exception)
        {
            OnException?.Invoke(sender, new ExceptionArgs(Exception));
        }
    }

    public class ExceptionArgs
    {
        public Exception Exception;
        public ExceptionArgs(Exception Exception)
        {
            this.Exception = Exception;
        }
    }
}
