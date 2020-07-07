using BlockchainClient.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainClient.API
{
    [Produces("application/json")]
    [Route("")]
    [ApiController]
    public class BlockchainClientController : ControllerBase
    {
        [HttpGet("wallet/new")]
        public IActionResult newWallet()
        {
            var wallet = RSA.RSA.GenerateKey();
            var response = new
            {
                privateKey = wallet.PrivateKey,
                publicKey = wallet.PublicKey
            };

            return Ok(response);
        }

        [HttpPost("generate/transaction")]
        public IActionResult newTransaction(TransactionClient transaction)
        {
            var sign = RSA.RSA.Sign(transaction.PrivateKey, transaction.ToString());
            var response = new { transaction = transaction, signature = sign };
            
            return Ok(response);
        }
    }
}
