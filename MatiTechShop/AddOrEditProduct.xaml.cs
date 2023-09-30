using MatiTechShop.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MatiTechShop
{
    /// <summary>
    /// Logika interakcji dla klasy AddOrEditProduct.xaml
    /// </summary>
    public partial class AddOrEditProduct : Window
    {
        public Product ?Product { get; set; }
        public ObservableCollection<Parameter> ?Parameters { get; set; } = new ObservableCollection<Parameter>();   
        public AddOrEditProduct(int categoryId)
        {
            InitializeComponent();
            dataGrid.DataContext = Parameters;
            var categories = DataAccess.ReadCategory();
            foreach(Category c in categories)
            {
                ListBoxItem listBoxItem = new()
                {
                    Content = c.Name,
                    Tag = c.Id.ToString()
                };
                if (c.Id == categoryId)
                    listBoxItem.IsSelected = true;
                category.Items.Add(listBoxItem);
            }
        }
        public AddOrEditProduct(Product product)
        {
            InitializeComponent();
            page.Title = "Edytuj produkt - MatiTechShop";
            Product = product;
            name.Text = Product.Name;   
            description.Text = Product.Description;
            price.Text = Product.Price.ToString().Replace(".", ",");
            bestFeature.Text = Product.BestFeature;
            quantity.Text = Product.Quantity.ToString();
            guarantee.Text = Product.Guarantee.ToString();
            if(Product.Promotion != null)
            {
                checkbox.IsChecked = true;
                promotion.Text = (Product.Promotion.ToString() ?? "").Replace(".", ",");
            }
            //DirectoryInfo d = new("..\\..\\..\\Images\\ProductImages\\" + Product.Id.ToString() + "\\default\\");
            //mainImage.Text = d.GetFiles("*.*")[0].FullName;           
            var categories = DataAccess.ReadCategory();
            List<int> selectedCategories = DataAccess.ReadCategories(Product.Id);
            foreach (Category c in categories)
            {
                ListBoxItem listBoxItem = new()
                {
                    Content = c.Name,
                    Tag = c.Id.ToString()
                };
                if (selectedCategories.Contains(c.Id))
                    listBoxItem.IsSelected = true;
                category.Items.Add(listBoxItem);
            }
            Dictionary<string, string> param = DataAccess.ReadProduct(Product.Id).Item2;
            var keys = param.Keys;
            dataGrid.DataContext = Parameters;
            foreach(string key in keys)
                Parameters.Add(new Parameter(key, param[key]));
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {           
            bool success = true;
            bool dataGridSuccess = true;
            List<string?> parameterNames = new();
            if(Parameters is not null)
            {
                foreach(Parameter param in Parameters)
                {
                    if(string.IsNullOrWhiteSpace(param.Name) || string.IsNullOrWhiteSpace(param.Value))
                    {
                        dataGridSuccess = false;
                        break;
                    }
                    if(parameterNames.Contains(param.Name))
                    {
                        dataGridSuccess = false;
                        break;
                    }
                    else
                        parameterNames.Add(param.Name);
                }
            }           
            if(dataGridSuccess == false)
            {
                success = false;
                dataGridError.Visibility = Visibility.Visible;
            }
            else
                dataGridError.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                success = false;
                nameError.Visibility = Visibility.Visible;
            }
            else
                nameError.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(description.Text))
            {
                success = false;
                descriptionError.Visibility = Visibility.Visible;
            }
            else
                descriptionError.Visibility = Visibility.Collapsed;
            if (category.SelectedItems.Count == 0)
            {
                success = false;
                categoryError.Visibility = Visibility.Visible;  
            }
            else
                categoryError.Visibility = Visibility.Collapsed;
            //Uri.IsWellFormedUriString("file:///" + mainImage.Text.Replace(@"\", @"/"), UriKind.Absolute) == false
            if (Product is null && mainImage.Text == "")
            {
                success = false;
                imageError.Visibility = Visibility.Visible;
            }
            else
                imageError.Visibility = Visibility.Collapsed;
            if (decimal.TryParse(price.Text, out _) == false || decimal.Parse(price.Text) < 0.01m || decimal.Parse(price.Text) > 999999.99m || price.Text.ToString().Replace(",", ".").TrimEnd('0').SkipWhile(c => c != '.').Skip(1).Count() > 2)
            {
                success = false;
                priceError.Visibility = Visibility.Visible; 
            }
            else
                priceError.Visibility = Visibility.Collapsed;
            if (checkbox.IsChecked == true && (decimal.TryParse(promotion.Text, out _) == false || decimal.Parse(promotion.Text) < 0.01m || decimal.Parse(promotion.Text) > 999999.99m || decimal.Parse(promotion.Text) >= decimal.Parse(price.Text) || promotion.Text.ToString().Replace(",", ".").TrimEnd('0').SkipWhile(c => c != '.').Skip(1).Count() > 2))
            {
                success = false;    
                promotionError.Visibility = Visibility.Visible; 
            }
            else
                promotionError.Visibility = Visibility.Collapsed;
            if (bestFeature.Text != "" && (string.IsNullOrWhiteSpace(bestFeature.Text)))
            {
                success = false;
                bestFeatureError.Visibility = Visibility.Visible;
            }
            else
                bestFeatureError.Visibility = Visibility.Collapsed;
            if(int.TryParse(quantity.Text, out _) == false || int.Parse(quantity.Text) < 0 || int.Parse(quantity.Text) > 999999999)
            {
                success = false;    
                quantityError.Visibility = Visibility.Visible;   
            }
            if (guarantee.Text != "" && (int.TryParse(guarantee.Text, out _) == false || int.Parse(guarantee.Text) < 0 || int.Parse(guarantee.Text) > 999999))
            {
                success = false;
                guaranteeError.Visibility = Visibility.Visible;
            }
            else
                guaranteeError.Visibility = Visibility.Collapsed;
            if (success == false)
                return;
            Dictionary<string, string> addedParameters = new();
            if(Parameters is not null)
            {
                foreach(var item in Parameters)
                    addedParameters.Add(item.Name ?? "", item.Value ?? "");
            }           
            
            List<int> categories = new();
            if(category.SelectedItems is not null)
            {
                foreach(var item in category.SelectedItems)
                    categories.Add(int.Parse(((ListBoxItem)item).Tag.ToString() ?? ""));
            }           
            if(Product is null)
            {
                Product p = new(name.Text, description.Text, decimal.Parse(price.Text), (checkbox.IsChecked == true) ? decimal.Parse(promotion.Text) : null, (string.IsNullOrWhiteSpace(bestFeature.Text)) ? null : bestFeature.Text, int.Parse(quantity.Text), (string.IsNullOrWhiteSpace(guarantee.Text)) ? null : int.Parse(guarantee.Text));
                long? insertedId = DataAccess.CreateProduct(p, categories, addedParameters);
                Directory.CreateDirectory("..\\..\\..\\Images\\ProductImages\\" + insertedId.ToString() + "\\default");
                File.Copy(mainImage.Text, "..\\..\\..\\Images\\ProductImages\\" + insertedId.ToString() + "\\default\\" + System.IO.Path.GetFileName(mainImage.Text), true);   
            }
            else
            {
                Product.Name = name.Text;
                Product.Description = description.Text;
                Product.Price = decimal.Parse(price.Text);
                Product.Promotion = (checkbox.IsChecked == true) ? decimal.Parse(promotion.Text) : null;
                Product.BestFeature = (string.IsNullOrWhiteSpace(bestFeature.Text)) ? null : bestFeature.Text;
                Product.Quantity = int.Parse(quantity.Text);
                Product.Guarantee = (string.IsNullOrWhiteSpace(guarantee.Text)) ? null : int.Parse(guarantee.Text);
                DataAccess.UpdateProduct(Product, categories, addedParameters);
                DirectoryInfo d = new("..\\..\\..\\Images\\ProductImages\\" + Product.Id.ToString() + "\\default\\");
                if(mainImage.Text != string.Empty)
                {
                    File.Delete(d.GetFiles("*.*")[0].FullName);               
                    File.Copy(mainImage.Text, "..\\..\\..\\Images\\ProductImages\\" + Product.Id.ToString() + "\\default\\" + System.IO.Path.GetFileName(mainImage.Text), true);
                }                   
            }                       
            Close();
        }
        private void Button_Click_Cancel(object sender, RoutedEventArgs e) => Close();

        private void FileInput_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new()
            {
                DefaultExt = ".png",
                Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif"
            };
            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value)
                mainImage.Text = dlg.FileName;
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e) => promotion.IsEnabled = true;

        private void Checkbox_Unchecked(object sender, RoutedEventArgs e) => promotion.IsEnabled = false;

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (checkbox.IsChecked == true)
                checkbox.IsChecked = false;
            else
                checkbox.IsChecked = true;
        }
    }
}
