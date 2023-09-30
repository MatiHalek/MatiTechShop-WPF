using MatiTechShop.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static Frame Frame { get; set; } = new Frame();
        public MainWindow()
        {
            InitializeComponent();
            Frame = mainFrame;
        }
        public static void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            Frame.Navigated -= Frame_Navigated;
            while (Frame.CanGoBack)
            {
                try
                {
                    Frame.RemoveBackEntry();
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ReleaseNotes == true)
            {
                await Task.Delay(10000);
                MessageBoxResult result = MessageBox.Show(DataAccess.ReleaseNotes, "MatiTechShop - notki wydania", MessageBoxButton.OK, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)
                {
                    Properties.Settings.Default.ReleaseNotes = false;
                    Properties.Settings.Default.Save();
                }
                    
            }          
        }
    }
}
