using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private bool _isDragging;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var listView = sender as ListView;
                    var item = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
                    if (item != null)
                    {
                        var task = item.DataContext as KanbanTask;
                        if (task != null)
                        {
                            _isDragging = true;
                            DragDrop.DoDragDrop(listView, task, DragDropEffects.Move);
                            _isDragging = false;
                        }
                    }
                }
            }
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            var task = e.Data.GetData(typeof(KanbanTask)) as KanbanTask;
            var targetListView = sender as ListView;
            var targetStatus = (KanbanTaskStatus)targetListView.Tag;

            if (task != null && task.Status != targetStatus)
            {
                var viewModel = DataContext as MainWindowViewModel;
                viewModel?.MoveTaskAsync(task, targetStatus);
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement)?.DataContext as KanbanTask;
            if (item != null)
            {
                var viewModel = DataContext as MainWindowViewModel;
                viewModel?.EditTaskCommand.Execute(item);
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
} 