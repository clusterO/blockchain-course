using Blockchain.API;
using Blockchain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Blockchain.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static Cryptocurrency blockchain = BlockchainController.blockchain;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Transaction> transactions = blockchain.GetTransactions();
            ViewBag.Transactions = transactions;

            List<Block> blocks = blockchain.GetBlocks();
            ViewBag.blocks = blocks;

            return View();
        }

        public IActionResult Mine()
        {
            blockchain.Mine();
            return RedirectToAction("Index");
        }

        public IActionResult Configure()
        {
            return View(blockchain.GetNodes());
        }

        public IActionResult RegisterNodes(string nodes)
        {
            string[] node = nodes.Split(',');
            blockchain.RegisterNodes(node);
            return RedirectToAction("Configure");
        }

        public IActionResult Coinbase()
        {
            List<Block> blocks = blockchain.GetBlocks();
            ViewBag.Blocks = blocks;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
