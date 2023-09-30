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

namespace MatiTechShop
{
    /// <summary>
    /// Logika interakcji dla klasy Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
            if (DataAccess.UserData != null)
            {
                btnRegistration.Visibility = Visibility.Collapsed;
                btnLogin.Visibility = Visibility.Collapsed;
                btnLogout.Visibility = Visibility.Visible;
                btnOrdermanage.Visibility = Visibility.Visible; 
                account.Text = DataAccess.UserData["Username"];
                if (DataAccess.UserData["Position"] == "3")
                {
                    verified.Source = new BitmapImage(new Uri("Images/verifiedOwner.png", UriKind.Relative));
                    verifiedTooltip.Content = "To konto ma status zweryfikowanego, ponieważ należy do właściciela serwisu";
                }                   
                if (int.Parse(DataAccess.UserData["Position"] ?? "") > 1)
                    verified.Visibility = Visibility.Visible;               
            }
            Loaded += NavigationControls;            
        }
        private void NavigationControls(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Frame.NavigationService.CanGoBack)
                goBack.Visibility = Visibility.Visible;
            if(MainWindow.Frame.NavigationService.CanGoForward)
                goForward.Visibility = Visibility.Visible;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("MatiTechShop (wersja desktopowa)\nWersja 1.1 (19.06.2023 r.)\n\nOgromny wybór, wspaniałe produkty i niskie ceny w MatiTechShop! Wiele okazji dla każdego, kto chce kupić urządzenie. Prawdziwa uczta dla wszystkich fanów technologii. Zapraszamy!\n\n©2023 MatiTechShop Corporation. Wszelkie prawa zastreżone.", "Informacje o programie", MessageBoxButton.OK, MessageBoxImage.Information); ;
        }
        private void MenuItem_Click_Registration(object sender, RoutedEventArgs e)
        {
            Registration r = new();
            r.ShowDialog();
            if(DataAccess.UserData != null)
            {
                MainWindow.Frame.Navigated += MainWindow.Frame_Navigated;
                MainWindow.Frame.NavigationService.Navigate(new MainPage());
            }               
        }
        private void MenuItem_Click_Logout(object sender, RoutedEventArgs e)
        {
            DataAccess.UserData = null;
            MainWindow.Frame.Navigated += MainWindow.Frame_Navigated;
            MainWindow.Frame.NavigationService.Navigate(new MainPage());
        }
        private void MenuItem_Click_Login(object sender, RoutedEventArgs e)
        {
            Login l = new();
            l.ShowDialog();
            if (DataAccess.UserData != null)
            {
                MainWindow.Frame.Navigated += MainWindow.Frame_Navigated;
                MainWindow.Frame.NavigationService.Navigate(new MainPage());
            }
                                                        
        }
        private void GoBack_Click(object sender, RoutedEventArgs e) => MainWindow.Frame.NavigationService.GoBack();

        private void GoForward_Click(object sender, RoutedEventArgs e) => MainWindow.Frame.NavigationService.GoForward();

        private void Cart_Click(object sender, RoutedEventArgs e) => MainWindow.Frame.NavigationService.Navigate(new Cart());

        private void MenuItem_Click_Default(object sender, RoutedEventArgs e) => MainWindow.Frame.NavigationService.Navigate(new MainPage());

        private void MenuItem_Click_Shutdown(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MenuItem_Click_OrderManage(object sender, RoutedEventArgs e) => MainWindow.Frame.NavigationService.Navigate(new OrderManage());

        private void Menu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Application.Current.MainWindow.DragMove();
        }
        private void ReleaseNotes_Click(object sender, RoutedEventArgs e) => MessageBox.Show(DataAccess.ReleaseNotes, "MatiTechShop - notki wydania", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
