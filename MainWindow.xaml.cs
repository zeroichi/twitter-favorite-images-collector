using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LinqToTwitter;
using System.Diagnostics;

namespace TwitterFavoriteImagesCollector
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        TwitterContext twitter_context = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// PINコード入力ウィンドウを表示し、ユーザが入力したPINコードを返す。
        /// </summary>
        /// <returns></returns>
        private string GetPinCode()
        {
            string pin_code;
            var pin_input_window = new PinInputWindow();

            pin_input_window.ShowDialog();
            pin_code = pin_input_window.textbox_pincode.Text;
            Debug.WriteLine(pin_code);
            return pin_code;
        }

        private async void button_authenticate_Click(object sender, RoutedEventArgs e)
        {
            var auth = new PinAuthorizer()
            {
                // アプリケーションを識別するための ConsumerKey と ConsumerSecret を指定する必要がある。
                CredentialStore = new InMemoryCredentialStore
                {
                    // ConsumerKey = Environment.GetEnvironmentVariable(OAuthKeys.TwitterConsumerKey),
                    // ConsumerSecret = Environment.GetEnvironmentVariable(OAuthKeys.TwitterConsumerSecret)
                    ConsumerKey = "8P3QaBrEeizpG3jjcTir9g",
                    ConsumerSecret = "qVSl0rgFzxDjpQkvzZptuZ5qaTWb7aGJZguFK1Pk4"
                },
                GoToTwitterAuthorization = pageLink =>
                {
                    Debug.WriteLine(pageLink);
                    Process.Start(pageLink);
                },
                GetPin = () =>
                {
                    string pin_code = "";
                    // ウィンドウの表示は UI スレッド上で行われる必要があるので、Dispatcherを使用する。
                    // 参考: http://blogs.wankuma.com/naka/archive/2009/02/12/168020.aspx
                    var dispatcher = Application.Current.Dispatcher;
                    if (dispatcher.CheckAccess())
                    {
                        pin_code = GetPinCode();
                    }
                    else
                    {
                        pin_code = dispatcher.Invoke<string>(GetPinCode);
                    }
                    return pin_code;
                }
            };
            try
            {
                await auth.AuthorizeAsync();
                twitter_context = new TwitterContext(auth);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void button_get_Click(object sender, RoutedEventArgs e)
        {
            if (twitter_context == null)
            {
                MessageBox.Show("最初に認証してください。");
                return;
            }
            var favorites = await (from fav in twitter_context.Favorites where fav.Type == FavoritesType.Favorites && fav.Count == 200 select fav).ToListAsync();
            var sb = new StringBuilder();
            foreach(var f in favorites)
            {
                sb.AppendLine(f.Text);
            }
            textbox_result.Text = sb.ToString();
        }
    }
}
