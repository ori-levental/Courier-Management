using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlApi;
using PL.Helpers;

namespace PL.Courier
{
    /// <summary>
    /// Interaction logic for CourierListWindow.xaml
    /// </summary>
    public partial class CourierListWindow : Window
    {
        #region BL Access
        // Access point to the Business Logic layer
        static readonly IBl s_bl = Factory.Get();
        #endregion

        #region Stage 7 - Observer Mutex
        private readonly ObserverMutex _courierListMutex = new(); //stage 7
        #endregion

        #region Properties

        // Data source for the ComboBox options. 
        // Must be a collection (IEnumerable), not a single value.
        public IEnumerable<BO.CourierInListEnum> SortOptions { get; set; }

        // --- Dependency Property: CourierList ---
        // Bound to the ListView in XAML to display the data.
        public IEnumerable<CourierInList> CourierList
        {
            get { return (IEnumerable<CourierInList>)GetValue(CourierListProperty); }
            set { SetValue(CourierListProperty, value); }
        }

        public static readonly DependencyProperty CourierListProperty =
            DependencyProperty.Register("CourierList", typeof(IEnumerable<CourierInList>), typeof(CourierListWindow));

        // --- Dependency Property: CurrentSort ---
        // Bound to the ComboBox SelectedItem to track the user's choice.
        public BO.CourierInListEnum CurrentSort
        {
            get { return (BO.CourierInListEnum)GetValue(CurrentSortProperty); }
            set { SetValue(CurrentSortProperty, value); }
        }

        public static readonly DependencyProperty CurrentSortProperty =
            DependencyProperty.Register("CurrentSort", typeof(BO.CourierInListEnum), typeof(CourierListWindow),
                new PropertyMetadata(BO.CourierInListEnum.Id)); // Set default sort to ID

        // --- Dependency Property: IsActiveFilter (The Real Data) ---
        // Logic: null = All, true = Active, false = Inactive
        public bool? IsActiveFilter
        {
            get { return (bool?)GetValue(IsActiveFilterProperty); }
            set { SetValue(IsActiveFilterProperty, value); }
        }

        public static readonly DependencyProperty IsActiveFilterProperty =
            DependencyProperty.Register("IsActiveFilter", typeof(bool?), typeof(CourierListWindow),
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
                queryCourierList();
            }
        }

        #endregion

        #region Constructor
        public CourierListWindow()
        {
            // Populate the SortOptions list from the Enum values before InitializeComponent
            SortOptions = Enum.GetValues(typeof(BO.CourierInListEnum)).Cast<BO.CourierInListEnum>();

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
        private void CbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => queryCourierList();

        /// <summary>
        /// Placeholder for opening the Add Courier window.
        /// </summary>
        private void BtnAddCourier_Click(object sender, RoutedEventArgs e) => MainWindow.SafeExec(() => new CourierWindow().Show());

        /// <summary>
        /// Double Click on a row opens the Courier Window in Update Mode
        /// </summary>
        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 1. Get the row that was clicked
            var row = sender as ListViewItem;

            // 2. Extract the data object (CourierInList)
            if (row?.Content is BO.CourierInList courier)
            {
                // 3. Open the window with the ID (Update Constructor)
                // Using MainWindow.SafeExec to handle errors safely
                try
                {
                    var win = new CourierWindow(courier.Id);
                    win.Owner = this;
                    win.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Deletes a courier after confirmation.
        /// Stage 7: Handles simulator blocking of CRUD operations.
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // 1. Retrieve the courier object from the button row
            if (sender is Button btn && btn.DataContext is BO.CourierInList courier)
            {
                // 2. Ask for confirmation
                if (MessageBox.Show($"Are you sure you want to delete {courier.FullName}?",
                                    "Delete Courier",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 3. Get Manager ID and Execute Delete
                        int managerId = s_bl.Admin.GetConfig().ManagerId;
                        s_bl.Courier.DeleteCourier(managerId, courier.Id);

                        // 4. Refresh List
                        queryCourierList();
                    }
                    catch (BO.BlNotNullableException ex) when (ex.Message.Contains("Simulator is running"))
                    {
                        // Stage 7: Operation blocked by simulator
                        MessageBox.Show("Operation not allowed while simulator is running.\nPlease stop the simulator first.", 
                            "Simulator Active", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            // Important: Stop the click from triggering the Row Double-Click event
            e.Handled = true;
        }
        #endregion

        #region Internal Logic & Observers

        /// <summary>
        /// Centralized method to fetch the list from BL based on current filters and sort.
        /// </summary>
        private void queryCourierList()
        {
            try
            {
                // Get the Manager ID from configuration (assuming Manager context)
                int managerId = s_bl.Admin.GetConfig().ManagerId;

                // Fetch the list from BL, passing BOTH the Filter (IsActiveFilter) and the Sort (CurrentSort)
                CourierList = s_bl.Courier.ListOfCourier(managerId, IsActiveFilter, CurrentSort);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Observer method triggered by BL when data changes.
        /// Implements Stage 7 multithreading support with ObserverMutex.
        /// </summary>
        private void courierListObserver()
        {
            #region Stage 7 (for multithreading)
            if (_courierListMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                queryCourierList();

                // After completing the work, check if a restart was requested
                if (await _courierListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    courierListObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        #endregion

        #region Lifecycle Events

        /// <summary>
        /// Called when the window is fully loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Load data immediately upon opening
            queryCourierList();

            // 2. Register for updates (Observer Pattern)
            s_bl.Courier.AddObserver(courierListObserver);
        }

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        private void Window_Closed(object? sender, EventArgs e) => s_bl.Courier.RemoveObserver(courierListObserver);
        // Unregister the observer to prevent memory leaks and errors

        #endregion
    }
}