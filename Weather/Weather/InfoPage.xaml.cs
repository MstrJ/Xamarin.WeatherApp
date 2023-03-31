using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
            OnClickLbGithub();
        }

        private void OnClickLbGithub()
        {
            lbGithub.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                    await Launcher.OpenAsync("https://github.com/MstrJ");
                })
            });
        }

        private async void ImageButton_Clicked(object sender, EventArgs e)
        {
           await Navigation.PopModalAsync();
        }
    }
}