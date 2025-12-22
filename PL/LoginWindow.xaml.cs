using System;
using System.Windows;
using System.Windows.Controls; // Required for PasswordBox and TextBox
using System.Windows.Input;
using BlApi;
using BO;

namespace PL
{
    public partial class LoginWindow : Window
    {
        // Access to the Logic Layer
        static readonly IBl s_bl = Factory.Get();

        public LoginWindow()
        {
            InitializeComponent();
        }

        // --- Password Show/Hide Logic ---
        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                // Switch to visible mode (text)
                VisiblePasswordBox.Text = PasswordBox.Password; // Copy password to the visible textbox
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;
            }
            else
            {
                // Switch to hidden mode (dots)
                PasswordBox.Password = VisiblePasswordBox.Text; // Copy password to the hidden passwordbox
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Reset previous error messages
            ErrorMessage.Visibility = Visibility.Collapsed;

            string idInput = IdTextBox.Text;

            // Retrieve password from the currently visible box
            string passwordInput = (PasswordBox.Visibility == Visibility.Visible)
                                    ? PasswordBox.Password
                                    : VisiblePasswordBox.Text;

            // 1. Basic input validation (empty)
            if (string.IsNullOrWhiteSpace(idInput) || string.IsNullOrWhiteSpace(passwordInput))
            {
                ShowError("Please enter your ID and password.");
                return;
            }

            // 2. Validate that ID is a number
            if (!int.TryParse(idInput, out int id))
            {
                ShowError("ID must contain only numbers.");
                return;
            }

            try
            {
                // 3. Call BL
                EmployType type = s_bl.Courier.EnterToSystem(id, passwordInput);

                // 4. Route based on returned employee type
                switch (type)
                {
                    case EmployType.Manager:
                        new MainWindow().Show(); // Open manager screen
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

        // Helper function to display errors
        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        // Function ensuring the box accepts only numbers
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}