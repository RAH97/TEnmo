using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TenmoClient.Models
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public int AccountFromId { get; set; }
        public int AccountToId { get; set; }
        public decimal Amount { get; set; }

        public TransferStatus Status { get; set; }
        public TransferType Type { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }












    }
}
