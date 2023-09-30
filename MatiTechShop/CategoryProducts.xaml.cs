using MatiTechShop.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    /// Logika interakcji dla klasy CategoryProducts.xaml
    /// </summary>
    
    public partial class CategoryProducts : Page
    {
        public Category? Category { get; set; }
        public ObservableCollection<Product>? ProductList { get; set; }
        public CategoryProducts(int categoryId)
        {
            InitializeComponent();
            DataContext = this;
            var tmp = DataAccess.ReadCategory().Where(category => category.Id == categoryId);
            foreach (Category category in tmp)
                Category = category;
            if(Category is not null)
            {
                Run categoryTitle = new()
                {
                    Text = Category.Name
                };
                Run categoryNumberOfProducts = new()
                {
                    Text = " (Liczba wyników: " + Category.Count + ")",
                    FontSize = 15
                };
                categoryName.Inlines.Add(categoryTitle);
                categoryName.Inlines.Add(categoryNumberOfProducts);
                if (Category.Count == 0)
                    error.Visibility = Visibility.Visible;
            }              
            ProductList = new ObservableCollection<Product>(DataAccess.ReadProducts(categoryId));
            if (DataAccess.UserData != null && int.Parse(DataAccess.UserData["Position"] ?? "") > 1)
                btnAdd.Visibility = Visibility.Visible;           
        }
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;   
            WrapPanel wp = (WrapPanel)tb.Parent;
            StackPanel sp = (StackPanel)wp.Parent;
            Border b = (Border)sp.Parent;
            MainWindow.Frame.Navigate(new ProductDetails(int.Parse(b.Tag.ToString() ?? "")));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(Category is not null)
            {
                AddOrEditProduct addOrEdit = new(Category.Id);
                addOrEdit.ShowDialog();
                ProductList = new ObservableCollection<Product>(DataAccess.ReadProducts(Category.Id));
                productList.ItemsSource = ProductList;
                Run categoryTitle = new()
                {
                    Text = Category.Name
                };
                Run categoryNumberOfProducts = new()
                {
                    Text = " (Liczba wyników: " + Category.Count + ")",
                    FontSize = 15
                };
                categoryName.Inlines.Clear();
                categoryName.Inlines.Add(categoryTitle);
                categoryName.Inlines.Add(categoryNumberOfProducts);
            }            
        }

        private void DefaultImage_Loaded(object sender, RoutedEventArgs e)
        {
            DirectoryInfo d = new("..\\..\\..\\Images\\ProductImages\\" + ((Border)sender).Tag.ToString() + "\\default\\");
            BitmapImage bitmapImage;
            using (var stream = new FileStream(d.GetFiles("*.*")[0].FullName, FileMode.Open))
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            ImageBrush ib = new()
            {
                ImageSource = bitmapImage,
                Stretch = Stretch.UniformToFill
            };
            RenderOptions.SetBitmapScalingMode(ib, BitmapScalingMode.HighQuality);
            ((Border)sender).Background = ib;
        }
        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            ((TextBlock)sender).Text = "(" + (((TextBlock)sender).Tag.ToString() ?? "").PadLeft(6, '0') + ")";
        }

        private void TextBlock_Loaded_1(object sender, RoutedEventArgs e)
        {
            ((TextBlock)sender).Text = DataAccess.FormatNumber(decimal.Parse(((TextBlock)sender).Tag.ToString() ?? ""));
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dic = DataAccess.ReadProduct(int.Parse(((StackPanel)sender).Tag.ToString() ?? "")).Item2;
            var keys = dic.Keys;
            BrushConverter b = new();
            ((StackPanel)sender).Children.Clear();
            foreach (var key in keys)
            {
  
                WrapPanel wp = new()
                {
                    Orientation = Orientation.Horizontal
                };
                Image i = new()
                {
                    Source = new BitmapImage(new Uri("Images/listIcon3D.png", UriKind.Relative)),
                    Margin = new Thickness(3, 3, 6, 0),
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Top
                };
                RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.HighQuality);
                TextBlock tb = new()
                {
                    Text = key.ToUpper() + ": ",
                    Foreground = (Brush?)b.ConvertFromString("Blue"),  
                };
                TextBlock tb2 = new()
                {
                    Text = dic[key],
                };
                wp.Children.Add(i);
                wp.Children.Add(tb);
                wp.Children.Add(tb2);
                ((StackPanel)sender).Children.Add(wp);
            }
        }
        private void BestFeature_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            if (tb.Text == string.Empty)
            {
                WrapPanel wp = (WrapPanel)tb.Parent;
                Label l = (Label)wp.Parent;
                l.Visibility = Visibility.Collapsed;
            }
        }

        private void DefaultImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MainWindow.Frame.Navigate(new ProductDetails(int.Parse(((Border)sender).Tag.ToString() ?? "")));
        private void Price_Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid g = (Grid)sender;
            StackPanel sp = (StackPanel)g.Parent;
            TextBlock data = (TextBlock)sp.Children[0];
            if(data.Text.IndexOf('|') > -1)
            {
                decimal price = decimal.Parse(data.Text[..data.Text.IndexOf('|')], CultureInfo.InvariantCulture);
                if (!data.Text.EndsWith('|'))
                {
                    price = decimal.Parse(data.Text[(data.Text.IndexOf('|') + 1)..], CultureInfo.InvariantCulture);
                    data.Text = DataAccess.FormatNumber(decimal.Parse(data.Text[..data.Text.IndexOf('|')], CultureInfo.InvariantCulture));
                    data.Visibility = Visibility.Visible;
                }
                TextBlock priceZl = new()
                {
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5, 0, 5, 0),
                    FontSize = 50,
                    Text = DataAccess.FormatNumber(price)[..^3]
                };
                Grid.SetRowSpan(priceZl, 2);
                TextBlock priceGr = new()
                {
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    FontSize = 20,
                    Text = ((int)((price - Math.Truncate(price)) * 100)).ToString().PadLeft(2, '0')
                };
                Grid.SetColumn(priceGr, 1);
                g.Children.Add(priceZl);
                g.Children.Add(priceGr);
            }            
        }
    }
}
