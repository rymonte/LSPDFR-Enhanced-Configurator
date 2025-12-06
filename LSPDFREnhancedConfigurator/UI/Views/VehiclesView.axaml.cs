using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class VehiclesView : UserControl
    {
        private VehiclesViewModel? _previousViewModel;
        private System.Collections.Generic.HashSet<string> _expandedNodeIds = new System.Collections.Generic.HashSet<string>();

        public VehiclesView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            // Unsubscribe from previous ViewModel
            if (_previousViewModel != null)
            {
                _previousViewModel.VehicleTreeItems.CollectionChanged -= OnVehicleTreeItemsChanged;
            }

            if (DataContext is VehiclesViewModel viewModel)
            {
                // Subscribe to collection changes
                viewModel.VehicleTreeItems.CollectionChanged += OnVehicleTreeItemsChanged;
                _previousViewModel = viewModel;
            }
        }

        private void OnVehicleTreeItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Before collection changes, save expansion states
            if (e.Action == NotifyCollectionChangedAction.Reset || e.Action == NotifyCollectionChangedAction.Remove)
            {
                SaveExpansionStates();
            }

            // Restore expansion states after UI updates
            Dispatcher.UIThread.Post(() => RestoreExpansionStates(), DispatcherPriority.Background);
        }

        private void SaveExpansionStates()
        {
            _expandedNodeIds.Clear();
            var treeView = this.FindControl<TreeView>("VehiclesTreeView");
            if (treeView == null) return;

            var treeViewItems = treeView.GetVisualDescendants().OfType<TreeViewItem>();
            foreach (var item in treeViewItems)
            {
                if (item.DataContext is VehicleTreeItemViewModel vm && item.IsExpanded)
                {
                    // Store a unique ID based on display text since we don't have a proper ID
                    _expandedNodeIds.Add(vm.DisplayText);
                }
            }
        }

        private void RestoreExpansionStates()
        {
            var treeView = this.FindControl<TreeView>("VehiclesTreeView");
            if (treeView == null) return;

            var treeViewItems = treeView.GetVisualDescendants().OfType<TreeViewItem>().ToList();

            foreach (var item in treeViewItems)
            {
                if (item.DataContext is VehicleTreeItemViewModel vm)
                {
                    // Restore expansion state based on display text
                    bool shouldExpand = _expandedNodeIds.Contains(vm.DisplayText);
                    item.IsExpanded = shouldExpand;
                    vm.IsExpanded = shouldExpand;
                }
            }
        }

        private void OnBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Clear focus when clicking outside interactive elements
            if (e.Source is not TextBox && e.Source is not NumericUpDown &&
                e.Source is not ComboBox && e.Source is not ListBoxItem && e.Source is not CheckBox)
            {
                this.Focus();
            }
        }
    }
}
