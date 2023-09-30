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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Security.Cryptography;

namespace MatiTechShop
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }
    /// <summary>
    /// Logika interakcji dla klasy Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        public string ButtonTitle { get; set; } = "Zaloguj się";
        public Login()
        {
            InitializeComponent();
            DataContext = this;
        }
        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
            };
            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };
            SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }
        public static bool VerifyPassword(string password, string input)
        {
            var elements = password.Split(';');
            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);
            var password2 = Rfc2898DeriveBytes.Pbkdf2(input, salt, 10000, HashAlgorithmName.SHA256, 256 / 8);
            return CryptographicOperations.FixedTimeEquals(hash, password2);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void TryLogin()
        {
            var user = DataAccess.CheckUsername(login.Text);
            if (user != null)
            {
                if (VerifyPassword(user["Password"] ?? "", password.Password))
                {
                    user.Remove("Password");
                    DataAccess.UserData = user;
                    Close();
                }
                else
                    error.Visibility = Visibility.Visible;
            }
            else
                error.Visibility = Visibility.Visible;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Registration r = new();
            Close();
            r.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                TryLogin();
        }
    }
}
