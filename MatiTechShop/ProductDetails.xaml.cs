using MatiTechShop.Classes;
using System;
using System.Collections.Generic;
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
    /// Logika interakcji dla klasy ProductDetails.xaml
    /// </summary>
    public partial class ProductDetails : Page
    {
        public Product ?Product { get; set; }
        public ProductDetails(int productId)
        {
            InitializeComponent();
            Product = DataAccess.ReadProduct(productId).Item1;
            id.Text = "Kod produktu: " + Product.Id.ToString().PadLeft(6, '0');
            name.Text = Product.Name;   
            description.Text = Product.Description; 
            if(Product.BestFeature != null)
            {
                bestFeature.Content = Product.BestFeature;
                bestFeatureBorder.Visibility = Visibility.Visible;
            }
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            decimal price = Product.Price;
            if(Product.Promotion != null)
            {
                oldPrice.Text = Product.Price.ToString("n", nfi).Replace(".", ",");
                oldPrice.Visibility = Visibility.Visible;
                price = (decimal)Product.Promotion;
            }
            PriceZl.Text = Math.Truncate(price).ToString("n", nfi)[..^3];
            PriceGr.Text = ((int)((price - Math.Truncate(price)) * 100)).ToString().PadLeft(2, '0');           
            if(Product.Quantity > 0)
                quantity.Text = "Dostępne sztuki: " + Product.Quantity.ToString();
            else
            {
                var button = (from item in pricePanel.Children.Cast<FrameworkElement>() where "addToCart".Equals(item.Tag) select item).First();
                pricePanel.Children.Remove(button);
                quantity.Text = "Produkt niedostępny";
                BrushConverter bc = new();
                quantity.Foreground = (Brush?)bc.ConvertFromString("Orangered");
                quantity.HorizontalAlignment = HorizontalAlignment.Center;
                guarantee.HorizontalAlignment = HorizontalAlignment.Center;
                quantity.FontSize = 15;              
            }
            if(Product.Guarantee != null)
            {
                guarantee.Text = "Gwarancja: " + Product.Guarantee.ToString() + " miesięcy";
                guarantee.Visibility = Visibility.Visible;
            }
            Dictionary<string, string> parameters = DataAccess.ReadProduct(productId).Item2;
            if (parameters.Count == 0)
                productProperties.Visibility = Visibility.Collapsed;
            else
            {
                int i = 1;
                var keys = parameters.Keys; 
                foreach( var key in keys )
                {
                    BrushConverter brushConverter = new();
                    TableRow tr = new();
                    if (i % 2 == 1)
                        tr.Background = (Brush?)brushConverter.ConvertFromString("DarkGray");
                    else
                        tr.Background = (Brush?)brushConverter.ConvertFromString("White");
                    TableCell td = new()
                    {
                        TextAlignment = TextAlignment.Right
                    };
                    Paragraph paragraph = new(new Run(key));
                    td.Padding = new Thickness(5);
                    td.Blocks.Add(paragraph);
                    TableCell td2 = new()
                    {
                        TextAlignment = TextAlignment.Left
                    };
                    paragraph = new(new Run(parameters[key]));
                    td2.Padding = new Thickness(5);
                    td2.Blocks.Add(paragraph);
                    tr.Cells.Add(td);
                    tr.Cells.Add(td2);
                    rowGroup.Rows.Add(tr);
                    i++;
                }
            }            
            DirectoryInfo d = new("..\\..\\..\\Images\\ProductImages\\" + Product.Id.ToString() + "\\default\\");
            foreach(var file in d.GetFiles("*.*"))
            {               
                FileStream stream = new(file.FullName, FileMode.Open);
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                Image image = new()
                {
                    Source = bitmapImage,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
                imagePanel.Children.Add(image);
                stream.Close();
                stream.Dispose();
            }
            if (DataAccess.UserData != null && int.Parse(DataAccess.UserData["Position"] ?? "") > 1)
                statusBar.Visibility = Visibility.Visible;  
        }
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            int cartProductCount = 0;
            Dictionary<int, int> cart = new();
            if (File.ReadAllBytes("..\\..\\..\\cart.txt").Length > 0)
                cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(File.ReadAllText("..\\..\\..\\cart.txt")) ?? new();
                if(Product is not null)
                {
                    if (cart.ContainsKey(Product.Id))
                        cartProductCount = cart[Product.Id];
                    if(cartProductCount + 1 <= Product.Quantity)
                    {
                        MessageBox.Show("Produkt \"" + Product.Name + "\" został pomyślnie dodany do koszyka!\nLiczba tych produktów znajdujących się obecnie w koszyku: " + (cartProductCount + 1).ToString() + ".", "Dodano do koszyka", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (cartProductCount == 0)
                            cart.Add(Product.Id, 1);
                        else
                            cart[Product.Id]++;
                        File.WriteAllText("..\\..\\..\\cart.txt", System.Text.Json.JsonSerializer.Serialize(cart));
                    }
                    else
                        MessageBox.Show("Produkt \"" + Product.Name + "\" nie mógł zostać dodany do koszyka, ponieważ w koszyku znajduje się jego maksymalna ilość (" + Product.Quantity + ").", "Błąd dodawania do koszyka", MessageBoxButton.OK, MessageBoxImage.Error);
                }                          
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if(Product is not null)
            {
                MessageBoxResult result = MessageBox.Show("Czy NA PEWNO chcesz USUNĄĆ produkt \"" + Product.Name + "\" wraz ze wszystkimi odniesieniami do tego produktu?\nTa operacja jest nieodwracalna!", "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataAccess.DeleteProduct(Product.Id);
                    DirectoryInfo d = new("..\\..\\..\\Images\\ProductImages\\" + Product.Id.ToString() + "\\");
                    d.Delete(true);
                    ((MainWindow)Application.Current.MainWindow).mainFrame.NavigationService.Navigate(new MainPage());
                }                    
            }           
        }
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if(Product is not null)
            {
                AddOrEditProduct addOrEditProduct = new(Product);
                addOrEditProduct.ShowDialog();
                ((MainWindow)Application.Current.MainWindow).mainFrame.NavigationService.Navigate(new ProductDetails(Product.Id));
            }
            
        }
    }
}
