using MatiTechShop.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logika interakcji dla klasy OrderManage.xaml
    /// </summary>
    public partial class OrderManage : Page
    {
        public ObservableCollection<Order>? Orders { get; set; } = new ObservableCollection<Order>(DataAccess.ReadOrder());
        public OrderManage()
        {           
            InitializeComponent();
            dataGrid.DataContext = Orders;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (Order)dataGrid.SelectedItem;
            if(element is not null)
            {
                OrderDetails details = new(element.Id, element.Status ?? "");
                details.ShowDialog();
                Orders = new ObservableCollection<Order>(DataAccess.ReadOrder());
                dataGrid.DataContext = Orders;
            }           
        }
    }
}
