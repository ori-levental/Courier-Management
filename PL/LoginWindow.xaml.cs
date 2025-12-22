using System;
using System.Windows;
using System.Windows.Controls; // נדרש עבור PasswordBox ו-TextBox
using System.Windows.Input;
using BlApi;
using BO;

namespace PL
{
    public partial class LoginWindow : Window
    {
        // גישה לשכבת הלוגיקה
        static readonly IBl s_bl = Factory.Get();

        public LoginWindow()
        {
            InitializeComponent();
        }

        // --- לוגיקה להצגת/הסתרת סיסמה ---
        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                // מעבר למצב גלוי (טקסט)
                VisiblePasswordBox.Text = PasswordBox.Password; // העתקת הסיסמה לתיבה הגלויה
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;
            }
            else
            {
                // מעבר למצב מוסתר (נקודות)
                PasswordBox.Password = VisiblePasswordBox.Text; // העתקת הסיסמה לתיבה המוסתרת
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // איפוס הודעות שגיאה קודמות
            ErrorMessage.Visibility = Visibility.Collapsed;

            string idInput = IdTextBox.Text;

            // שליפת הסיסמה מהתיבה שגלויה כרגע למשתמש
            string passwordInput = (PasswordBox.Visibility == Visibility.Visible)
                                    ? PasswordBox.Password
                                    : VisiblePasswordBox.Text;

            // 1. בדיקת קלט בסיסית (ריק)
            if (string.IsNullOrWhiteSpace(idInput) || string.IsNullOrWhiteSpace(passwordInput))
            {
                ShowError("Please enter your ID and password.");
                return;
            }

            // 2. בדיקה שה-ID הוא מספר
            if (!int.TryParse(idInput, out int id))
            {
                ShowError("ID must contain only numbers.");
                return;
            }

            try
            {
                // 3. קריאה ל-BL
                EmployType type = s_bl.Courier.EnterToSystem(id, passwordInput);

                // 4. ניתוב לפי סוג העובד שחזר
                switch (type)
                {
                    case EmployType.Manager:
                        new MainWindow().Show(); // פתיחת מסך מנהל
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

        // פונקציית עזר להצגת שגיאות
        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        // פונקציה שדואגת שהתיבה תקבל רק מספרים
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}