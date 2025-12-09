using BlApi;
using BO;
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

namespace PL.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryInListWindow.xaml
    /// </summary>
    public partial class DeliveryInListWindow : Window
    {
        #region BL Access
        // Access point to the Business Logic layer
        static readonly IBl s_bl = Factory.Get();
        #endregion

        #region Properties

        // Data source for the ComboBox options. 
        // Must be a collection (IEnumerable), not a single value.
        public IEnumerable<BO.DeliveryInList> SortOptions { get; set; }

        // --- Dependency Property: DeliveryList ---
        // Bound to the ListView in XAML to display the data.
        public IEnumerable<BO.DeliveryInList> DeliveryList
        {
            get { return (IEnumerable<BO.DeliveryInList>)GetValue(DeliveryListProperty); }
            set { SetValue(DeliveryListProperty, value); }
        }

        public static readonly DependencyProperty DeliveryListProperty =
            DependencyProperty.Register("DeliveryList", typeof(IEnumerable<BO.DeliveryInList>), typeof(DeliveryInListWindow));

        // --- Dependency Property: CurrentSort ---
        // Bound to the ComboBox SelectedItem to track the user's choice.
        public BO.DeliveryInList CurrentSort
        {
            get { return (BO.DeliveryInList)GetValue(CurrentSortProperty); }
            set { SetValue(CurrentSortProperty, value); }
        }

        public static readonly DependencyProperty CurrentSortProperty =
            DependencyProperty.Register("CurrentSort", typeof(BO.DeliveryInList), typeof(DeliveryInListWindow),
                new PropertyMetadata(null)); // Set default sort to null (or use a default instance if needed)

        // --- Dependency Property: IsActiveFilter (The Real Data) ---
        // Logic: null = All, true = Active, false = Inactive
        public bool? IsActiveFilter
        {
            get { return (bool?)GetValue(IsActiveFilterProperty); }
            set { SetValue(IsActiveFilterProperty, value); }
        }

        public static readonly DependencyProperty IsActiveFilterProperty =
            DependencyProperty.Register("IsActiveFilter", typeof(bool?), typeof(DeliveryInListWindow),
                new PropertyMetadata(null)); // Default is null (Show All)


        // --- Wrapper Property: FilterUI (The Trick) ---
        // Handles the conversion logic between the UI CheckBox state and the Data Logic.
        // Binds to the CheckBox in XAML.
        public bool? FilterUI
        {
            get
            {
                // Convert Data -> UI
                // Data: True (Active)   -> UI: True (V)
                // Data: False (Inactive)-> UI: Null (X)
                // Data: Null (All)      -> UI: False (Empty)

                if (IsActiveFilter == true) return true;
                if (IsActiveFilter == false) return null;
                return false;
            }
            set
            {
                // Convert UI -> Data
                // UI: True (V)    -> Data: True (Active)
                // UI: Null (X)    -> Data: False (Inactive)
                // UI: False (Empty)-> Data: Null (All)

                if (value == true) IsActiveFilter = true;
                else if (value == null) IsActiveFilter = false;
                else IsActiveFilter = null;

                // Trigger data refresh immediately
                queryDeliveryList();
            }
        }

        #endregion

        #region Constructor
        public DeliveryInListWindow()
        {
            // Populate the SortOptions list from the deliveries before InitializeComponent
            SortOptions = s_bl.Courier.ListOfCourier(s_bl.Admin.GetConfig().ManagerId, null, null);

            InitializeComponent();

            // Register Lifecycle events manually to ensure they fire
            this.Loaded += Window_Loaded;
            this.Closed += Window_Closed;
        }
        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Triggered when the user changes the sort selection in the ComboBox.
        /// </summary>
        private void CbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => queryDeliveryList();

        /// <summary>
        /// Placeholder for opening the Add Delivery window.
        /// </summary>
        private void BtnAddDelivery_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Add Delivery Window - Coming Soon");

        #endregion

        #region Internal Logic & Observers

        /// <summary>
        /// Centralized method to fetch the list from BL based on current filters and sort.
        /// </summary>
        private void queryDeliveryList()
        {
            try
            {
                // Get the Manager ID from configuration (assuming Manager context)
                int managerId = s_bl.Admin.GetConfig().ManagerId;

                // Fetch the list from BL, passing the Filter (IsActiveFilter)
                DeliveryList = s_bl.Courier.ListOfCourier(managerId, IsActiveFilter, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Observer method triggered by BL when data changes.
        /// </summary>
        private void deliveryListObserver() =>
            // CRITICAL: BL events run on a background thread.
            // We must use the Dispatcher to update the UI thread safely.
            this.Dispatcher.Invoke(() =>
            {
                queryDeliveryList();
            });

        #endregion

        #region Lifecycle Events

        /// <summary>
        /// Called when the window is fully loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Load data immediately upon opening
            queryDeliveryList();

            // 2. Register for updates (Observer Pattern)
            s_bl.Courier.AddObserver(deliveryListObserver);
        }

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        private void Window_Closed(object? sender, EventArgs e) => s_bl.Courier.RemoveObserver(deliveryListObserver);
        // Unregister the observer to prevent memory leaks and errors

        #endregion
    }
}
