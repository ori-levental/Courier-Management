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

        // --- תוספת: מונה לכישלונות (שהם גם פתוחים) ---
        public int FailedCount { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NetworkAwareWindow
    {
        static readonly IBl s_bl = Factory.Get();
        private bool _isPasswordSyncing = false;

        #region Stage 7 - Observer Mutexes
        private readonly ObserverMutex _clockMutex = new();
        private readonly ObserverMutex _configMutex = new();
        private readonly ObserverMutex _orderMutex = new();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        #region Dependency Properties

        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        public BO.Config Configuration
        {
            get { return (BO.Config)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

        public OrderStats OrderStatistics
        {
            get { return (OrderStats)GetValue(OrderStatisticsProperty); }
            set { SetValue(OrderStatisticsProperty, value); }
        }
        public static readonly DependencyProperty OrderStatisticsProperty =
            DependencyProperty.Register("OrderStatistics", typeof(OrderStats), typeof(MainWindow));

        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(MainWindow),
                new PropertyMetadata(1));

        public bool IsSimulatorRunning
        {
            get { return (bool)GetValue(IsSimulatorRunningProperty); }
            set { SetValue(IsSimulatorRunningProperty, value); }
        }
        public static readonly DependencyProperty IsSimulatorRunningProperty =
            DependencyProperty.Register("IsSimulatorRunning", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        #endregion

        #region Observers

        private void clockObserver()
        {
            if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired()) return;
            Dispatcher.BeginInvoke(async () =>
            {
                CurrentTime = s_bl.Admin.GetClock();
                UpdateOrderStats();
                if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested()) clockObserver();
            });
        }

        private void configObserver()
        {
            if (_configMutex.CheckAndSetLoadInProgressOrRestartRequired()) return;
            Dispatcher.BeginInvoke(async () =>
            {
                var newConfig = s_bl.Admin.GetConfig();
                Configuration = newConfig;
                if (pbManagerPass.Password != newConfig.ManagerPassword)
                {
                    _isPasswordSyncing = true;
                    pbManagerPass.Password = newConfig.ManagerPassword;
                    _isPasswordSyncing = false;
                }
                if (await _configMutex.UnsetLoadInProgressAndCheckRestartRequested()) configObserver();
            });
        }

        private void orderObserver()
        {
            if (_orderMutex.CheckAndSetLoadInProgressOrRestartRequired()) return;
            Dispatcher.BeginInvoke(async () =>
            {
                UpdateOrderStats();
                if (await _orderMutex.UnsetLoadInProgressAndCheckRestartRequested()) orderObserver();
            });
        }

        #endregion

        #region Lifecycle Events

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTime = s_bl.Admin.GetClock();
            var config = s_bl.Admin.GetConfig();
            Configuration = config;

            if (config.ManagerPassword != null)
            {
                _isPasswordSyncing = true;
                pbManagerPass.Password = config.ManagerPassword;
                _isPasswordSyncing = false;
            }

            Interval = 1;
            IsSimulatorRunning = false;
            UpdateOrderStats();

            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
            s_bl.Order.AddObserver(orderObserver);
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (IsSimulatorRunning)
            {
                try { s_bl.Admin.StopSimulator(); IsSimulatorRunning = false; }
                catch (Exception ex) { MessageBox.Show($"Error stopping simulator: {ex.Message}"); }
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
                    // Open: All orders with status Open (New + Failed + NotFound)
                    OpenCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.Open),

                    // OnCare: Currently delivering
                    OnCareCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.OnCare),

                    // Failed: Orders that are Open BUT have history (TotalDeliveries > 0)
                    // This is a subset of 'OpenCount', displayed additionally.
                    FailedCount = orders.Count(o => o.OrderStatus == BO.OrderStatus.Open && o.TotalDeliveries > 0),

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

        private void BtnAddMinute_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Minute));
        private void BtnAddHour_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Hour));
        private void BtnAddDay_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Day));
        private void BtnAddMonth_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Month));
        private void BtnAddYear_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Year));

        private void BtnToggleSimulator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsSimulatorRunning)
                {
                    if (Interval <= 0)
                    {
                        MessageBox.Show("Interval must be positive."); return;
                    }
                    s_bl.Admin.StartSimulator(Interval);
                    IsSimulatorRunning = true;
                    MessageBox.Show($"Simulator started (Interval: {Interval}m).", "Started", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    s_bl.Admin.StopSimulator();
                    IsSimulatorRunning = false;
                    MessageBox.Show("Simulator stopped.", "Stopped", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                IsSimulatorRunning = false;
            }
        }

        private void BtnOpenEntry_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
        }

        private async void BtnInitDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Initialize DB?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
 
                try
                {
                    // close all the windows
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                        {
                            window.Close();
                        }
                    }
                    Mouse.OverrideCursor = Cursors.Wait;
                    await s_bl.Admin.InitializeDBAsync();
                    Configuration = s_bl.Admin.GetConfig();
                    _isPasswordSyncing = true; pbManagerPass.Password = Configuration.ManagerPassword ?? ""; _isPasswordSyncing = false;
                    CurrentTime = s_bl.Admin.GetClock();
                    UpdateOrderStats();
                    MessageBox.Show("DB Initialized.", "Success");
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
                finally { Mouse.OverrideCursor = null; }
            }
        }

        private async void BtnResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("RESET DB?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                try
                {
                    // close all the windows
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                        {
                            window.Close();
                        }
                    }
                    Mouse.OverrideCursor = Cursors.Wait;
                    await s_bl.Admin.ResetDBAsync();
                    Configuration = s_bl.Admin.GetConfig();
                    _isPasswordSyncing = true; pbManagerPass.Password = Configuration.ManagerPassword ?? ""; _isPasswordSyncing = false;
                    CurrentTime = s_bl.Admin.GetClock();
                    UpdateOrderStats();
                    MessageBox.Show("DB Reset.", "Success");
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
                finally { Mouse.OverrideCursor = null; }
            }
        }

        private async void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            if (txtVisibleManagerPass.Visibility == Visibility.Visible) Configuration.ManagerPassword = txtVisibleManagerPass.Text;
            else Configuration.ManagerPassword = pbManagerPass.Password;

            await ExecuteNetworkActionAsync(async () =>
            {
                await s_bl.Admin.SetConfigAsync((BO.Config)Configuration);
                MessageBox.Show("Configuration saved!", "Success");
            }, "Validating...", "Validated.");
        }

        private void BtnShowList_Click(object sender, RoutedEventArgs e) => SafeExec(OpenCourierList);
        private void BtnShowOrderList_Click(object sender, RoutedEventArgs e) => SafeExec(OpenOrderList);

        #endregion

        #region Password Handling
        private void PbManagerPass_PasswordChanged(object sender, RoutedEventArgs e) { if (!_isPasswordSyncing) Configuration.ManagerPassword = pbManagerPass.Password; }
        private void BtnToggleManagerPass_Click(object sender, RoutedEventArgs e)
        {
            if (pbManagerPass.Visibility == Visibility.Visible) { txtVisibleManagerPass.Text = pbManagerPass.Password; pbManagerPass.Visibility = Visibility.Collapsed; txtVisibleManagerPass.Visibility = Visibility.Visible; }
            else { pbManagerPass.Password = txtVisibleManagerPass.Text; txtVisibleManagerPass.Visibility = Visibility.Collapsed; pbManagerPass.Visibility = Visibility.Visible; }
        }
        #endregion

        #region Window Helpers
        public static void SafeExec(Action action) { try { action(); } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); } }
        private void OpenCourierList() { new CourierListWindow().Show(); }
        private void OpenOrderList() { new OrderListWindow().Show(); }
        #endregion
    }
}