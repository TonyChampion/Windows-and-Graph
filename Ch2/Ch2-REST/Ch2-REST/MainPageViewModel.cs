using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Ch2_REST
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IPublicClientApplication publicClientApp;
        private static readonly HttpClient httpClient = new HttpClient();

        private AuthenticationResult authenticationResult;

        public MainPageViewModel()
        {
            string URI = string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());

            publicClientApp = PublicClientApplicationBuilder.Create("[Client ID]")
                                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                                .WithRedirectUri(URI)
                                .Build();
        }

        public async Task Authenticate()
        {
            authenticationResult = await publicClientApp.AcquireTokenInteractive(new string[] { "user.read" })
                                            .ExecuteAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", authenticationResult.AccessToken));
            await LoadMe();
        }

        public async Task LoadMe()
        {
            // Get user info
            var result = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me/");
            var json = await result.Content.ReadAsStringAsync();

            var jsonObject = JsonObject.Parse(json);
            ProfileDisplayName = jsonObject.GetNamedString("displayName");
            ProfileEmail = jsonObject.GetNamedString("userPrincipalName");

            // Get photo data
            var photo = await httpClient.GetAsync("https://graph.microsoft.com/beta/me/photo/$value");
            Stream photoStream = await photo.Content.ReadAsStreamAsync();

            using(var ras = photoStream.AsRandomAccessStream())
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(ras);
                ProfileImageSource = bitmapImage;

            }
        }

        private ImageSource _profileImageSource;
        public ImageSource ProfileImageSource
        {
            get { return _profileImageSource; }
            set
            {
                _profileImageSource = value;
                NotifyPropertyChanged(nameof(ProfileImageSource));
            }
        }

        private string _displayName;
        public string ProfileDisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                NotifyPropertyChanged(nameof(ProfileDisplayName));
            }
        }


        private string _emailAddress;
        public string ProfileEmail
        {
            get { return _emailAddress; }
            set
            {
                _emailAddress = value;
                NotifyPropertyChanged(nameof(ProfileEmail));
            }
        }

        protected void NotifyPropertyChanged(string propertyName) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
