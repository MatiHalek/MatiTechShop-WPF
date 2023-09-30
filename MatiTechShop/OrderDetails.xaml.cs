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
using System.Windows.Shapes;

namespace MatiTechShop
{
    /// <summary>
    /// Logika interakcji dla klasy OrderDetails.xaml
    /// </summary>
    public partial class OrderDetails : Window
    {
        public int Id { get; set; }
        public OrderDetails(int id, string status)
        {
            InitializeComponent();
            Id = id;
            header.Text = ("Zamówienie #" + id).ToUpper();
            List<string> details = DataAccess.ReadOrderDetails(id);
            foreach(string item in details)
            {
                TextBlock textBlock = new()
                {
                    Text = item,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 15
                };
                stackPanel.Children.Add(textBlock);
            }
            foreach(ComboBoxItem item in comboBox.Items)
            {
                if(item.Content.ToString() == status)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataAccess.UpdateOrderStatus(Id, comboBox.Text);
            Close();
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e) => Close();
    }
}
