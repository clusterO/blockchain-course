using Ecommerce.Hubs;
using Ecommerce.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ecommerce.API
{
    [EnableCors("CorsPolicy")]
    [Produces("application/json")]
    [Route("api")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        public PaymentController(IHubContext<ChatHub> hubcontext)
        {
            HubContext = hubcontext;
        }

        private IHubContext<ChatHub> HubContext
        {
            get;
            set;
        }

        [HttpGet("chain")]
        public async Task<IActionResult> fullChain()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application"));
        
            try
            {
                var url = new Uri("https://localhost:11111" + "/chain");
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                var model = new
                {
                    chain = new List<Block>(),
                    length = 0
                };

                var data = JsonConvert.DeserializeAnonymousType(content, model);

                return Ok(data);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("Pay")]
        public async Task<IActionResult> MakePayment([FromBody] Transaction transaction, string ip, int pid)
        {
            var json = JsonConvert.SerializeObject(transaction);
            var uri = "https://localhost:11111/transactions/new";
            var stringContent = new StringContent(json, System.Text.UnicodeEncoding.UTF8, "application/json");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsync(uri, stringContent);
            var content = await response.Content.ReadAsStringAsync();
            var send = new { message = "" };
            var data = JsonConvert.DeserializeAnonymousType(content, send);

            if(data.message.Contains("Transaction will be added to block") && ip != null)
            {
                await this.HubContext.Clients.All.SendAsync(ip, pid, ListItem.Items().First(x => x.Id == pid).Url);
                OwnedItem.AddUser(ip, pid);
            }

            return Ok(data);
        }
    }
}
