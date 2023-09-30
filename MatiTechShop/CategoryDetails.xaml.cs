using MatiTechShop.Classes;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MatiTechShop
{
    /// <summary>
    /// Logika interakcji dla klasy CategoryDetails.xaml
    /// </summary>
    public partial class CategoryDetails : Window
    {
        public Category ?Category { get; set; }

        public CategoryDetails()
        {
            InitializeComponent();
        }
        public CategoryDetails(int id)
        {
            InitializeComponent();
            List<Category> tmp = DataAccess.ReadCategory();
            foreach (Category c in tmp)
            {
                if(c.Id == id)
                {
                    Category = c;
                    break;
                }
            }
            if(Category is not null)
            {
                categoryBox.Text = Category.Name;
                imageBox.Text = Category.Image;
            }           
            window.Title = "Edytuj kategorię - MatiTechShop";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(categoryBox.Text) && CategoryRegex().IsMatch(categoryBox.Text) && !string.IsNullOrWhiteSpace(imageBox.Text))
            {
                if(Category != null)
                {
                    Category.Name = categoryBox.Text;
                    Category.Image = imageBox.Text;
                    DataAccess.UpdateCategory(Category);
                }
                else
                {
                    DataAccess.CreateCategory(new Category
                    {
                        Name = categoryBox.Text,
                        Image = imageBox.Text   
                    });
                }
                Close();
            }
            else
                MessageBox.Show("Nie można zatwierdzić zmian. Upewnij się, czy uzupełniono poprawnie wszystkie pola i spróbuj ponownie", "Niepoprawne dane", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        [GeneratedRegex("^[\\p{L}\\s]+$")]
        private static partial Regex CategoryRegex();
    }
}
