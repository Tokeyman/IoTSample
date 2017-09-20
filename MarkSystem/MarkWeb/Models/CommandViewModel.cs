using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkWeb.Models
{
    public class CommandViewModel
    {
        public string CommandGroupId { get; set; }

        [DataType(DataType.MultilineText)]
        public string TimingText { get; set; }

        [DataType(DataType.MultilineText)]
        public string RepeatText { get; set; }
    }
}