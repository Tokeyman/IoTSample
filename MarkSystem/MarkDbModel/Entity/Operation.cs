using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkDbModel.Entity
{
    public class Operation
    {
        [Key]
        public int Id { get; set; }

        public string TargetGuid { get; set; }
        public string Action { get; set; }

        public Operation(string TargetGuid,string Action)
        {
            this.TargetGuid = TargetGuid;
            this.Action = Action;
        }
    }
}
