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
    public partial class OutfitsView : UserControl
    {
        private OutfitsViewModel? _previousViewModel;
        private System.Collections.Generic.HashSet<string> _expandedNodeIds = new System.Collections.Generic.HashSet<string>();

        public OutfitsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            // Unsubscribe from previous ViewModel
            if (_previousViewModel != null)
            {
                _previousViewModel.OutfitTreeItems.CollectionChanged -= OnOutfitTreeItemsChanged;
            }

            if (DataContext is OutfitsViewModel viewModel)
            {
                // Subscribe to collection changes
                viewModel.OutfitTreeItems.CollectionChanged += OnOutfitTreeItemsChanged;
                _previousViewModel = viewModel;
            }
        }

        private void OnOutfitTreeItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
            var treeView = this.FindControl<TreeView>("OutfitsTreeView");
            if (treeView == null) return;

            var treeViewItems = treeView.GetVisualDescendants().OfType<TreeViewItem>();
            foreach (var item in treeViewItems)
            {
                if (item.DataContext is OutfitTreeItemViewModel vm && item.IsExpanded)
                {
                    // Store a unique ID based on display text since we don't have a proper ID
                    _expandedNodeIds.Add(vm.DisplayText);
                }
            }
        }

        private void RestoreExpansionStates()
        {
            var treeView = this.FindControl<TreeView>("OutfitsTreeView");
            if (treeView == null) return;

            var treeViewItems = treeView.GetVisualDescendants().OfType<TreeViewItem>().ToList();

            foreach (var item in treeViewItems)
            {
                if (item.DataContext is OutfitTreeItemViewModel vm)
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
                e.Source is not ComboBox && e.Source is not TreeViewItem && e.Source is not CheckBox)
            {
                this.Focus();
            }
        }
    }
}
