using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSamples.ViewModel
{
    public class ComboBoxItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public ComboBoxItem(string Value,string Text)
        {
            this.Value = Value;
            this.Text = Text;   
        }
    }
}
