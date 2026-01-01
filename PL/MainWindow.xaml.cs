using BlApi;
using BO;
using PL.Courier;
using PL.Order;
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
    public partial class MainWindow : Window
    {
        // Access to the Business Layer via Factory
        static readonly IBl s_bl = Factory.Get();

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

        // 3. Dependency Property for Order Statistics (New)
        public OrderStats OrderStatistics
        {
            get { return (OrderStats)GetValue(OrderStatisticsProperty); }
            set { SetValue(OrderStatisticsProperty, value); }
        }
        public static readonly DependencyProperty OrderStatisticsProperty =
            DependencyProperty.Register("OrderStatistics", typeof(OrderStats), typeof(MainWindow));

        #endregion

        #region Observers

        /// <summary>
        /// Observer method triggered when the BL Clock updates.
        /// Updates the UI Clock and refreshes order statistics (as statuses depend on time).
        /// </summary>
        private void clockObserver()
        {
            this.Dispatcher.Invoke(() =>
            {
                CurrentTime = s_bl.Admin.GetClock();
                UpdateOrderStats(); // Refresh stats on every tick
            });
        }

        /// <summary>
        /// Observer method triggered when Configuration updates.
        /// </summary>
        private void configObserver()
        {
            this.Dispatcher.Invoke(() =>
            {
                Configuration = s_bl.Admin.GetConfig();
            });
        }

        #endregion

        #region Lifecycle Events

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initial data fetch
            CurrentTime = s_bl.Admin.GetClock();
            Configuration = s_bl.Admin.GetConfig();
            UpdateOrderStats(); // Initial stats calculation

            // Subscribe to BL updates
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // Unsubscribe to prevent memory leaks
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        /// <summary>
        /// Fetches the list of orders and calculates the count per status.
        /// </summary>
        private void UpdateOrderStats()
        {
            try
            {
                var orders = s_bl.Order.ListOfOrder(s_bl.Admin.GetConfig().ManagerId, null, null, null);

                var stats = new OrderStats
                {
                    OpenCount = orders.Count(o => o.OrderStatus == BO.ShipmentCompletionStatus.Open),
                    OnCareCount = orders.Count(o => o.OrderStatus == BO.ShipmentCompletionStatus.OnCare),
                    ProvidedCount = orders.Count(o => o.OrderStatus == BO.ShipmentCompletionStatus.Provided),
                    RefusedCount = orders.Count(o => o.OrderStatus == BO.ShipmentCompletionStatus.Refused),
                    CancelledCount = orders.Count(o => o.OrderStatus == BO.ShipmentCompletionStatus.Cancelled)
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

        // Time Manipulation Buttons
        private void BtnAddMinute_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Minute));
        private void BtnAddHour_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Hour));
        private void BtnAddDay_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Day));
        private void BtnAddMonth_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Month));
        private void BtnAddYear_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Year));

        /// <summary>
        /// Initializes the Database with dummy data.
        /// </summary>
        private async void BtnInitDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Initialize DB? Existing data will be overwritten.",
                                "Confirm Initialization",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    CloseAllWindows();

                    // Run heavy task on background thread
                    await Task.Run(() => s_bl.Admin.InitializeDB());
                    await Task.Delay(1000); // Allow file I/O to complete

                    // Refresh UI
                    Configuration = s_bl.Admin.GetConfig();
                    CurrentTime = s_bl.Admin.GetClock();
                    UpdateOrderStats();

                    MessageBox.Show("Database Initialized successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during initialization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// Resets the Database (Deletes all data).
        /// </summary>
        private async void BtnResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("RESET DB? All data will be lost forever!",
                                "Critical Warning",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    CloseAllWindows();

                    await Task.Run(() => s_bl.Admin.ResetDB());
                    await Task.Delay(1000);

                    // Refresh UI
                    Configuration = s_bl.Admin.GetConfig();
                    CurrentTime = s_bl.Admin.GetClock();
                    UpdateOrderStats();

                    MessageBox.Show("Database Reset successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during reset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SafeExec(() =>
            {
                s_bl.Admin.SetConfig((BO.Config)Configuration);
                MessageBox.Show("Configuration updated successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void BtnShowList_Click(object sender, RoutedEventArgs e) => SafeExec(OpenCourierList);

        private void BtnShowOrderList_Click(object sender, RoutedEventArgs e) => SafeExec(OpenOrderList);

        #endregion

        #region Window Helpers

        /// <summary>
        /// Wrapper for safe execution of actions with global error handling.
        /// </summary>
        public static void SafeExec(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCourierList()
        {
            var openWindow = Application.Current.Windows.OfType<CourierListWindow>().FirstOrDefault();

            if (openWindow != null)
            {
                openWindow.Activate();
                if (openWindow.WindowState == WindowState.Minimized)
                    openWindow.WindowState = WindowState.Normal;
            }
            else
            {
                new CourierListWindow().Show();
            }
        }

        private void OpenOrderList()
        {
            var openWindow = Application.Current.Windows.OfType<OrderListWindow>().FirstOrDefault();

            if (openWindow != null)
            {
                openWindow.Activate();
                if (openWindow.WindowState == WindowState.Minimized)
                    openWindow.WindowState = WindowState.Normal;
            }
            else
            {
                new OrderListWindow().Show();
            }
        }

        /// <summary>
        /// Closes all auxiliary windows to prevent data inconsistency during DB operations.
        /// </summary>
        private void CloseAllWindows()
        {
            List<Window> windowsToClose = [];

            foreach (Window window in Application.Current.Windows)
            {
                if (window != this)
                {
                    windowsToClose.Add(window);
                }
            }

            foreach (var window in windowsToClose)
            {
                window.Close();
            }
        }

        #endregion
    }
}