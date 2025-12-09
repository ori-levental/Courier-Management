using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using BlApi;
using BO;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        // BL access
        static readonly IBl s_bl = BlApi.Factory.Get();

        // ButtonText DP
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(OrderWindow), new PropertyMetadata(string.Empty));
        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        // DependencyProperty that holds the mutable view-model instance shown on the screen
        public OrderInListViewModel? CurrentOrder
        {
            get => (OrderInListViewModel?)GetValue(CurrentOrderProperty);
            set => SetValue(CurrentOrderProperty, value);
        }
        public static readonly DependencyProperty CurrentOrderProperty =
            DependencyProperty.Register(nameof(CurrentOrder), typeof(OrderInListViewModel), typeof(OrderWindow), new PropertyMetadata(null));

        // Helper collections for ComboBoxes
        public IEnumerable<BO.OrderType> OrderTypes { get; }

        // expose static converter instances so XAML can reference them via x:Static
        public static readonly IValueConverter IdToReadOnlyKey = new IdToReadOnlyConverter();

        // parameterless ctor routes to main ctor
        public OrderWindow() : this(0) { }

        // id == 0 -> Add, otherwise Update
        public OrderWindow(int id = 0)
        {
            // prepare ComboSources (do this before InitializeComponent so XAML can use them)
            OrderTypes = Enum.GetValues(typeof(BO.OrderType)).Cast<BO.OrderType>();

            // Set Button text BEFORE the view loads
            ButtonText = id == 0 ? "Add" : "Update";

            InitializeComponent();

            // DataContext is set in XAML to Self (RelativeSource Mode=Self)
            // Now assign the dependency property CurrentOrder AFTER InitializeComponent
            if (id == 0)
            {
                // Add mode: new mutable VM with default/empty values
                CurrentOrder = new OrderInListViewModel
                {
                    OrderId = 0,
                    OrderingName = string.Empty,
                    PhoneNumber = string.Empty,
                    FullAddress = string.Empty,
                    Latitude = 0,
                    Longitude = 0,
                    OrderType = BO.OrderType.Standard,
                    IsHeavy = false,
                    Description = string.Empty,
                    AirDistance = 0
                };
            }
            else
            {
                try
                {
                    // Try to load from BL first
                    var orderDetails = s_bl.Order.OrderDetails(s_bl.Admin.GetConfig().ManagerId, id);
                    if (orderDetails != null)
                    {
                        CurrentOrder = new OrderInListViewModel
                        {
                            OrderId = orderDetails.Id,
                            OrderingName = orderDetails.OrderingName,
                            PhoneNumber = orderDetails.PhoneNumber,
                            FullAddress = orderDetails.FullAddress,
                            Latitude = orderDetails.Latitude,
                            Longitude = orderDetails.Longitude,
                            OrderType = orderDetails.OrderType,
                            IsHeavy = orderDetails.IsHeavy,
                            Description = orderDetails.Description,
                            AirDistance = orderDetails.AirDistance
                        };
                    }
                    else
                    {
                        CurrentOrder = new OrderInListViewModel
                        {
                            OrderId = id,
                            OrderingName = string.Empty,
                            PhoneNumber = string.Empty,
                            FullAddress = string.Empty,
                            Latitude = 0,
                            Longitude = 0,
                            OrderType = BO.OrderType.Standard,
                            IsHeavy = false,
                            Description = string.Empty,
                            AirDistance = 0
                        };
                        MessageBox.Show($"Order with ID {id} not found.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    CurrentOrder = new OrderInListViewModel
                    {
                        OrderId = id,
                        OrderingName = string.Empty,
                        PhoneNumber = string.Empty,
                        FullAddress = string.Empty,
                        Latitude = 0,
                        Longitude = 0,
                        OrderType = BO.OrderType.Standard,
                        IsHeavy = false,
                        Description = string.Empty,
                        AirDistance = 0
                    };
                    MessageBox.Show($"Error loading order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Implement add/update using BL (map CurrentOrder -> BO and call BL)
            MessageBox.Show($"Action: {ButtonText}\nOrderId: {CurrentOrder?.OrderId}", "Add/Update", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Mutable view-model used for TwoWay bindings based on Order properties
        public class OrderInListViewModel : INotifyPropertyChanged
        {
            private int _orderId;
            public int OrderId
            {
                get => _orderId;
                set { if (_orderId != value) { _orderId = value; OnPropertyChanged(nameof(OrderId)); } }
            }

            private string _orderingName = string.Empty;
            public string OrderingName
            {
                get => _orderingName;
                set { if (_orderingName != value) { _orderingName = value; OnPropertyChanged(nameof(OrderingName)); } }
            }

            private string _phoneNumber = string.Empty;
            public string PhoneNumber
            {
                get => _phoneNumber;
                set { if (_phoneNumber != value) { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); } }
            }

            private string _fullAddress = string.Empty;
            public string FullAddress
            {
                get => _fullAddress;
                set { if (_fullAddress != value) { _fullAddress = value; OnPropertyChanged(nameof(FullAddress)); } }
            }

            private double _latitude;
            public double Latitude
            {
                get => _latitude;
                set { if (_latitude != value) { _latitude = value; OnPropertyChanged(nameof(Latitude)); } }
            }

            private double _longitude;
            public double Longitude
            {
                get => _longitude;
                set { if (_longitude != value) { _longitude = value; OnPropertyChanged(nameof(Longitude)); } }
            }

            private BO.OrderType _orderType = BO.OrderType.Standard;
            public BO.OrderType OrderType
            {
                get => _orderType;
                set { if (_orderType != value) { _orderType = value; OnPropertyChanged(nameof(OrderType)); } }
            }

            private bool _isHeavy;
            public bool IsHeavy
            {
                get => _isHeavy;
                set { if (_isHeavy != value) { _isHeavy = value; OnPropertyChanged(nameof(IsHeavy)); } }
            }

            private string? _description;
            public string? Description
            {
                get => _description;
                set { if (_description != value) { _description = value; OnPropertyChanged(nameof(Description)); } }
            }

            private double _airDistance;
            public double AirDistance
            {
                get => _airDistance;
                set { if (_airDistance != value) { _airDistance = value; OnPropertyChanged(nameof(AirDistance)); } }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Converter: OrderId -> IsReadOnly (true when id != 0 => update mode)
    public class IdToReadOnlyConverter : IValueConverter
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

            // non-zero OrderId -> make TextBox readonly (update mode)
            return id != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
