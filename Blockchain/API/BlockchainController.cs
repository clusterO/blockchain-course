using Blockchain.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blockchain.API
{
    [EnableCors("CorsPolicy")]
    [Produces("application/json")]
    [Route("")]
    [ApiController]
    public class BlockchainController : ControllerBase
    {
        public static Cryptocurrency blockchain = new Cryptocurrency();

        [HttpPost("transactions/new")]
        public IActionResult newTransaction([FromBody]Transaction transaction)
        {
            var response = blockchain.CreateTransaction(transaction);
            return Ok(response);
        }

        [HttpGet("transactions/get")]
        public IActionResult getTransaction()
        {
            var response = new { transactions = blockchain.GetTransactions() };
            return Ok(response);
        }

        [HttpGet("chain")]
        public IActionResult chain()
        {
            var blocks = blockchain.GetBlocks();
            var response = new { chain = blocks, length = blocks.Count };
            return Ok(response);
        }

        [HttpGet("mine")]
        public IActionResult mine()
        {
            var block = blockchain.Mine();
            var response = new
            {
                message = "New block created",
                blockNumber = block.Index,
                transactions = block.Transactions.ToArray(),
                nonce = block.Proof,
                previousHash = block.PreviousHash
            };

            return Ok(response);
        }

        [HttpPost("nodes/register")]
        public IActionResult registerNodes(string[] nodes)
        {
            blockchain.RegisterNodes(nodes);
            var response = new
            {
                message = "New nodes have been added",
                numberOfNodes = nodes.Count()
            };

            return Created("", response);
        }

        [HttpGet("nodes/resolve")]
        public IActionResult consensus()
        {
            return Ok(blockchain.Consensus());
        }

        [HttpGet("nodes/get")]
        public IActionResult getNodes()
        {
            return Ok(new { nodes = blockchain.GetNodes() });
        }

        [HttpGet("wallet/miner")]
        public IActionResult getMinerWallet()
        {
            return Ok(blockchain.GetMinderWaller());
        }
    }
}
