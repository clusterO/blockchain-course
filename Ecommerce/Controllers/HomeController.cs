using Ecommerce.Hubs;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public HomeController(IHubContext<ChatHub> hubcontext)
        {
            HubContext = hubcontext;
        }

        private IHubContext<ChatHub> HubContext
        {
            get;
            set;
        }

        public IActionResult Index()
        {
            var listItem = ListItem.Items();
            ViewBag.Items = listItem;
            return View();
        }

        public IActionResult GenerateQR()
        {
            return View();
        }

        public async Task<IActionResult> APICall(string ip, int id)
        {
            await this.HubContext.Clients.All();
            OwnedItem.AddUser(ip, id);
            return Content("successfull");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
