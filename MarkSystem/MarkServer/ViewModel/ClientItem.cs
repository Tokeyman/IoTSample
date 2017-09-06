using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace MarkServer.ViewModel
{
    public class ClientItem:ObservableObject
    {
        private string _IP;
        public string IP { get { return _IP; } set { Set(ref _IP, value); } }

        private string _Port;
        public string Port { get { return _Port; } set { Set(ref _Port, value); } }

        private string _Guid;
        public string Guid { get { return _Guid; } set { Set(ref _Guid, value); } }


        public ClientItem() { }
        public ClientItem(string IP,string Port,string Guid)
        {
            this.IP = IP;
            this.Port = Port;
            this.Guid = Guid;
        }
    }
}
