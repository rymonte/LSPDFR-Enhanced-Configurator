using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class RanksView : UserControl
    {
        public RanksView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private RanksViewModel? _previousViewModel;

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] DataContext changed. Previous: {_previousViewModel != null}, New: {DataContext is RanksViewModel}");

            // Unsubscribe from previous ViewModel
            if (_previousViewModel != null)
            {
                _previousViewModel.RankTreeItems.CollectionChanged -= OnRankTreeItemsChanged;
                LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] Unsubscribed from previous ViewModel");
            }

            if (DataContext is RanksViewModel viewModel)
            {
                // Subscribe to collection changes
                viewModel.RankTreeItems.CollectionChanged += OnRankTreeItemsChanged;
                _previousViewModel = viewModel;
                LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] Subscribed to new ViewModel, RankTreeItems count: {viewModel.RankTreeItems.Count}");
            }
        }

        private void OnRankTreeItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] OnRankTreeItemsChanged: Action={e.Action}");

            // Before collection changes, save expansion states
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                SaveExpansionStates();
            }

            // Restore expansion states after UI updates
            Dispatcher.UIThread.Post(() => RestoreExpansionStates(), DispatcherPriority.Background);
        }

        private System.Collections.Generic.HashSet<string> _expandedRankIds = new System.Collections.Generic.HashSet<string>();

        private void SaveExpansionStates()
        {
            _expandedRankIds.Clear();
            var treeViewItems = RanksTreeView.GetVisualDescendants().OfType<TreeViewItem>();
            foreach (var item in treeViewItems)
            {
                if (item.DataContext is RankTreeItemViewModel vm && item.IsExpanded)
                {
                    _expandedRankIds.Add(vm.Rank.Id);
                }
            }
        }

        private void RestoreExpansionStates()
        {
            var viewModel = DataContext as RanksViewModel;
            var treeViewItems = RanksTreeView.GetVisualDescendants().OfType<TreeViewItem>().ToList();

            LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] RestoreExpansionStates: {treeViewItems.Count} tree items found, {_expandedRankIds.Count} previously expanded");
            LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] LastAffectedRankId: {viewModel?.LastAffectedRankId?.Substring(0, Math.Min(8, viewModel?.LastAffectedRankId?.Length ?? 0)) ?? "null"}");

            int expandedCount = 0;
            foreach (var item in treeViewItems)
            {
                if (item.DataContext is RankTreeItemViewModel vm)
                {
                    // Expand if it was previously expanded OR if it's the last affected rank (from Undo/Redo)
                    bool shouldExpand = _expandedRankIds.Contains(vm.Rank.Id);

                    // Also expand the last affected rank after Undo/Redo operations
                    if (viewModel?.LastAffectedRankId != null && vm.Rank.Id == viewModel.LastAffectedRankId)
                    {
                        shouldExpand = true;
                        LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] Force-expanding '{vm.Rank.Name}' due to LastAffectedRankId match");
                    }

                    if (shouldExpand) expandedCount++;
                    item.IsExpanded = shouldExpand;
                }
            }

            LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] Expanded {expandedCount} items");
        }

        private void OnBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Only commit changes when clicking on background, but don't deselect tree items
            // Tree items should only be deselected when clicking in the TreeView's empty space
            if (e.Source is not TextBox && e.Source is not NumericUpDown)
            {
                // Commit any pending changes before clearing focus
                if (DataContext is RanksViewModel viewModel)
                {
                    viewModel.CommitChanges();
                }

                this.Focus();
            }
        }

        private void OnTreeViewPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Only deselect if clicking on the TreeView itself (empty space), not on a TreeViewItem
            if (e.Source is TreeView)
            {
                if (DataContext is RanksViewModel viewModel)
                {
                    viewModel.CommitChanges();
                    viewModel.SelectedTreeItem = null;
                }
            }
        }

        private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
        {
            // Trigger validation update first
            if (DataContext is RanksViewModel viewModel)
            {
                viewModel.OnFieldLostFocus();
                viewModel.CommitChanges();
            }
        }

        private void OnTreeItemDoubleTapped(object? sender, TappedEventArgs e)
        {
            // Get the TextBlock that was double-clicked
            if (sender is not TextBlock textBlock)
                return;

            // Get the DataContext (RankTreeItemViewModel)
            if (textBlock.DataContext is not RankTreeItemViewModel treeItem)
                return;

            // Only toggle expansion if this item has children (pay bands)
            if (!treeItem.HasChildren)
                return;

            // Find the TreeViewItem container for this data item
            var treeViewItem = FindTreeViewItemForData(RanksTreeView, treeItem);
            if (treeViewItem != null)
            {
                // Toggle the TreeViewItem's IsExpanded property
                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;

                // Also update the ViewModel to keep them in sync
                treeItem.IsExpanded = treeViewItem.IsExpanded;

                LSPDFREnhancedConfigurator.Services.Logger.Trace($"[View] Double-clicked '{treeItem.Rank.Name}', toggled expansion to {treeViewItem.IsExpanded}");
            }

            // Mark the event as handled to prevent it from bubbling
            e.Handled = true;
        }

        private TreeViewItem? FindTreeViewItemForData(TreeView treeView, RankTreeItemViewModel data)
        {
            // Search through visual tree to find TreeViewItem with matching DataContext
            var treeViewItems = treeView.GetVisualDescendants().OfType<TreeViewItem>();
            foreach (var item in treeViewItems)
            {
                if (item.DataContext == data)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
