using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    // Главное окно приложения
    public partial class MainWindow : Window
    {
        // Точка начала перетаскивания
        private Point _startPoint;
        // Флаг, указывающий что идет перетаскивание
        private bool _isDragging;
        private KanbanTaskViewModel _draggedTask;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Обработчик начала перетаскивания задачи
        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            var listView = sender as ListView;
            var item = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (item != null)
            {
                _draggedTask = item.DataContext as KanbanTaskViewModel;
                _isDragging = true;
            }
        }

        // Обработчик движения мыши при перетаскивании
        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed && _draggedTask != null)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var listView = sender as ListView;
                    var dragData = new DataObject("KanbanTask", _draggedTask);
                    DragDrop.DoDragDrop(listView, dragData, DragDropEffects.Move);
                    _isDragging = false;
                    _draggedTask = null;
                }
            }
        }

        // Обработчик завершения перетаскивания
        private async void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("KanbanTask"))
            {
                var task = e.Data.GetData("KanbanTask") as KanbanTaskViewModel;
                var targetListView = sender as ListView;
                var targetStatus = (KanbanTaskStatus)targetListView.Tag;

                if (task != null && task.Status != targetStatus)
                {
                    var viewModel = DataContext as MainWindowViewModel;
                    await viewModel.MoveTaskAsync(task, targetStatus);
                }
            }
        }

        // Обработчик двойного клика по задаче
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            if (listView?.SelectedItem is KanbanTaskViewModel task)
            {
                var viewModel = DataContext as MainWindowViewModel;
                viewModel?.EditTaskCommand.Execute(task);
            }
        }

        // Поиск родительского элемента определенного типа
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