using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PL
{
    /// <summary>
    /// Base class for windows that perform asynchronous network operations.
    /// Centralizes all error handling, visual display, and mouse management.
    /// </summary>
    public class NetworkAwareWindow : Window
    {
        // --- 1. Shared Variables (Dependency Properties) ---

        public string AddressStatus
        {
            get { return (string)GetValue(AddressStatusProperty); }
            set { SetValue(AddressStatusProperty, value); }
        }
        public static readonly DependencyProperty AddressStatusProperty =
            DependencyProperty.Register("AddressStatus", typeof(string), typeof(NetworkAwareWindow), new PropertyMetadata(null));

        public Brush AddressBorderBrush
        {
            get { return (Brush)GetValue(AddressBorderBrushProperty); }
            set { SetValue(AddressBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty AddressBorderBrushProperty =
            DependencyProperty.Register("AddressBorderBrush", typeof(Brush), typeof(NetworkAwareWindow), new PropertyMetadata(Brushes.Gray));


        // --- 2. A helper function that does all the dirty work ---

        /// <summary>
        /// Executes an asynchronous action with visual feedback.
        /// Returns true if successful, false otherwise.
        /// </summary>
        protected async Task<bool> ExecuteNetworkActionAsync(Func<Task> action, string loadingMessage, string successMessage)
        {
            try
            {
                // Start: Visual indication
                Mouse.OverrideCursor = Cursors.Wait;
                AddressStatus = loadingMessage;
                AddressBorderBrush = Brushes.Orange;

                // Perform the specific action passed as a parameter
                await action();

                // Success
                AddressStatus = successMessage;
                AddressBorderBrush = Brushes.Green;

                return true; // Action completed successfully
            }
            catch (BO.BlNetworkException ex)
            {
                // Handle network errors (Red border)
                AddressStatus = $"Network Error: {ex.Message}";
                AddressBorderBrush = Brushes.Red;

                // Brief delay to allow the UI to render the red border before the MessageBox blocks it
                await Task.Delay(50);

                MessageBox.Show(AddressStatus, "Network Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (BO.BlInvalidDataException ex) when (ex.Message.Contains("address") || ex.Message.Contains("coordinates") || ex.Message.Contains("found"))
            {
                // Handle invalid address errors (Red border)
                AddressStatus = "Invalid Address: Could not find coordinates.";
                AddressBorderBrush = Brushes.Red;

                // Brief delay to allow the UI to render the red border before the MessageBox blocks it
                await Task.Delay(50);

                MessageBox.Show(AddressStatus, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                // Handle unexpected errors (Gray border)
                AddressBorderBrush = Brushes.Gray;

                await Task.Delay(50);

                MessageBox.Show($"System Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                // Always restore the cursor
                Mouse.OverrideCursor = null;
            }
        }
    }
}