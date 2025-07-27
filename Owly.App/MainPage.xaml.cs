using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Owly.App
{
    public class TabItem : INotifyPropertyChanged
    {
        private string _title;
        private string _content;
        private bool _isSelected;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainPage : ContentPage
    {
        public ObservableCollection<TabItem> Tabs { get; set; }
        private int _tabCounter = 1;
        private Dictionary<TabItem, Grid> _tabControls = new Dictionary<TabItem, Grid>();

        public MainPage()
        {
            InitializeComponent();
            InitializeTabs();
            BindingContext = this;
            
            // Set up scroll event handler
            tabScrollView.Scrolled += TabScrollView_Scrolled;
        }

        private void TabScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            UpdateArrowButtonStates();
        }

        private void InitializeTabs()
        {
            Tabs = new ObservableCollection<TabItem>
            {
                new TabItem 
                { 
                    Title = "Overview", 
                    Content = "Welcome to the Owly application. This is the overview tab where you can see a summary of your data.",
                    IsSelected = true
                },
                new TabItem 
                { 
                    Title = "Details", 
                    Content = "This tab shows detailed information about the selected items from the tree view."
                },
                new TabItem 
                { 
                    Title = "Settings", 
                    Content = "Configure your application settings and preferences here."
                }
            };

            // Create initial tab controls
            foreach (var tab in Tabs)
            {
                CreateTabControl(tab);
            }

            // Set up collection changed event
            Tabs.CollectionChanged += Tabs_CollectionChanged;
        }

        private void Tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TabItem newTab in e.NewItems)
                {
                    CreateTabControl(newTab);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TabItem removedTab in e.OldItems)
                {
                    RemoveTabControl(removedTab);
                }
            }
        }

        private void CreateTabControl(TabItem tab)
        {
            var tabGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Auto } } };

            var tabButton = new Button
            {
                Text = tab.Title,
                BackgroundColor = tab.IsSelected ? Colors.White : Colors.Transparent,
                TextColor = tab.IsSelected ? Colors.Black : Colors.Gray,
                BorderWidth = 0,
                Padding = new Thickness(12, 8),
                BindingContext = tab
            };
            tabButton.Clicked += OnTabClicked;

            var closeButton = new Button
            {
                Text = "×",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Gray,
                BorderWidth = 0,
                Padding = new Thickness(4, 8),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                BindingContext = tab
            };
            closeButton.Clicked += OnTabCloseClicked;

            // Only show close button if there's more than one tab
            closeButton.IsVisible = Tabs.Count > 1;

            Grid.SetColumn(tabButton, 0);
            Grid.SetColumn(closeButton, 1);

            tabGrid.Children.Add(tabButton);
            tabGrid.Children.Add(closeButton);

            tabContainer.Children.Add(tabGrid);
            _tabControls[tab] = tabGrid;

            // Subscribe to property changes
            tab.PropertyChanged += Tab_PropertyChanged;
        }

        private void RemoveTabControl(TabItem tab)
        {
            if (_tabControls.TryGetValue(tab, out var tabGrid))
            {
                tabContainer.Children.Remove(tabGrid);
                _tabControls.Remove(tab);
                tab.PropertyChanged -= Tab_PropertyChanged;
            }
        }

        private void Tab_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TabItem tab && _tabControls.TryGetValue(tab, out var tabGrid))
            {
                if (e.PropertyName == nameof(TabItem.IsSelected))
                {
                    var tabButton = tabGrid.Children.OfType<Button>().FirstOrDefault();
                    if (tabButton != null)
                    {
                        tabButton.BackgroundColor = tab.IsSelected ? Colors.White : Colors.Transparent;
                        tabButton.TextColor = tab.IsSelected ? Colors.Black : Colors.Gray;
                    }
                }
                else if (e.PropertyName == nameof(TabItem.Title))
                {
                    var tabButton = tabGrid.Children.OfType<Button>().FirstOrDefault();
                    if (tabButton != null)
                    {
                        tabButton.Text = tab.Title;
                    }
                }
            }
        }

        private void OnAddTabClicked(object sender, EventArgs e)
        {
            var newTab = new TabItem
            {
                Title = $"Tab {_tabCounter++}",
                Content = $"This is the content for Tab {_tabCounter - 1}. You can customize this content as needed."
            };

            // Deselect all other tabs
            foreach (var tab in Tabs)
            {
                tab.IsSelected = false;
            }

            // Select the new tab
            newTab.IsSelected = true;

            Tabs.Add(newTab);

            // Update close button visibility for all tabs
            UpdateCloseButtonVisibility();
        }

        private void OnTabClicked(object sender, EventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.BindingContext is TabItem clickedTab)
            {
                // Deselect all tabs
                foreach (var tab in Tabs)
                {
                    tab.IsSelected = false;
                }

                // Select the clicked tab
                clickedTab.IsSelected = true;
            }
        }

        private void OnTabCloseClicked(object sender, EventArgs e)
        {
            if (sender is Button closeButton && closeButton.BindingContext is TabItem tabToClose)
            {
                // Don't close if it's the last tab
                if (Tabs.Count <= 1)
                    return;

                // If we're closing the selected tab, select the previous one
                if (tabToClose.IsSelected)
                {
                    var index = Tabs.IndexOf(tabToClose);
                    var newSelectedIndex = index > 0 ? index - 1 : 0;
                    Tabs[newSelectedIndex].IsSelected = true;
                }

                Tabs.Remove(tabToClose);
                UpdateCloseButtonVisibility();
            }
        }

        private void UpdateCloseButtonVisibility()
        {
            foreach (var tabControl in _tabControls.Values)
            {
                var closeButton = tabControl.Children.OfType<Button>().Skip(1).FirstOrDefault();
                if (closeButton != null)
                {
                    closeButton.IsVisible = Tabs.Count > 1;
                }
            }
        }

        private void OnScrollLeftClicked(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (tabContainer.Children.Count > 0)
                    {
                        var currentScroll = tabScrollView.ScrollX;
                        var newScroll = Math.Max(0, currentScroll - 100);
                        await tabScrollView.ScrollToAsync(newScroll, 0, true);
                    }
                }
                catch
                {
                    // Silent fallback
                }
            });
        }

        private void OnScrollRightClicked(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (tabContainer.Children.Count > 0)
                    {
                        var currentScroll = tabScrollView.ScrollX;
                        var newScroll = currentScroll + 100;
                        await tabScrollView.ScrollToAsync(newScroll, 0, true);
                    }
                }
                catch
                {
                    // Silent fallback
                }
            });
        }

        private void UpdateArrowButtonStates()
        {
            try
            {
                var scrollX = tabScrollView.ScrollX;
                
                // Simple logic: disable left if at start, disable right if we can't scroll more
                leftArrowButton.IsEnabled = scrollX > 10;
                rightArrowButton.IsEnabled = true; // Always enabled for now
                
                // Visual feedback
                leftArrowButton.TextColor = leftArrowButton.IsEnabled ? Colors.Black : Colors.Gray;
                rightArrowButton.TextColor = rightArrowButton.IsEnabled ? Colors.Black : Colors.Gray;
            }
            catch
            {
                // Fallback
                leftArrowButton.IsEnabled = true;
                rightArrowButton.IsEnabled = true;
                leftArrowButton.TextColor = Colors.Black;
                rightArrowButton.TextColor = Colors.Black;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Update arrow states when page appears
            UpdateArrowButtonStates();
        }
    }

    // Value Converters
    public class TabSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? Colors.White : Colors.Transparent;
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TabTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? Colors.Black : Colors.Gray;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowCloseButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int tabCount)
            {
                return tabCount > 1; // Show close button only if there's more than one tab
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
