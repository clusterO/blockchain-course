using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using Wallet.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace Wallet.Views
{
    public partial class AboutPage : ContentPage
    {
        public Transaction Transaction { get; set; }
        public AboutPage()
        {
            InitializeComponent();

            Transaction = new Transaction
            {
                Fees = 2,
                Recipient = "",
                Amount = 0
            };

            BindingContext = this;
        }

        private async void btnScanClicked(object sender, EventArgs e)
        {
            var ScannerPage = new ZXingScannerPage();
            ScannerPage.OnScanResult += (result) =>
            {
                ScannerPage.IsScanning = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();
                    txtBarcode.Text = result.Text;
                    Transaction.Signature = result.Text;
                    var str = result.Text.Split(new string[] { "amount=" }, StringSplitOptions.None);

                    if (str.Length > 1)
                        txtAmount.Text = str[1];
                });
            };

            await Navigation.PushAsync(ScannerPage);
        }

        private async void btnPayClicked(object sender, EventArgs e)
        {
            var dict = ParseQueryString(new Uri(Transaction.Signature));
            Transaction.Sender = Credential.PublicKey;
            Transaction.PrivateKey = Credential.PrivateKey;
            Transaction.Recipient = dict.GetValueOrDefault("recipient");
            Transaction.Amount = Convert.ToDecimal(dict.GetValueOrDefault("amount"));
            var url = Transaction.Signature.Split('?');
            var signature = RSA.Sign(Transaction.PrivateKey, Transaction.ToString());

            dict.Add("signature", signature);
            dict.Add("sender", Transaction.Sender);
            var json = JsonConvert.SerializeObject(dict);

            var stringContent = new StringContent(json, System.Text.UnicodeEncoding.UTF8, "application/json");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                string Url = url[0] + "?ip=" + dict.GetValueOrDefault("ip") + "&pid=" + dict.GetValueOrDefault("pid");
                var response = await client.PostAsync(Url, stringContent);

                var content = await response.Content.ReadAsStringAsync();
                var send = new { message = "" };
                var data = JsonConvert.DeserializeAnonymousType(content, send);

                if (data.message.Contains("Transaction will be added to block"))
                {
                    await DisplayAlert("Payment done", "Transaction completed", "OK");
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static Dictionary<string, string> ParseQueryString(Uri uri)
        {
            var query = uri.Query.Substring(uri.Query.IndexOf('?') + 1);
            var pairs = query.Split('&');

            return pairs
                .Select(o => o.Split('='))
                .Where(items => items.Count() == 2)
                .ToDictionary(pair => Uri.UnescapeDataString(pairs[0]),
                pair => Uri.UnescapeDataString(pair[1]));
        }
    }
}