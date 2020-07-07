using System.ComponentModel;
using Wallet.ViewModels;
using Xamarin.Forms;

namespace Wallet.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}