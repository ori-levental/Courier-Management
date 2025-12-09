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
        public DeliveryViewModel? CurrentDelivery
        {
            get => (DeliveryViewModel?)GetValue(CurrentDeliveryProperty);
            set => SetValue(CurrentDeliveryProperty, value);
        }
        public static readonly DependencyProperty CurrentDeliveryProperty =
            DependencyProperty.Register(nameof(CurrentDelivery), typeof(DeliveryViewModel), typeof(DeliveryWindow), new PropertyMetadata(null));

        // Helper collections for ComboBoxes (read-only)
        public IEnumerable<BO.ShippingType> DeliveryTypes { get; }
        public IEnumerable<BO.ShipmentCompletionStatus> DeliveryEndTypes { get; }

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
            DeliveryEndTypes = Enum.GetValues(typeof(BO.ShipmentCompletionStatus)).Cast<BO.ShipmentCompletionStatus>();

            // Set Button text BEFORE the view loads
            ButtonText = id == 0 ? "Add" : "Update";

            InitializeComponent();

            // DataContext is set in XAML to Self (RelativeSource Mode=Self)
            // Now assign the dependency property CurrentDelivery AFTER InitializeComponent,
            // per your instruction — create new view-model for Add mode or load existing for Update mode.
            if (id == 0)
            {
                // Add mode: new mutable VM with default/empty values
                CurrentDelivery = new DeliveryViewModel
                {
                    DeliveryId = 0,
                    CourierId = null,
                    CourierName = string.Empty,
                    ShippingType = BO.ShippingType.Car,
                    DeliveryStartTime = DateTime.Now,
                    DeliveryEndType = null,
                    DeliveryEndTime = null
                };
            }
            else
            {
                try
                {
                    var doDelivery = s_dal.Delivery.Read(id);
                    if (doDelivery != null)
                    {
                        var courierName = s_dal.Courier.Read(doDelivery.CourierId)?.FullName ?? string.Empty;

                        CurrentDelivery = new DeliveryViewModel
                        {
                            DeliveryId = doDelivery.Id,
                            CourierId = doDelivery.CourierId,
                            CourierName = courierName,
                            ShippingType = (BO.ShippingType)doDelivery.DeliveryShippingType,
                            DeliveryStartTime = doDelivery.StartDeliveryTime,
                            DeliveryEndType = doDelivery.EndType.HasValue ? (BO.ShipmentCompletionStatus?)doDelivery.EndType.Value : null,
                            DeliveryEndTime = doDelivery.EndOrderTime
                        };
                    }
                    else
                    {
                        CurrentDelivery = new DeliveryViewModel
                        {
                            DeliveryId = id,
                            CourierId = null,
                            CourierName = string.Empty,
                            ShippingType = BO.ShippingType.Car,
                            DeliveryStartTime = DateTime.Now,
                            DeliveryEndType = null,
                            DeliveryEndTime = null
                        };
                        MessageBox.Show($"Delivery with ID {id} not found.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    CurrentDelivery = new DeliveryViewModel
                    {
                        DeliveryId = id,
                        CourierId = null,
                        CourierName = string.Empty,
                        ShippingType = BO.ShippingType.Car,
                        DeliveryStartTime = DateTime.Now,
                        DeliveryEndType = null,
                        DeliveryEndTime = null
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

        // Mutable view-model used for TwoWay bindings
        public class DeliveryViewModel : INotifyPropertyChanged
        {
            private int _deliveryId;
            public int DeliveryId
            {
                get => _deliveryId;
                set { if (_deliveryId != value) { _deliveryId = value; OnPropertyChanged(nameof(DeliveryId)); } }
            }

            private int? _courierId;
            public int? CourierId
            {
                get => _courierId;
                set { if (_courierId != value) { _courierId = value; OnPropertyChanged(nameof(CourierId)); } }
            }

            private string _courierName = string.Empty;
            public string CourierName
            {
                get => _courierName;
                set { if (_courierName != value) { _courierName = value; OnPropertyChanged(nameof(CourierName)); } }
            }

            private BO.ShippingType _shippingType = BO.ShippingType.Car;
            public BO.ShippingType ShippingType
            {
                get => _shippingType;
                set { if (_shippingType != value) { _shippingType = value; OnPropertyChanged(nameof(ShippingType)); } }
            }

            private DateTime _deliveryStartTime = DateTime.Now;
            public DateTime DeliveryStartTime
            {
                get => _deliveryStartTime;
                set { if (_deliveryStartTime != value) { _deliveryStartTime = value; OnPropertyChanged(nameof(DeliveryStartTime)); } }
            }

            private BO.ShipmentCompletionStatus? _deliveryEndType;
            public BO.ShipmentCompletionStatus? DeliveryEndType
            {
                get => _deliveryEndType;
                set { if (_deliveryEndType != value) { _deliveryEndType = value; OnPropertyChanged(nameof(DeliveryEndType)); } }
            }

            private DateTime? _deliveryEndTime;
            public DateTime? DeliveryEndTime
            {
                get => _deliveryEndTime;
                set { if (_deliveryEndTime != value) { _deliveryEndTime = value; OnPropertyChanged(nameof(DeliveryEndTime)); } }
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
