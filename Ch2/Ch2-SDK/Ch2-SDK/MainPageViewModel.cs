using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Security.Authentication.Web;
using Microsoft.Graph.Auth;
using Microsoft.Graph;

namespace Ch2_SDK
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IPublicClientApplication publicClientApp;
        private GraphServiceClient graphClient;

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

            InteractiveAuthenticationProvider authProvider = new InteractiveAuthenticationProvider(publicClientApp, new string[] { "user.read" });

            graphClient = new GraphServiceClient(authProvider);

            await LoadMe();
        }

        public async Task LoadMe()
        {
            // Get user info
            User user = await graphClient.Me
                                .Request()
                                .GetAsync();

            ProfileDisplayName = user.DisplayName;
            ProfileEmail = user.UserPrincipalName;

            // Get photo data
            graphClient.BaseUrl = "https://graph.microsoft.com/beta";
            Stream photoStream = await graphClient.Me.Photo.Content
                   .Request()
                   .GetAsync();

            using (var ras = photoStream.AsRandomAccessStream())
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
