using BlApi;
using BO;
using PL.Courier;
using PL.Order;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PL
{
    /// <summary>
    /// Helper class to aggregate order statistics for the dashboard summary.
    /// </summary>
    public class OrderStats
    {
        public int OpenCount { get; set; }
        public int OnCareCount { get; set; }
        public int ProvidedCount { get; set; }
        public int RefusedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NetworkAwareWindow
    {
        // Access to the Business Layer via Factory
        static readonly IBl s_bl = Factory.Get();

        // Flag to prevent infinite loops during password sync
        private bool _isPasswordSyncing = false;

        #region Stage 7 - Observer Mutexes
        private readonly ObserverMutex _clockMutex = new(); //stage 7
        private readonly ObserverMutex _configMutex = new(); //stage 7
        private readonly ObserverMutex _orderMutex = new(); //stage 7
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // Register lifecycle event handlers
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        #region Dependency Properties

        // 1. Dependency Property for System Clock
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        // 2. Dependency Property for Configuration Object
        public BO.Config Configuration
        {
            get { return (BO.Config)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

        // 3. Dependency Property for Order Statistics
        public OrderStats OrderStatistics
        {
            get { return (OrderStats)GetValue(OrderStatisticsProperty); }
            set { SetValue(OrderStatisticsProperty, value); }
        }
        public static readonly DependencyProperty OrderStatisticsProperty =
            DependencyProperty.Register("OrderStatistics", typeof(OrderStats), typeof(MainWindow));

        // 4. Dependency Property for Simulator Interval (Stage 7)
        /// <summary>
        /// Defines the interval (in minutes) at which the clock advances during simulator operation.
        /// Allows the manager to control the speed of simulation progression.
        /// </summary>
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(MainWindow), 
                new PropertyMetadata(1)); // Default: 1 minute per simulation step

        // 5. Dependency Property for Simulator Running State (Stage 7 - NEW)
        /// <summary>
        /// Flag indicating whether the simulator is currently running.
        /// Used to control UI state and button behavior.
        /// </summary>
        public bool IsSimulatorRunning
        {
            get { return (bool)GetValue(IsSimulatorRunningProperty); }
            set { SetValue(IsSimulatorRunningProperty, value); }
        }
        public static readonly DependencyProperty IsSimulatorRunningProperty =
            DependencyProperty.Register("IsSimulatorRunning", typeof(bool), typeof(MainWindow), 
                new PropertyMetadata(false)); // Default: Not running

        #endregion

        #region Observers

        private void clockObserver()
        {
            #region Stage 7 (for multithreading)
            if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                CurrentTime = s_bl.Admin.GetClock();
                UpdateOrderStats();

                // After completing the work, check if a restart was requested
                if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    clockObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        private void configObserver()
        {
            #region Stage 7 (for multithreading)
            if (_configMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                var newConfig = s_bl.Admin.GetConfig();
                Configuration = newConfig;

                // Sync PasswordBox only if changed externally
                if (pbManagerPass.Password != newConfig.ManagerPassword)
                {
                    _isPasswordSyncing = true;
                    pbManagerPass.Password = newConfig.ManagerPassword;
                    _isPasswordSyncing = false;
                }

                // After completing the work, check if a restart was requested
                if (await _configMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    configObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        private void orderObserver()
        {
            #region Stage 7 (for multithreading)
            if (_orderMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                UpdateOrderStats();

                // After completing the work, check if a restart was requested
                if (await _orderMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    orderObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        #endregion

        #region Lifecycle Events

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initial data fetch
            CurrentTime = s_bl.Admin.GetClock();
            var config = s_bl.Admin.GetConfig();
            Configuration = config;

            // Sync initial password to PasswordBox
            if (config.ManagerPassword != null)
            {
                _isPasswordSyncing = true;
                pbManagerPass.Password = config.ManagerPassword;
                _isPasswordSyncing = false;
            }

            // Initialize Interval to default value
            Interval = 1;

            // Initialize simulator state as not running
            IsSimulatorRunning = false;
            
            // Ensure buttons are enabled on startup
            TxbInterval.IsEnabled = true;
            BtnAddMinute.IsEnabled = true;
            BtnAddHour.IsEnabled = true;
            BtnAddDay.IsEnabled = true;
            BtnAddMonth.IsEnabled = true;
            BtnAddYear.IsEnabled = true;
            BtnInitDB.IsEnabled = true;
            BtnResetDB.IsEnabled = true;

            UpdateOrderStats();

            // Subscribe to BL updates
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
            s_bl.Order.AddObserver(orderObserver);
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // Stage 7: Stop simulator before closing if it's running
            if (IsSimulatorRunning)
            {
                try
                {
                    s_bl.Admin.StopSimulator();
                    IsSimulatorRunning = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error stopping simulator during close: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
            s_bl.Order.RemoveObserver(orderObserver);
        }

        private void UpdateOrderStats()
        {
            try
            {
                var orders = s_bl.Order.ListOfOrder(s_bl.Admin.GetConfig().ManagerId, null, null, null);

                var stats = new OrderStats
                {
                    OpenCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.Open),
                    OnCareCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.OnCare),
                    ProvidedCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.Provided),
                    RefusedCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.Refused),
                    CancelledCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.Cancelled)
                };

                OrderStatistics = stats;
            }
            catch
            {
                OrderStatistics = new OrderStats();
            }
        }
        #endregion

        #region Buttons & Logic

        // Time Manipulation
        private void BtnAddMinute_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Minute));
        private void BtnAddHour_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Hour));
        private void BtnAddDay_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Day));
        private void BtnAddMonth_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Month));
        private void BtnAddYear_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Year));

        /// <summary>
        /// Toggles the simulator state: starts if stopped, stops if running.
        /// Stage 7: Manages simulator lifecycle and UI state synchronization.
        /// </summary>
        private void BtnToggleSimulator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsSimulatorRunning)
                {
                    // --- START SIMULATOR ---
                    
                    // Validate Interval value
                    if (Interval <= 0)
                    {
                        MessageBox.Show("Interval must be a positive number (minutes).", "Invalid Interval", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Start the simulator
                    s_bl.Admin.StartSimulator(Interval);
                    
                    // Update UI state AFTER successful start
                    IsSimulatorRunning = true;
                    
                    // Disable interval input while running
                    TxbInterval.IsEnabled = false;
                    
                    // Disable time manipulation buttons
                    BtnAddMinute.IsEnabled = false;
                    BtnAddHour.IsEnabled = false;
                    BtnAddDay.IsEnabled = false;
                    BtnAddMonth.IsEnabled = false;
                    BtnAddYear.IsEnabled = false;
                    
                    // Disable DB management buttons
                    BtnInitDB.IsEnabled = false;
                    BtnResetDB.IsEnabled = false;

                    MessageBox.Show($"Simulator started with interval: {Interval} minute(s).", "Simulator Started", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // --- STOP SIMULATOR ---
                    
                    s_bl.Admin.StopSimulator();
                    
                    // Update UI state AFTER successful stop
                    IsSimulatorRunning = false;
                    
                    // Re-enable interval input
                    TxbInterval.IsEnabled = true;
                    
                    // Re-enable time manipulation buttons
                    BtnAddMinute.IsEnabled = true;
                    BtnAddHour.IsEnabled = true;
                    BtnAddDay.IsEnabled = true;
                    BtnAddMonth.IsEnabled = true;
                    BtnAddYear.IsEnabled = true;
                    
                    // Re-enable DB management buttons
                    BtnInitDB.IsEnabled = true;
                    BtnResetDB.IsEnabled = true;

                    MessageBox.Show("Simulator stopped.", "Simulator Stopped", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling simulator: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Reset state on error
                IsSimulatorRunning = false;
                TxbInterval.IsEnabled = true;
                BtnAddMinute.IsEnabled = true;
                BtnAddHour.IsEnabled = true;
                BtnAddDay.IsEnabled = true;
                BtnAddMonth.IsEnabled = true;
                BtnAddYear.IsEnabled = true;
                BtnInitDB.IsEnabled = true;
                BtnResetDB.IsEnabled = true;
            }
        }

        private async void BtnInitDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Initialize DB? Existing data will be overwritten.",
                                "Confirm Initialization", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    // Using await directly against the BL
                    await s_bl.Admin.InitializeDBAsync();

                    // Update window
                    Configuration = s_bl.Admin.GetConfig();

                    // Sync password to display
                    _isPasswordSyncing = true;
                    pbManagerPass.Password = Configuration.ManagerPassword ?? "";
                    _isPasswordSyncing = false;

                    CurrentTime = s_bl.Admin.GetClock();
                    UpdateOrderStats();
                    MessageBox.Show("Database Initialized successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                finally { Mouse.OverrideCursor = null; }
            }
        }

        private async void BtnResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("RESET DB? All data will be lost forever!", "Critical Warning",
                                MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    // Using await directly against the BL
                    await s_bl.Admin.ResetDBAsync();

                    // Update window
                    Configuration = s_bl.Admin.GetConfig();

                    // Sync password to display
                    _isPasswordSyncing = true;
                    pbManagerPass.Password = Configuration.ManagerPassword ?? "";
                    _isPasswordSyncing = false;

                    CurrentTime = s_bl.Admin.GetClock();
                    UpdateOrderStats();
                    MessageBox.Show("Database Reset successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                finally { Mouse.OverrideCursor = null; }
            }
        }

        private async void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            // Update local logic (password)
            if (txtVisibleManagerPass.Visibility == Visibility.Visible)
                Configuration.ManagerPassword = txtVisibleManagerPass.Text;
            else
                Configuration.ManagerPassword = pbManagerPass.Password;

            // Using the generic function from the base class
            await ExecuteNetworkActionAsync(async () =>
            {
                // The specific logic of this window
                await s_bl.Admin.SetConfigAsync((BO.Config)Configuration);
                MessageBox.Show("Configuration updated successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            }, "Validating Company Address...", "Address Validated.");
        }
        private void BtnShowList_Click(object sender, RoutedEventArgs e) => SafeExec(OpenCourierList);
        private void BtnShowOrderList_Click(object sender, RoutedEventArgs e) => SafeExec(OpenOrderList);

        #endregion

        #region Password Handling

        private void PbManagerPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordSyncing)
            {
                Configuration.ManagerPassword = pbManagerPass.Password;
            }
        }

        private void BtnToggleManagerPass_Click(object sender, RoutedEventArgs e)
        {
            if (pbManagerPass.Visibility == Visibility.Visible)
            {
                txtVisibleManagerPass.Text = pbManagerPass.Password;
                pbManagerPass.Visibility = Visibility.Collapsed;
                txtVisibleManagerPass.Visibility = Visibility.Visible;
            }
            else
            {
                pbManagerPass.Password = txtVisibleManagerPass.Text;
                txtVisibleManagerPass.Visibility = Visibility.Collapsed;
                pbManagerPass.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Window Helpers

        public static void SafeExec(Action action)
        {
            try { action(); }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void OpenCourierList()
        {
            var openWindow = Application.Current.Windows.OfType<CourierListWindow>().FirstOrDefault();
            if (openWindow != null) { openWindow.Activate(); if (openWindow.WindowState == WindowState.Minimized) openWindow.WindowState = WindowState.Normal; }
            else { new CourierListWindow().Show(); }
        }

        private void OpenOrderList()
        {
            var openWindow = Application.Current.Windows.OfType<OrderListWindow>().FirstOrDefault();
            if (openWindow != null) { openWindow.Activate(); if (openWindow.WindowState == WindowState.Minimized) openWindow.WindowState = WindowState.Normal; }
            else { new OrderListWindow().Show(); }
        }

        private void CloseAllWindows()
        {
            List<Window> windowsToClose = [];
            foreach (Window window in Application.Current.Windows)
            {
                if (window != this) windowsToClose.Add(window);
            }
            foreach (var window in windowsToClose) window.Close();
        }

        #endregion
    }
}