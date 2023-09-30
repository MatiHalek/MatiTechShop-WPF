using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Windows.Storage;
using System.IO;
using MatiTechShop.Classes;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace MatiTechShop
{
    public static class DataAccess
    {
        public static string FormatNumber(decimal number)
        {
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return number.ToString("n", nfi).Replace(".", ",");
        }
        public static string ReleaseNotes 
        {
            get
            {
                return "Witamy w wersji 1.1! Oto nowości jakie przygotowaliśmy: \n\nUlepszono wygląd listy produktów z poziomu poszczególnych kategorii.\nDodano opcję przesuwania okna głównego programu za pomocą menu górnego aplikacji.\nPlik z bazą danych sklepu znajduje się teraz w głównym katalogu projektu.\nPoprawiono mechanizmy walidacji podczas tworzenia nowego konta i przy składaniu zamówienia.\nPoprawiono dostępność do niektórych funkcji w aplikacji (np. przyciski niektórych okien dialogowych reagują teraz na klawisze Enter i Escape).\nKoszyk teraz prawidłowo reaguje na usunięcie z niego wszystkich produktów.\nWprowadzono inne drobne ulepszenia uwzględniające m.in. przejrzystszy wygląd oraz responsywność aplikacji.\n\nNAPRAWIONE BŁĘDY:\nNaprawiono błąd, który powodował zawieszanie się aplikacji, kiedy podczas edytowania produktu nie podano ścieżki do nowego zdjęcia (jak i później przy otwieraniu strony tego produktu). [Krytyczny]\nNaprawiono błąd, który poprzez przyciski nawigacji umożliwiał powrót do poprzedniej sesji (np. do poprzednio zalogowanego konta). [Krytyczny]\nNaprawiono błąd, który w pewnych przypadkach powodował błędne wyświetlanie groszy w cenie produktu z poziomu jego szczegółów.\nNaprawiono błąd, który zawsze pokazywał w koszyku cenę produktu bez uwzględniania promocji (podczas zamówienia cena była wyliczana prawidłowo).\nNaprawiono błąd, przez który przy powrocie do strony z listą produktów z danej kategorii przy użyciu nawigacji następowało zwielokrotnienie tych samych parameterów w produktach.\n\nPrzypominamy, że wersja 1.x desktopowej aplikacji MatiTechShop będzie wspierana na systemach Windows 10 i Windows 11 do 29 lutego 2024 r.";
            }
        }
        public static Dictionary<string, string?>? UserData { get; set; } = null;
        public static void InitializeDatabase()
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            List<string> tableCommands = new()
            {
                "CREATE TABLE IF NOT EXISTS category (category_id INTEGER PRIMARY KEY, category NVARCHAR(25), image NVARCHAR(200))",
                "CREATE TABLE IF NOT EXISTS user (user_id INTEGER PRIMARY KEY, username NVARCHAR(15), password NVARCHAR(255), email NVARCHAR(254), birth_date DATE, phone_number NVARCHAR(20) NULL, first_name NVARCHAR(30) NULL, surname NVARCHAR(50) NULL, postcode NVARCHAR(6) NULL, city NVARCHAR(50) NULL, street NVARCHAR(50) NULL, house_number NVARCHAR(10) NULL, is_logged TINYINT(1), user_type TINYINT(4))",
                "CREATE TABLE IF NOT EXISTS product (product_id INTEGER PRIMARY KEY, name NVARCHAR(100), description NVARCHAR(5000), price DECIMAL(8,2), promotion DECIMAL(8,2) NULL, best_feature NVARCHAR(30) NULL, quantity INT(9), guarantee INT(6) NULL)",
                "CREATE TABLE IF NOT EXISTS product_category (product_category_id INTEGER PRIMARY KEY, product_id INTEGER, category_id INTEGER)",
                "CREATE TABLE IF NOT EXISTS parameter (parameter_id INTEGER PRIMARY KEY, parameter NVARCHAR(100))",
                "CREATE TABLE IF NOT EXISTS product_parameter (product_parameter_id INTEGER PRIMARY KEY, product_id INTEGER, parameter_id INTEGER, value NVARCHAR(100))",
                "CREATE TABLE IF NOT EXISTS user_order (order_id INTEGER PRIMARY KEY, user_id INTEGER, order_date DATE, delivery NVARCHAR(100), payment NVARCHAR(100), status NVARCHAR(100))",
                "CREATE TABLE IF NOT EXISTS product_order (product_order_id INTEGER PRIMARY KEY, order_id INTEGER, product_name NVARCHAR(100), quantity INTEGER, price_per_item DECIMAL(8, 2))"
            };
            foreach (string command in tableCommands)
            {
                var createTable = new SqliteCommand(command, db);
                createTable.ExecuteReader();
            }
        }
        //Users
        public static long? CreateUser(string username, string password, string email, DateTime ?date, string? phone)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var insertCommand = new SqliteCommand("INSERT INTO user(username, password, email, birth_date, phone_number, is_logged, user_type) VALUES (@Username, @Password, @Email, @BirthDate, @PhoneNumber, 1, 1)", db);
            insertCommand.Parameters.AddWithValue("@Username", username);
            insertCommand.Parameters.AddWithValue("@Password", password);
            insertCommand.Parameters.AddWithValue("@Email", email);
            insertCommand.Parameters.AddWithValue("@BirthDate", date);
            insertCommand.Parameters.AddWithValue("@PhoneNumber", phone == null ? DBNull.Value : phone);
            insertCommand.ExecuteReader();
            insertCommand = new SqliteCommand("SELECT last_insert_rowid()", db);
            return (long?)insertCommand.ExecuteScalar();
        }
        public static Dictionary<string, string?>? CheckUsername(string username)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("SELECT * FROM user WHERE username = @Username", db);
            command.Parameters.AddWithValue("@Username", username);
            SqliteDataReader query = command.ExecuteReader();
            if (query.HasRows)
            {
                query.Read();
                return new Dictionary<string, string?>
                {
                    {"Id",  query.GetInt32(0).ToString()},
                    {"Username", query.GetString(1)},
                    {"Password", query.GetString(2)},
                    {"Email", query.GetString(3)},
                    {"Birth date", query.GetDateTime(4).ToString()},
                    {"Phone number", query.IsDBNull(5)?null:query.GetString(5)},
                    {"First name",  query.IsDBNull(6)?null:query.GetString(6)},
                    {"Surname", query.IsDBNull(7)?null:query.GetString(7)},
                    {"Postcode", query.IsDBNull(8)?null:query.GetString(8)},
                    {"City", query.IsDBNull(9)?null:query.GetString(9)},
                    {"Street", query.IsDBNull(10)?null:query.GetString(10)},
                    {"House number", query.IsDBNull(11)?null:query.GetString(11)},
                    {"Position", query.GetInt32(13).ToString()}
                };
            }
            return null;
        }
        public static bool CheckEmail(string email)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("SELECT * FROM user WHERE email = @Email", db);
            command.Parameters.AddWithValue("@Email", email);
            SqliteDataReader query = command.ExecuteReader();
            if (query.HasRows)
                return true;
            else
                return false;
        }
        //Categories
        public static void CreateCategory(Category category)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var insertCommand = new SqliteCommand
            {
                Connection = db,
                CommandText = "INSERT INTO category(category, image) VALUES (@category, @image);"
            };
            insertCommand.Parameters.AddWithValue("@category", category.Name);
            insertCommand.Parameters.AddWithValue("@image", category.Image);
            insertCommand.ExecuteReader();
        }
        public static int CountProducts(int categoryId)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("SELECT COUNT(*) FROM product_category WHERE category_id = @CategoryId", db);
            command.Parameters.AddWithValue("@CategoryId", categoryId);
            SqliteDataReader query = command.ExecuteReader();
            query.Read();
            return query.GetInt32(0);
        }
        public static List<Category> ReadCategory()
        {
            var entries = new List<Category>();
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using (var db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                var selectCommand = new SqliteCommand
                    ("SELECT * FROM category", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Category category = new()
                    {
                        Id = query.GetInt32(0),
                        Name = query.GetString(1),
                        Image = query.GetString(2)
                    };
                    entries.Add(category);
                }
            }
            return entries;
        }
        public static List<int> ReadCategories(int productId)
        {
            List<int> list = new();
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("SELECT category_id FROM product_category WHERE product_id = @ProductId", db);
            command.Parameters.AddWithValue("@ProductId", productId);
            SqliteDataReader query = command.ExecuteReader();
            while(query.Read())
                list.Add(query.GetInt32(0));
            return list;
        }
        public static void UpdateCategory(Category category)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var updateCommand = new SqliteCommand
            {
                Connection = db,
                CommandText = "UPDATE category SET category=@Category, image=@Image WHERE category_id=@Id"
            };
            updateCommand.Parameters.AddWithValue("@Id", category.Id);
            updateCommand.Parameters.AddWithValue("@Category", category.Name);
            updateCommand.Parameters.AddWithValue("@Image", category.Image);
            updateCommand.ExecuteNonQuery();
        }
        public static void DeleteCategory(Category category)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var updateCommand = new SqliteCommand
            {
                Connection = db,
                CommandText = "DELETE FROM category WHERE category_id=@Id"
            };
            updateCommand.Parameters.AddWithValue("@Id", category.Id);
            updateCommand.ExecuteNonQuery();
        }
        //Products
        public static long? CreateProduct(Product product, List<int> categories, Dictionary<string, string> ?parameters)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var insertCommand = new SqliteCommand("INSERT INTO product(name, description, price, promotion, best_feature, quantity, guarantee) VALUES (@Name, @Description, @Price, @Promotion, @BestFeature, @Quantity, @Guarantee)", db);
            insertCommand.Parameters.AddWithValue("@Name", product.Name);
            insertCommand.Parameters.AddWithValue("@Description", product.Description);
            insertCommand.Parameters.AddWithValue("@Price", product.Price);
            insertCommand.Parameters.AddWithValue("@Promotion", product.Promotion == null ? DBNull.Value : product.Promotion);
            insertCommand.Parameters.AddWithValue("@BestFeature", product.BestFeature == null ? DBNull.Value : product.BestFeature);
            insertCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
            insertCommand.Parameters.AddWithValue("@Guarantee", product.Guarantee == null ? DBNull.Value : product.Guarantee);
            insertCommand.ExecuteReader();
            insertCommand = new SqliteCommand("SELECT last_insert_rowid()", db);
            long ?insertedProductId = (long?)insertCommand.ExecuteScalar();
            categories = categories.Distinct().ToList();
            foreach (int category in categories)
            {
                insertCommand = new SqliteCommand("INSERT INTO product_category(product_id, category_id) VALUES (@ProductId, @CategoryId)", db);
                insertCommand.Parameters.AddWithValue("@ProductId", insertedProductId);
                insertCommand.Parameters.AddWithValue("@CategoryId", category);
                insertCommand.ExecuteReader();
            }
            if(parameters is not null)
            {
                insertCommand = new SqliteCommand("SELECT * FROM parameter", db);
                Dictionary<int, string> allParameters = new();
                SqliteDataReader query = insertCommand.ExecuteReader();
                while (query.Read())
                    allParameters.Add(query.GetInt32(0), query.GetString(1));           
                var keys = parameters.Keys;  
                foreach(string key in keys)
                {
                    long ?parameterId;
                    if (allParameters.ContainsValue(key))
                        parameterId = allParameters.FirstOrDefault(x => x.Value == key).Key;
                    else
                    {
                        insertCommand = new SqliteCommand("INSERT INTO parameter(parameter) VALUES(@Parameter)", db);
                        insertCommand.Parameters.AddWithValue("@Parameter", key);
                        insertCommand.ExecuteReader();
                        insertCommand = new SqliteCommand("SELECT last_insert_rowid()", db);
                        parameterId = (long?)insertCommand.ExecuteScalar();  
                    }
                    insertCommand = new SqliteCommand("INSERT INTO product_parameter(product_id, parameter_id, value) VALUES(@ProductId, @ParameterId, @Value)", db);
                    insertCommand.Parameters.AddWithValue("@ProductId", insertedProductId);
                    insertCommand.Parameters.AddWithValue("@ParameterId", parameterId);
                    insertCommand.Parameters.AddWithValue("@Value", parameters[key]);
                    insertCommand.ExecuteReader();
                }
            }
            return insertedProductId;
            
        }
        public static void UpdateProduct(Product product, List<int> categories, Dictionary<string, string>? parameters)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var insertCommand = new SqliteCommand("UPDATE product SET name=@Name, description=@Description, price=@Price, promotion=@Promotion, best_feature=@BestFeature, quantity=@Quantity, guarantee=@Guarantee WHERE product_id=@ProductId", db);
            insertCommand.Parameters.AddWithValue("@Name", product.Name);
            insertCommand.Parameters.AddWithValue("@Description", product.Description);
            insertCommand.Parameters.AddWithValue("@Price", product.Price);
            insertCommand.Parameters.AddWithValue("@Promotion", product.Promotion == null ? DBNull.Value : product.Promotion);
            insertCommand.Parameters.AddWithValue("@BestFeature", product.BestFeature == null ? DBNull.Value : product.BestFeature);
            insertCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
            insertCommand.Parameters.AddWithValue("@Guarantee", product.Guarantee == null ? DBNull.Value : product.Guarantee);
            insertCommand.Parameters.AddWithValue("@ProductId", product.Id);
            insertCommand.ExecuteReader();
            insertCommand = new SqliteCommand("DELETE FROM product_parameter WHERE product_id = @ProductId", db);
            insertCommand.Parameters.AddWithValue("@ProductId", product.Id);
            insertCommand.ExecuteReader();
            insertCommand = new SqliteCommand("DELETE FROM product_category WHERE product_id = @ProductId", db);
            insertCommand.Parameters.AddWithValue("@ProductId", product.Id);
            insertCommand.ExecuteReader();
            categories = categories.Distinct().ToList();
            foreach (int category in categories)
            {
                insertCommand = new SqliteCommand("INSERT INTO product_category(product_id, category_id) VALUES (@ProductId, @CategoryId)", db);
                insertCommand.Parameters.AddWithValue("@ProductId", product.Id);
                insertCommand.Parameters.AddWithValue("@CategoryId", category);
                insertCommand.ExecuteReader();
            }
            if (parameters is not null)
            {
                insertCommand = new SqliteCommand("SELECT * FROM parameter", db);
                Dictionary<int, string> allParameters = new();
                SqliteDataReader query = insertCommand.ExecuteReader();
                while (query.Read())
                    allParameters.Add(query.GetInt32(0), query.GetString(1));
                var keys = parameters.Keys;
                foreach (string key in keys)
                {
                    long? parameterId;
                    if (allParameters.ContainsValue(key))
                        parameterId = allParameters.FirstOrDefault(x => x.Value == key).Key;
                    else
                    {
                        insertCommand = new SqliteCommand("INSERT INTO parameter(parameter) VALUES(@Parameter)", db);
                        insertCommand.Parameters.AddWithValue("@Parameter", key);
                        insertCommand.ExecuteReader();
                        insertCommand = new SqliteCommand("SELECT last_insert_rowid()", db);
                        parameterId = (long?)insertCommand.ExecuteScalar();
                    }
                    insertCommand = new SqliteCommand("INSERT INTO product_parameter(product_id, parameter_id, value) VALUES(@ProductId, @ParameterId, @Value)", db);
                    insertCommand.Parameters.AddWithValue("@ProductId", product.Id);
                    insertCommand.Parameters.AddWithValue("@ParameterId", parameterId);
                    insertCommand.Parameters.AddWithValue("@Value", parameters[key]);
                    insertCommand.ExecuteReader();
                }
            }
        }
        public static (Product, Dictionary<string, string>) ReadProduct(int productId)
        {
            Product p = new();
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("SELECT * FROM product WHERE product_id = @ProductId", db);
            command.Parameters.AddWithValue("@ProductId", productId);
            SqliteDataReader query = command.ExecuteReader();
            query.Read();
            p.Id = query.GetInt32(0);
            p.Name = query.GetString(1);    
            p.Description = query.GetString(2); 
            p.Price = query.GetDecimal(3);
            p.Promotion = query.IsDBNull(4) ? null : query.GetDecimal(4);
            p.BestFeature = query.IsDBNull(5) ? null : query.GetString(5);
            p.Quantity = query.GetInt32(6);
            p.Guarantee = query.IsDBNull(7) ? null : query.GetInt32(7);
            Dictionary<string, string> d = new();
            command = new SqliteCommand("SELECT parameter, value FROM product_parameter INNER JOIN parameter USING(parameter_id) WHERE product_id = @ProductId", db);
            command.Parameters.AddWithValue("@ProductId", productId);
            query = command.ExecuteReader();
            while(query.Read())
                d[query.GetString(0)] = query.GetString(1);
            return (p, d);
        }
        public static List<Product> ReadProducts(int categoryId)
        {
            List<Product> products = new();  
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("SELECT DISTINCT product_id FROM product_category WHERE category_id = @CategoryId", db);
            command.Parameters.AddWithValue("@CategoryId", categoryId);
            SqliteDataReader query = command.ExecuteReader();
            while(query.Read())
                products.Add(ReadProduct(query.GetInt32(0)).Item1);
            return products;
        }
        public static void DeleteProduct(int productId)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("DELETE FROM product WHERE product_id = @ProductId", db);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.ExecuteReader();
            command = new SqliteCommand("DELETE FROM product_parameter WHERE product_id = @ProductId", db);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.ExecuteReader();
            command = new SqliteCommand("DELETE FROM product_category WHERE product_id = @ProductId", db);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.ExecuteReader();          
            Dictionary<int, int> cart;
            File.AppendAllText("..\\..\\..\\cart.txt", "");
            if (File.ReadAllBytes("..\\..\\..\\cart.txt").Length > 0)
            {
                cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(File.ReadAllText("..\\..\\..\\cart.txt")) ?? new();
                cart.Remove(productId);
                File.WriteAllText("..\\..\\..\\cart.txt", System.Text.Json.JsonSerializer.Serialize(cart));
            }
        }
        public static void ChangeQuantity(int productId, int newQuantity)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("UPDATE product SET quantity = @Quantity WHERE product_id = @ProductId", db);
            command.Parameters.AddWithValue("@Quantity", newQuantity);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.ExecuteReader();
        }
        // Order
        public static void CreateOrder(int user_id, DateTime order_date, string delivery, string payment, string status, string?[] productNames, int[] quantities, decimal?[] pricesPerItem)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var insertCommand = new SqliteCommand("INSERT INTO user_order(user_id, order_date, delivery, payment, status) VALUES (@UserId, @OrderDate, @Delivery, @Payment, @Status)", db);
            insertCommand.Parameters.AddWithValue("@UserId", user_id);
            insertCommand.Parameters.AddWithValue("@OrderDate", order_date);
            insertCommand.Parameters.AddWithValue("@Delivery", delivery);
            insertCommand.Parameters.AddWithValue("@Payment", payment);
            insertCommand.Parameters.AddWithValue("@Status", status);
            insertCommand.ExecuteReader();
            insertCommand = new SqliteCommand("SELECT last_insert_rowid()", db);
            long? insertedId = (long?)insertCommand.ExecuteScalar();
            for (int i = 0; i < productNames.Length; i++)
            {
                insertCommand = new SqliteCommand("INSERT INTO product_order(order_id, product_name, quantity, price_per_item) VALUES (@OrderId, @ProductName, @Quantity, @Price)", db);
                insertCommand.Parameters.AddWithValue("@OrderId", insertedId);
                insertCommand.Parameters.AddWithValue("@ProductName", productNames[i]);
                insertCommand.Parameters.AddWithValue("@Quantity", quantities[i]);
                insertCommand.Parameters.AddWithValue("@Price", pricesPerItem[i]);
                insertCommand.ExecuteReader();
            }
        }
        public static List<Order> ReadOrder()
        {
            var entries = new List<Order>();
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using (var db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                var selectCommand = new SqliteCommand
                    ("SELECT order_id, username, order_date, delivery, payment, status FROM user_order INNER JOIN user USING(user_id)", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Order order = new()
                    {
                        Id = query.GetInt32(0),
                        Username = query.GetString(1),
                        Date = query.GetDateTime(2),
                        Delivery = query.GetString(3),
                        Payment = query.GetString(4),
                        Status = query.GetString(5)
                    };
                    entries.Add(order);
                }
            }
            return entries;
        }
        public static List<string> ReadOrderDetails(int orderId)
        {
            List<string> details = new();
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("SELECT * FROM product_order WHERE order_id = @OrderId", db);
            command.Parameters.AddWithValue("@OrderId", orderId);
            SqliteDataReader query = command.ExecuteReader();
            while (query.Read())
                details.Add(query.GetString(2) + ": " + query.GetInt32(3).ToString() + " x " + FormatNumber(query.GetDecimal(4)) + " zł");
            return details;
        }
        public static void UpdateOrderStatus(int orderId, string newStatus)
        {
            string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sklep.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            db.Open();
            var command = new SqliteCommand("UPDATE user_order SET status=@Status WHERE order_id=@OrderId", db);
            command.Parameters.AddWithValue("@OrderId", orderId);
            command.Parameters.AddWithValue("@Status", newStatus);
            command.ExecuteReader();
        }
    }
}
