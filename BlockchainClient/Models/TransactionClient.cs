using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainClient.Models
{
    public class TransactionClient
    {
        public decimal Amount { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public string PrivateKey { get; set; }
        public decimal Fees { get; set; }

        public override string ToString()
        {
            return $"{Amount.ToString("0.00000000")} {Recipient} {Sender}";
        }
    }
}
