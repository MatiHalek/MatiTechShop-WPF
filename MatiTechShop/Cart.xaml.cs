using ABI.System;
using MatiTechShop.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Linq;

namespace MatiTechShop
{
    /// <summary>
    /// Logika interakcji dla klasy Cart.xaml
    /// </summary>
    public partial class Cart : Page
    {
        public string ButtonTitle { get; set; } = "Złóż zamówienie";
        public Dictionary<int, int>? CartContent { get; set; } = new();
        public ObservableCollection<Product>? CartProducts { get; set; } = new();
        public Cart()
        {
            InitializeComponent();
            DataContext = this;
            if (File.ReadAllBytes("..\\..\\..\\cart.txt").Length > 0)
            {
                CartContent = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(File.ReadAllText("..\\..\\..\\cart.txt")) ?? new();
                var keys = CartContent.Keys;
                foreach (var key in keys)
                    CartProducts.Add(DataAccess.ReadProduct(key).Item1);
            }
            cartList.ItemsSource = CartProducts; 
            if(CartContent.Count == 0)
            {
                customerData.Opacity = 0.4;
                customerData.IsEnabled = false;
                error.Visibility = Visibility.Visible;
                productSection.Visibility = Visibility.Collapsed;   
            }
            if(DataAccess.UserData != null)
            {
                firstNameEntry.Text = DataAccess.UserData["First name"];
                surnameEntry.Text = DataAccess.UserData["Surname"];
                postcodeEntry.Text = DataAccess.UserData["Postcode"];
                cityEntry.Text = DataAccess.UserData["City"];
                streetEntry.Text = DataAccess.UserData["Street"];
                houseNumberEntry.Text = DataAccess.UserData["House number"];
                emailEntry.Text = DataAccess.UserData["Email"];
                phoneEntry.Text = DataAccess.UserData["Phone number"];
            }
        }
        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image i = (Image)sender;
            Grid g = (Grid)i.Parent;
            Border b = (Border)g.Parent;
            DirectoryInfo d = new("..\\..\\..\\Images\\ProductImages\\" + b.Tag.ToString() + "\\default\\");
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
            ((Image)sender).Source = bitmapImage;
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock txtBlock = (TextBlock)sender;
            StackPanel sp = (StackPanel)txtBlock.Parent;
            Grid g = (Grid)sp.Parent;
            Border b = (Border)g.Parent;
            if(CartContent is not null)
            {
                decimal price = 0;
                int amount = CartContent[int.Parse(b.Tag.ToString() ?? "")];
                if(CartProducts is not null)
                {
                    Product product = CartProducts.Where(x => x.Id == int.Parse(b.Tag.ToString() ?? "")).ToList()[0];
                    if (product.Promotion is not null)
                        price = (decimal)product.Promotion;
                    else
                        price = product.Price;
                }                   
                if(txtBlock.Tag.ToString() == "FullPrice")
                    txtBlock.Text = DataAccess.FormatNumber(price * amount) + " zł";
                else
                    txtBlock.Text = amount + " x " + DataAccess.FormatNumber(price) + " zł";
            }            
        }
        private void TextBlockId_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock txtBlock = (TextBlock)sender;
            StackPanel sp = (StackPanel)txtBlock.Parent;
            Grid g = (Grid)sp.Parent;
            Border b = (Border)g.Parent;
            txtBlock.Text = "KOD PRODUKTU: " + (b.Tag.ToString() ?? "").PadLeft(6, '0');
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            StackPanel sp = (StackPanel)txtBox.Parent;
            Grid g = (Grid)sp.Parent;
            Border b = (Border)g.Parent;
            if(CartContent is not null)
            {
                txtBox.Text = CartContent[int.Parse(b.Tag.ToString() ?? "")].ToString();
                txtBox.TextChanged += TextBox_TextChanged;
            }
                
        }
        private static bool ValidateRegex(TextBox txtBox, string regex, bool IgnoreWhiteSpace = false)
        {
            BrushConverter b = new();
            string text = txtBox.Text;
            if(IgnoreWhiteSpace)
            {
                Regex r = WhiteSpaceRegex();
                text = r.Replace(text, "");
            }
            if (Regex.IsMatch(text, regex) == true)
            {
                txtBox.BorderBrush = (Brush?)b.ConvertFromString("Green");
                return true;
            }               
            else
            {
                txtBox.BorderBrush = (Brush?)b.ConvertFromString("Red");
                return false;
            }
        }
        private static bool ValidateNull(TextBox txtBox)
        {
            BrushConverter b = new();
            if (string.IsNullOrWhiteSpace(txtBox.Text))
            {
                txtBox.BorderBrush = (Brush?)b.ConvertFromString("Red");
                return false;
            }                
            else
            {
                txtBox.BorderBrush = (Brush?)b.ConvertFromString("Green");
                return true;
            }
        }
        private static bool ValidateEmail(TextBox txtBox)
        {
            BrushConverter b = new();
            var emailChecker = new EmailAddressAttribute();
            if (emailChecker.IsValid(txtBox.Text))
            {
                txtBox.BorderBrush = (Brush?)b.ConvertFromString("Green");
                return true;
            }
            else
            {
                txtBox.BorderBrush = (Brush?)b.ConvertFromString("Red");
                return false;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool success = true;
            if (!ValidateRegex(firstNameEntry, @"^[\p{L}]+$"))
                success = false;
            if (!ValidateRegex(surnameEntry, @"^[\p{L}]+$"))
                success = false;
            if (!ValidateRegex(postcodeEntry, @"^[0-9]{2}-[0-9]{3}$"))
                success = false;
            if (!ValidateNull(cityEntry))
                success = false;
            if (!ValidateNull(streetEntry))
                success = false;
            if (!ValidateNull(houseNumberEntry))
                success = false;
            if (!ValidateRegex(phoneEntry, @"^[0-9]{9}$", true))
                success = false;
            if (!ValidateEmail(emailEntry))
                success = false;
            if (delivery.SelectedIndex == 0 || payment.SelectedIndex == 0)
                success = false;
            if (success == true)
            {
                
                if(DataAccess.UserData is not null && CartProducts is not null && CartContent is not null)
                {
                    string?[] productNames = new string?[CartProducts.Count];
                    int[] quantities = new int[CartProducts.Count];
                    decimal?[] prices = new decimal?[CartProducts.Count];
                    for(int i=0; i<CartProducts.Count; i++)
                    {
                        productNames[i] = CartProducts[i].Name;
                        quantities[i] = CartContent[CartProducts[i].Id];
                        prices[i] = (CartProducts[i].Promotion == null)?CartProducts[i].Price:CartProducts[i].Promotion;
                        DataAccess.ChangeQuantity(CartProducts[i].Id, CartProducts[i].Quantity - quantities[i]);
                    }
                    File.WriteAllText("..\\..\\..\\cart.txt", string.Empty);
                    DataAccess.CreateOrder(int.Parse(DataAccess.UserData["Id"] ?? ""), DateTime.Now, delivery.Text ?? "", payment.Text ?? "", "Zarejestrowano dane przesyłki", productNames, quantities, prices);
                    MessageBox.Show("Dziękujemy za złożenie zamówienia!", "Pomyślne złożenie zamówienia", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainWindow.Frame.Navigated += MainWindow.Frame_Navigated;
                    MainWindow.Frame.NavigationService.Navigate(new MainPage());

                }
                else
                {
                    Login l = new();
                    l.ShowDialog();
                }
            }
            else
                MessageBox.Show("Proszę poprawnie uzupełnić wszystkie pola oraz wybrać opcję dostawy i płatności.", "Błąd składania zamówienia", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            StackPanel sp = (StackPanel)txtBox.Parent;
            Grid g = (Grid)sp.Parent;
            Border b = (Border)g.Parent;
            int newValue;
            if (int.TryParse(txtBox.Text, out int result) == false || result <= 0)
                newValue = 1;
            else if (result > int.Parse(txtBox.Tag.ToString() ?? ""))
                newValue = int.Parse(txtBox.Tag.ToString() ?? "");
            else
                newValue = int.Parse(txtBox.Text);
            if (CartContent is not null)
            {
                CartContent[int.Parse(b.Tag.ToString() ?? "")] = newValue;
                Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(File.ReadAllText("..\\..\\..\\cart.txt")) ?? new();
                cart[int.Parse(b.Tag.ToString() ?? "")] = newValue;
                File.WriteAllText("..\\..\\..\\cart.txt", System.Text.Json.JsonSerializer.Serialize(cart));
                if (CartProducts is not null)
                {
                    CartProducts = new ObservableCollection<Product>(CartProducts);
                    cartList.ItemsSource = CartProducts;
                }
            }
        }

        private void CartDecrement_Click(object sender, RoutedEventArgs e)
        {
            TextBox counter = (TextBox)((StackPanel)((Button)sender).Parent).Children[1];
            counter.Text = (int.Parse(counter.Text) - 1).ToString();
        }

        private void CartIncrement_Click(object sender, RoutedEventArgs e)
        {
            TextBox counter = (TextBox)((StackPanel)((Button)sender).Parent).Children[1];
            counter.Text = (int.Parse(counter.Text) + 1).ToString();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Grid g = (Grid)btn.Parent;
            Border b = (Border)g.Parent;
            if (CartContent is not null)
            {
                CartContent.Remove(int.Parse(b.Tag.ToString() ?? ""));
                Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(File.ReadAllText("..\\..\\..\\cart.txt")) ?? new();
                cart.Remove(int.Parse(b.Tag.ToString() ?? ""));
                File.WriteAllText("..\\..\\..\\cart.txt", System.Text.Json.JsonSerializer.Serialize(cart));
                if (CartProducts is not null)
                {
                    foreach (Product p in CartProducts)
                    {
                        if (p.Id == int.Parse(b.Tag.ToString() ?? ""))
                        {
                            CartProducts.Remove(p);
                            break;
                        }
                            
                    }
                    CartProducts = new ObservableCollection<Product>(CartProducts);
                    cartList.ItemsSource = CartProducts;
                    if (CartProducts.Count == 0)
                    {
                        customerData.Opacity = 0.4;
                        customerData.IsEnabled = false;
                        error.Visibility = Visibility.Visible;
                        productSection.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        [GeneratedRegex("\\s+")]
        private static partial Regex WhiteSpaceRegex();
    }
}
