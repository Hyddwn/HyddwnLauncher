using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility.Model
{
    public class NexonUserTagV1Response
    {
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }
        public string GlobalUserNumber { get; set; }
        public string NexonTag { get; set; }
        public int NexonTagId { get; set; }
        public int RenamesRemaining { get; set; }
        public int TagId { get; set; }
    }
}
