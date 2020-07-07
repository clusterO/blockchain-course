using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using Wallet.Models;
using Wallet.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Wallet.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();

            var blocks = GetChain();
            var transactions = TransactionByAddress(Credential.PublicKey, blocks);
            decimal balance = 0;
            decimal receives = 0;
            decimal deduct = 0;
            List<string> listString = new List<string>();
            foreach(var item in transactions)
            {
                if(item.Recipient == Credential.PublicKey)
                {
                    balance += item.Amount;
                    receives += item.Amount;
                }
                else
                {
                    balance -= item.Amount;
                    receives -= item.Amount;
                }

                listString.Add(item.Sender + " sent " + item.Amount + " to " + item.Recipient);
            }

            txtReceives.Text = receives.ToString();
            txtDeduct.Text = deduct.ToString();
            txtBalance.Text = balance.ToString();

            list.ItemsSource = listString;
        }

        private List<Transaction> TransactionByAddress(string address, List<Block> chain)
        {
            List<Transaction> transactions = new List<Transaction>();
            foreach (var block in chain.OrderByDescending(x => x.Index))
            {
                var ownerTransactions = block.Transactions.Where(x => x.Sender == address || x.Recipient == address);
                transactions.AddRange(ownerTransactions);
            }

            return transactions;
        }

        private List<Block> GetChain()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var url = new Uri("" + "/api/chain");
                var response = client.GetAsync(url).Result;

                var content = response.Content.ReadAsStringAsync().Result;
                var model = new
                {
                    chain = new List<Block>(),
                    length = 0
                };

                var data = JsonConvert.DeserializeAnonymousType(content, model);

                return data.chain;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}