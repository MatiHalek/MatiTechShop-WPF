using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
    /// Logika interakcji dla klasy Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public string ButtonTitle { get; set; } =  "Zarejestruj się";
        public static string GenerateHash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(128 / 8);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 256 / 8);
            return string.Join(';', Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }
        public Registration()
        {
            InitializeComponent();
            DataContext = this;
            dateEntry.DisplayDateStart = new DateTime(1900, 1, 1);
            dateEntry.DisplayDateEnd = DateTime.Today;
        }
        private bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                usernameTextBlock.Text = "Proszę podać login.";
                usernameLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (username.Length < 3 || username.Length > 15)
            {
                usernameTextBlock.Text = "Nazwa użytkownika musi zawierać od 3 do 15 znaków.";
                usernameLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (username.StartsWith("_") || username.EndsWith("_"))
            {
                usernameTextBlock.Text = "Znak _ nie może znajdować się na początku lub końcu loginu.";
                usernameLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (CountRegex().Matches(username).Count > 1)
            {
                usernameTextBlock.Text = "Login może zawierać maksymalnie jeden znak _.";
                usernameLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (DigitRegex().IsMatch(username))
            {
                usernameTextBlock.Text = "Login nie może zawierać samych cyfr.";
                usernameLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (!UsernameRegex().IsMatch(username))
            {
                usernameTextBlock.Text = "Login może zawierać tylko wielkie i małe litery alfabetu łacińskiego, cyfry i znak _.";
                usernameLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (DataAccess.CheckUsername(username) != null)
            {
                usernameTextBlock.Text = "Taki login już istnieje. Wybierz inny.";
                usernameLabel.Visibility = Visibility.Visible;
                return false;
            }
            usernameLabel.Visibility = Visibility.Collapsed;
            return true;
        }
        private bool ValidatePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                passwordTextBlock.Text = "Proszę wpisać hasło.";
                passwordLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (!PasswordRegex().IsMatch(password))
            {
                passwordTextBlock.Text = "Hasło musi zawierać od 8 do 255 znaków (w tym cyfry, małe i wielkie litery oraz znaki specjalne).";
                passwordLabel.Visibility = Visibility.Visible;
                return false;
            }
            passwordLabel.Visibility = Visibility.Collapsed;
            return true;
        }
        private bool ValidatePassword2(string? password1, string? password2)
        {
            if (string.IsNullOrWhiteSpace(password2))
            {
                password2TextBlock.Text = "Proszę wpisać ponownie hasło.";
                password2Label.Visibility = Visibility.Visible;
                return false;
            }
            if (password1 != password2)
            {
                password2TextBlock.Text = "Wprowadzone hasła nie są identyczne.";
                password2Label.Visibility = Visibility.Visible;
                return false;
            }
            password2Label.Visibility = Visibility.Collapsed;
            return true;
        }
        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                emailTextBlock.Text = "Proszę podać adres e-mail.";
                emailLabel.Visibility = Visibility.Visible;
                return false;
            }
            var emailChecker = new EmailAddressAttribute();
            if (!emailChecker.IsValid(email))
            {
                emailTextBlock.Text = "Ten adres e-mail jest nieprawidłowy.";
                emailLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (DataAccess.CheckEmail(email) == true)
            {
                emailTextBlock.Text = "Ten email jest już przypisany do innego konta.";
                emailLabel.Visibility = Visibility.Visible;
                return false;
            }
            emailLabel.Visibility = Visibility.Collapsed;
            return true;
        }
        private bool ValidateDate(string? date)
        {
            string[] formats = { "yyyy-MM-dd", "dd.MM.yyyy"};
            if (!DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime inputDate))
            {
                dateTextBlock.Text = "Proszę podać poprawną datę (akceptowalne formaty: RRRR-MM-DD, DD.MM.RRRR)";
                dateLabel.Visibility = Visibility.Visible;
                return false;
            }
            DateTime d = DateTime.Today;
            if(inputDate > d || inputDate < new DateTime(1900, 1, 1))
            {
                dateTextBlock.Text = "Ta data jest nieobsługiwana.";
                dateLabel.Visibility = Visibility.Visible;
                return false;
            }
            if (d.AddYears(-13) < inputDate)
            {
                dateTextBlock.Text = "Aby założyć konto, musisz mieć co najmniej 13 lat.";
                dateLabel.Visibility = Visibility.Visible;
                return false;
            }
            dateLabel.Visibility = Visibility.Collapsed;
            return true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool success = true;
            if (!ValidateUsername(usernameEntry.Text))
                success = false;
            if (!ValidatePassword(passwordEntry.Password))
                success = false;
            if (!ValidatePassword2(passwordEntry.Password, password2Entry.Password))
                success = false;
            if (!ValidateEmail(emailEntry.Text))
                success = false;
            if (!ValidateDate(dateEntry.Text))
                success = false;
            if (checkbox.IsChecked == false)
            {
                checkboxLabel.Visibility = Visibility.Visible;
                success = false;
            }                
            if (success == true)
            {
                long ?insertId = DataAccess.CreateUser(usernameEntry.Text, GenerateHash(passwordEntry.Password), emailEntry.Text, dateEntry.SelectedDate, phoneEntry.Text);
                DataAccess.UserData = new Dictionary<string, string?>
                {
                    {"Id", insertId.ToString()},
                    {"Username", usernameEntry.Text},
                    {"Email", emailEntry.Text },
                    {"Birth date", dateEntry.SelectedDate.ToString() },
                    {"Phone number", phoneEntry.Text},
                    {"First name", null},
                    {"Surname", null },
                    {"Postcode", null },
                    {"City", null },
                    {"Street", null },
                    {"House number", null },
                    {"Position", "1" }
                };
                MessageBox.Show("Pomyślnie utworzono nowe konto!\nJesteś teraz zalogowany/a jako: " + usernameEntry.Text + ".", "Tworzenie nowego konta", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }
        private void UsernameEntry_TextChanged(object sender, TextChangedEventArgs e) => ValidateUsername(usernameEntry.Text);
        private void PasswordEntry_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidatePassword(passwordEntry.Password);
            ValidatePassword2(passwordEntry.Password, password2Entry.Password);
        }
        private void Password2Entry_PasswordChanged(object sender, RoutedEventArgs e) => ValidatePassword2(passwordEntry.Password, password2Entry.Password);
        private void EmailEntry_TextChanged(object sender, TextChangedEventArgs e) => ValidateEmail(emailEntry.Text);
        private void DateEntry_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ValidateDate(dateEntry.Text);
        private void DateEntry_KeyUp(object sender, KeyEventArgs e) => ValidateDate(dateEntry.Text);
        private void Checkbox_Checked(object sender, RoutedEventArgs e) => checkboxLabel.Visibility = Visibility.Collapsed;
        private void Checkbox_Unchecked(object sender, RoutedEventArgs e) => checkboxLabel.Visibility = Visibility.Visible;
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => checkbox.IsChecked = checkbox.IsChecked != true;
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Login l = new();
            Close();
            l.ShowDialog();           
        }
        private void Button_Click_Close(object sender, RoutedEventArgs e) => Close();
        private void DateEntry_LostFocus(object sender, RoutedEventArgs e)
        {
            if(dateEntry.Text != string.Empty)
                ValidateDate(dateEntry.Text);
        }

        [GeneratedRegex("^(?=.*[\\p{L}])(?=.*\\d)(?=.*[!@#$%^&*()])[\\p{L}\\d!@#$%^&*()]{8,255}$")]
        private static partial Regex PasswordRegex();
        [GeneratedRegex("^[0-9A-Za-z]{1}[0-9A-Za-z_]{1,13}[0-9A-Za-z]{1}$")]
        private static partial Regex UsernameRegex();
        [GeneratedRegex("^\\d+$")]
        private static partial Regex DigitRegex();
        [GeneratedRegex("_")]
        private static partial Regex CountRegex();
    }
}
