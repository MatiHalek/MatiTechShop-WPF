using MatiTechShop.Classes;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MatiTechShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainPage : Page
    {
        public ObservableCollection<Category> ?CategoryList { get; set; }
        public MainPage()
        {
            InitializeComponent();           
            DataContext = this;
            CategoryList = new ObservableCollection<Category>(DataAccess.ReadCategory());
            if (DataAccess.UserData != null && int.Parse(DataAccess.UserData["Position"] ?? "") > 1)
            {
                btnAdd.Visibility = Visibility.Visible;
                editMode.Visibility = Visibility.Visible;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(((Button)sender).Tag.ToString() ?? "");
            if(checkbox.IsChecked == true)
            {               
                CategoryDetails? cd = new(id);
                cd.ShowDialog();
                CategoryList = new ObservableCollection<Category>(DataAccess.ReadCategory());
                test.ItemsSource = CategoryList;
            }
            else
                MainWindow.Frame.NavigationService.Navigate(new CategoryProducts(id));
        }
        //https://digitaltrends.com/wp-content/uploads/2021/08/dell-xps-15-oled-2021-laptop.jpg?fit=720%2C720
        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            ContextMenu cm = (ContextMenu)mi.CommandParameter;
            Button b = (Button)cm.PlacementTarget;
            Category c = new();
            int ?id = int.Parse(b.Tag.ToString() ?? "");
            if(CategoryList != null)
            {
                foreach (var item in CategoryList)
                {
                    if(item.Id == id)
                    {
                        c = item;
                        break;
                    }
                }
                MessageBoxResult r = MessageBox.Show("Czy NA PEWNO chcesz USUNĄĆ kategorię \"" + c.Name + "\" ?\nTa operacja jest nieodwracalna!", "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(r == MessageBoxResult.Yes)
                {
                    DataAccess.DeleteCategory(c);              
                    CategoryList = new ObservableCollection<Category>(DataAccess.ReadCategory());
                    test.ItemsSource = CategoryList;
                }
            }           
        }
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (checkbox.IsChecked != true)
                checkbox.IsChecked = true;
            else
                checkbox.IsChecked = false; 
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            CategoryDetails cd = new();
            cd.ShowDialog();
            CategoryList = new ObservableCollection<Category>(DataAccess.ReadCategory());
            test.ItemsSource = CategoryList;
        }


        private void UniformGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (((UniformGrid)sender).ActualWidth < 400)
                ((UniformGrid)sender).Columns = 2;
            else if(((UniformGrid)sender).ActualWidth < 600)
                ((UniformGrid)sender).Columns = 3;
            else
                ((UniformGrid)sender).Columns = 4;
        }

        private void DeleteCategory_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataAccess.UserData != null && int.Parse(DataAccess.UserData["Position"] ?? "") > 1)
            {
                MenuItem mi = (MenuItem)sender;
                mi.Parent.SetValue(VisibilityProperty, Visibility.Visible);
            }           
        }
    }
}
