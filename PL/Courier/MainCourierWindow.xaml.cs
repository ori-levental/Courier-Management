using BlApi;
using BO;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PL.Courier
{
    public partial class MainCourierWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();
        private int _courierId;

        // משתנה עזר למניעת לולאות עדכון סיסמה
        private bool _isPasswordSyncing = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        // --- Dependency Properties ---

        public BO.Courier Courier
        {
            get { return (BO.Courier)GetValue(CourierProperty); }
            set { SetValue(CourierProperty, value); }
        }
        public static readonly DependencyProperty CourierProperty =
            DependencyProperty.Register("Courier", typeof(BO.Courier), typeof(MainCourierWindow));

        public bool HasActiveOrder
        {
            get { return (bool)GetValue(HasActiveOrderProperty); }
            set { SetValue(HasActiveOrderProperty, value); }
        }
        public static readonly DependencyProperty HasActiveOrderProperty =
            DependencyProperty.Register("HasActiveOrder", typeof(bool), typeof(MainCourierWindow));

        public bool HasNoActiveOrder => !HasActiveOrder;
        public bool IsVehicleEditable => HasNoActiveOrder;

        public MainCourierWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;
            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                Courier = s_bl.Courier.SearchCourier(_courierId, _courierId);

                // עדכון הסיסמה בתיבה המוסתרת (PasswordBox לא תומך ב-Binding)
                if (Courier.Password != null)
                {
                    _isPasswordSyncing = true;
                    pbPassword.Password = Courier.Password;
                    _isPasswordSyncing = false;
                }

                HasActiveOrder = (Courier.OrderInCare != null);

                OnPropertyChanged(nameof(HasNoActiveOrder));
                OnPropertyChanged(nameof(IsVehicleEditable));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
                Close();
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Password Logic ---

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // אם המשתמש מקליד בתיבה המוסתרת, נעדכן את האובייקט
            if (!_isPasswordSyncing)
            {
                Courier.Password = pbPassword.Password;
            }
        }

        // helped bt gemini
        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            // החלפה בין מצב גלוי למצב מוסתר
            if (pbPassword.Visibility == Visibility.Visible)
            {
                // מעבר למצב גלוי
                pbPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;

                // עדכון הטקסט הגלוי (Binding כבר עושה את זה, אבל ליתר ביטחון)
                // txtVisiblePassword מחובר ב-Binding, אז הוא יתעדכן לבד
            }
            else
            {
                // מעבר למצב מוסתר (נקודות)
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                pbPassword.Visibility = Visibility.Visible;

                // סנכרון חזרה למקרה ששינו את הטקסט הגלוי
                _isPasswordSyncing = true;
                pbPassword.Password = Courier.Password;
                _isPasswordSyncing = false;
            }
        }

        // --- Main Actions ---

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // וידוא שהסיסמה מעודכנת באובייקט לפני השליחה (במקרה שערכו בתיבה הגלויה)
                if (txtVisiblePassword.Visibility == Visibility.Visible)
                    Courier.Password = txtVisiblePassword.Text;
                else
                    Courier.Password = pbPassword.Password;

                s_bl.Courier.UpdateCourier(_courierId, Courier);
                MessageBox.Show("Details updated successfully!", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RefreshData();
            }
        }

        private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Courier.OrderInCare == null) return;

            try
            {
                // ה-DeliveryId כבר קיים באובייקט OrderInCare שמגיע מ-Tools
                int deliveryId = Courier.OrderInCare.DeliveryId;

                // סגירת ההזמנה
                s_bl.Order.CloseOrder(_courierId, _courierId, deliveryId);

                MessageBox.Show("Order delivered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opens Order Selection Window (Not implemented yet).");
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opens History Window (Not implemented yet).");
        }
    }
}