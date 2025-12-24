using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlApi;
using BO;

namespace PL
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml.
    /// Handles user authentication and navigation.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get();

        #region Dependency Properties

        // Dependency Property for MVVM Binding of the User ID
        public string UserIdInput
        {
            get { return (string)GetValue(UserIdInputProperty); }
            set { SetValue(UserIdInputProperty, value); }
        }

        public static readonly DependencyProperty UserIdInputProperty =
            DependencyProperty.Register(nameof(UserIdInput), typeof(string), typeof(LoginWindow), new PropertyMetadata(""));

        #endregion

        public LoginWindow()
        {
            InitializeComponent();

            // Set DataContext to self to enable DataBinding for UserIdInput
            this.DataContext = this;
        }

        #region UI Event Handlers

        /// <summary>
        /// Toggles password visibility between the PasswordBox and the visible TextBox.
        /// </summary>
        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                // Switch to visible mode
                VisiblePasswordBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;
            }
            else
            {
                // Switch to hidden mode
                PasswordBox.Password = VisiblePasswordBox.Text;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles the login process.
        /// Uses DataBinding for ID (MVVM compliant) and direct access for Password (security best practice).
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Visibility = Visibility.Collapsed;

            // Use the Bound Property for ID instead of accessing the TextBox directly
            string idInput = UserIdInput;

            // Retrieve password from the active control
            string passwordInput = (PasswordBox.Visibility == Visibility.Visible) ? PasswordBox.Password : VisiblePasswordBox.Text;

            if (string.IsNullOrWhiteSpace(idInput) || string.IsNullOrWhiteSpace(passwordInput))
            {
                ShowError("Please enter your ID and password.");
                return;
            }

            if (!int.TryParse(idInput, out int id))
            {
                ShowError("ID must contain only numbers.");
                return;
            }

            try
            {
                EmployType type = s_bl.Courier.EnterToSystem(id, passwordInput);

                switch (type)
                {
                    case EmployType.Manager:
                        new MainWindow().Show();
                        this.Close();
                        break;

                    case EmployType.Courier:
                        new PL.Courier.MainCourierWindow(id).Show();
                        this.Close();
                        break;

                    default:
                        ShowError("Unknown user type.");
                        break;
                }
            }
            catch (BlInvalidDataException ex)
            {
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("System Error: " + ex.Message);
            }
        }

        #endregion

        #region Helpers

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        #endregion
    }
}