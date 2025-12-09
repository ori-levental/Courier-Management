using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using BlApi;
using DalApi;
using BO;
using DO;

namespace PL.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryWindow.xaml
    /// </summary>
    public partial class DeliveryWindow : Window
    {
        // BL access (use BL if you have a loader there)
        static readonly IBl s_bl = BlApi.Factory.Get();

        // DAL fallback (used here to load DO and map to a view-model)
        static readonly IDal s_dal = DalApi.Factory.Get;

        // ButtonText DP (already present)
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(DeliveryWindow), new PropertyMetadata(string.Empty));
        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        // DependencyProperty that holds the mutable view-model instance shown on the screen
        public DeliveryInListViewModel? CurrentDelivery
        {
            get => (DeliveryInListViewModel?)GetValue(CurrentDeliveryProperty);
            set => SetValue(CurrentDeliveryProperty, value);
        }
        public static readonly DependencyProperty CurrentDeliveryProperty =
            DependencyProperty.Register(nameof(CurrentDelivery), typeof(DeliveryInListViewModel), typeof(DeliveryWindow), new PropertyMetadata(null));

        // Helper collections for ComboBoxes (read-only)
        public IEnumerable<BO.ShippingType> DeliveryTypes { get; }

        // expose static converter instances so XAML can reference them via x:Static
        public static readonly IValueConverter IdToReadOnlyKey = new IdToReadOnlyConverter();
        public static readonly IValueConverter IdToVisibilityKey = new IdToVisibilityConverter();

        // parameterless ctor routes to main ctor
        public DeliveryWindow() : this(0) { }

        // id == 0 -> Add, otherwise Update
        // NOTE: default parameter added as requested
        public DeliveryWindow(int id = 0)
        {
            // prepare ComboSources (do this before InitializeComponent so XAML can use them)
            DeliveryTypes = Enum.GetValues(typeof(BO.ShippingType)).Cast<BO.ShippingType>();

            // Set Button text BEFORE the view loads
            ButtonText = id == 0 ? "Add" : "Update";

            InitializeComponent();

            // DataContext is set in XAML to Self (RelativeSource Mode=Self)
            // Now assign the dependency property CurrentDelivery AFTER InitializeComponent,
            // per your instruction — create new view-model for Add mode or load existing for Update mode.
            if (id == 0)
            {
                // Add mode: new mutable VM with default/empty values
                CurrentDelivery = new DeliveryInListViewModel
                {
                    DeliveryId = 0,
                    FullName = string.Empty,
                    IsActive = true,
                    ShippingType = BO.ShippingType.Car,
                    EmploymentStartDate = DateTime.Now,
                    SumOrderInTime = 0,
                    SumOrderInLate = 0,
                    IdOrderInCare = null
                };
            }
            else
            {
                try
                {
                    // Try to load from BL first
                    var deliveryInList = s_bl.Courier.ListOfCourier(0, null, null)
    .FirstOrDefault(d => d.Id == id);
                    if (deliveryInList != null)
                    {
                        CurrentDelivery = new DeliveryInListViewModel
                        {
                            DeliveryId = deliveryInList.Id,
                            FullName = deliveryInList.FullName,
                            IsActive = deliveryInList.IsActive,
                            ShippingType = deliveryInList.DeliveryType,
                            EmploymentStartDate = deliveryInList.EmploymentStartDate,
                            SumOrderInTime = deliveryInList.SumOrderInTime,
                            SumOrderInLate = deliveryInList.SumOrderInLate,
                            IdOrderInCare = deliveryInList.IdOrderInCare
                        };
                    }
                    else
                    {
                        CurrentDelivery = new DeliveryInListViewModel
                        {
                            DeliveryId = id,
                            FullName = string.Empty,
                            IsActive = true,
                            ShippingType = BO.ShippingType.Car,
                            EmploymentStartDate = DateTime.Now,
                            SumOrderInTime = 0,
                            SumOrderInLate = 0,
                            IdOrderInCare = null
                        };
                        MessageBox.Show($"Delivery with ID {id} not found.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    CurrentDelivery = new DeliveryInListViewModel
                    {
                        DeliveryId = id,
                        FullName = string.Empty,
                        IsActive = true,
                        ShippingType = BO.ShippingType.Car,
                        EmploymentStartDate = DateTime.Now,
                        SumOrderInTime = 0,
                        SumOrderInLate = 0,
                        IdOrderInCare = null
                    };
                    MessageBox.Show($"Error loading delivery: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Implement add/update using BL (map CurrentDelivery -> DO/BO and call BL)
            MessageBox.Show($"Action: {ButtonText}\nDeliveryId: {CurrentDelivery?.DeliveryId}", "Add/Update", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Mutable view-model used for TwoWay bindings based on DeliveryInList properties
        public class DeliveryInListViewModel : INotifyPropertyChanged
        {
            private int _deliveryId;
            public int DeliveryId
            {
                get => _deliveryId;
                set { if (_deliveryId != value) { _deliveryId = value; OnPropertyChanged(nameof(DeliveryId)); } }
            }

            private string _fullName = string.Empty;
            public string FullName
            {
                get => _fullName;
                set { if (_fullName != value) { _fullName = value; OnPropertyChanged(nameof(FullName)); } }
            }

            private bool _isActive = true;
            public bool IsActive
            {
                get => _isActive;
                set { if (_isActive != value) { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }
            }

            private BO.ShippingType _shippingType = BO.ShippingType.Car;
            public BO.ShippingType ShippingType
            {
                get => _shippingType;
                set { if (_shippingType != value) { _shippingType = value; OnPropertyChanged(nameof(ShippingType)); } }
            }

            private DateTime? _employmentStartDate = DateTime.Now;
            public DateTime? EmploymentStartDate
            {
                get => _employmentStartDate;
                set { if (_employmentStartDate != value) { _employmentStartDate = value; OnPropertyChanged(nameof(EmploymentStartDate)); } }
            }

            private int _sumOrderInTime;
            public int SumOrderInTime
            {
                get => _sumOrderInTime;
                set { if (_sumOrderInTime != value) { _sumOrderInTime = value; OnPropertyChanged(nameof(SumOrderInTime)); } }
            }

            private int _sumOrderInLate;
            public int SumOrderInLate
            {
                get => _sumOrderInLate;
                set { if (_sumOrderInLate != value) { _sumOrderInLate = value; OnPropertyChanged(nameof(SumOrderInLate)); } }
            }

            private int? _idOrderInCare;
            public int? IdOrderInCare
            {
                get => _idOrderInCare;
                set { if (_idOrderInCare != value) { _idOrderInCare = value; OnPropertyChanged(nameof(IdOrderInCare)); } }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Converter: DeliveryId -> IsReadOnly (true when id != 0 => update mode)
    public class IdToReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // robust conversion: accept int, nullable int, string or other
            int id = 0;
            if (value is int i) id = i;
            else
            {
                var s = value?.ToString();
                if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var parsed)) id = parsed;
            }

            // non-zero DeliveryId -> make TextBox readonly (update mode)
            return id != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    // Converter: DeliveryId -> Visibility (Visible when id != 0, Collapsed otherwise)
    public class IdToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int id = 0;
            if (value is int i) id = i;
            else
            {
                var s = value?.ToString();
                if (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out var parsed)) id = parsed;
            }

            return id != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
