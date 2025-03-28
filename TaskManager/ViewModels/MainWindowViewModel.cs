using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Views;

namespace TaskManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ITaskService _taskService;

        private ObservableCollection<KanbanTask> _backlogTasks;
        public ObservableCollection<KanbanTask> BacklogTasks
        {
            get => _backlogTasks;
            set => SetProperty(ref _backlogTasks, value);
        }

        private ObservableCollection<KanbanTask> _requestTasks;
        public ObservableCollection<KanbanTask> RequestTasks
        {
            get => _requestTasks;
            set => SetProperty(ref _requestTasks, value);
        }

        private ObservableCollection<KanbanTask> _selectedTasks;
        public ObservableCollection<KanbanTask> SelectedTasks
        {
            get => _selectedTasks;
            set => SetProperty(ref _selectedTasks, value);
        }

        private ObservableCollection<KanbanTask> _inProgressTasks;
        public ObservableCollection<KanbanTask> InProgressTasks
        {
            get => _inProgressTasks;
            set => SetProperty(ref _inProgressTasks, value);
        }

        private ObservableCollection<KanbanTask> _completedTasks;
        public ObservableCollection<KanbanTask> CompletedTasks
        {
            get => _completedTasks;
            set => SetProperty(ref _completedTasks, value);
        }

        public DelegateCommand AddBacklogTaskCommand { get; private set; }
        public DelegateCommand AddRequestTaskCommand { get; private set; }
        public DelegateCommand AddSelectedTaskCommand { get; private set; }
        public DelegateCommand AddInProgressTaskCommand { get; private set; }
        public DelegateCommand AddCompletedTaskCommand { get; private set; }
        public DelegateCommand<KanbanTask> EditTaskCommand { get; private set; }

        public MainWindowViewModel(ITaskService taskService)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));

            BacklogTasks = new ObservableCollection<KanbanTask>();
            RequestTasks = new ObservableCollection<KanbanTask>();
            SelectedTasks = new ObservableCollection<KanbanTask>();
            InProgressTasks = new ObservableCollection<KanbanTask>();
            CompletedTasks = new ObservableCollection<KanbanTask>();

            AddBacklogTaskCommand = new DelegateCommand(async () => await ExecuteAddTaskCommandAsync(KanbanTaskStatus.Backlog));
            AddRequestTaskCommand = new DelegateCommand(async () => await ExecuteAddTaskCommandAsync(KanbanTaskStatus.Request));
            AddSelectedTaskCommand = new DelegateCommand(async () => await ExecuteAddTaskCommandAsync(KanbanTaskStatus.Selected));
            AddInProgressTaskCommand = new DelegateCommand(async () => await ExecuteAddTaskCommandAsync(KanbanTaskStatus.InProgress));
            AddCompletedTaskCommand = new DelegateCommand(async () => await ExecuteAddTaskCommandAsync(KanbanTaskStatus.Completed));
            EditTaskCommand = new DelegateCommand<KanbanTask>(async (task) => await ExecuteEditTaskCommandAsync(task));

            _ = LoadTasksAsync();
        }

        private async Task ExecuteAddTaskCommandAsync(KanbanTaskStatus status)
        {
            try
            {
                var addTaskWindow = new AddTaskWindow
                {
                    Owner = Application.Current.MainWindow
                };

                if (addTaskWindow.ShowDialog() == true)
                {
                    var newTask = addTaskWindow.Task;
                    newTask.Status = status;
                    var addedTask = await _taskService.AddTaskAsync(newTask);
                    GetCollectionForStatus(status).Add(addedTask);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteEditTaskCommandAsync(KanbanTask task)
        {
            try
            {
                if (task == null) return;

                var editTaskWindow = new AddTaskWindow(task.Clone() as KanbanTask)
                {
                    Owner = Application.Current.MainWindow
                };

                if (editTaskWindow.ShowDialog() == true)
                {
                    if (editTaskWindow.IsDeleteRequested)
                    {
                        await DeleteTaskAsync(task);
                    }
                    else
                    {
                        var updatedTask = editTaskWindow.Task;
                        await _taskService.UpdateTaskAsync(updatedTask);
                        await LoadTasksAsync(); // Перезагружаем все задачи
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteTaskAsync(KanbanTask task)
        {
            try
            {
                await _taskService.DeleteTaskAsync(task.Id);
                GetCollectionForStatus(task.Status).Remove(task);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task MoveTaskAsync(KanbanTask task, KanbanTaskStatus newStatus)
        {
            if (task == null) return;

            try
            {
                // Создаем копию задачи для обновления
                var updatedTask = task.Clone() as KanbanTask;
                updatedTask.Status = newStatus;

                // Обновляем задачу в базе данных
                await _taskService.UpdateTaskAsync(updatedTask);

                // Удаляем задачу из исходной коллекции
                GetCollectionForStatus(task.Status).Remove(task);

                // Добавляем задачу в целевую коллекцию
                var targetCollection = GetCollectionForStatus(newStatus);
                targetCollection.Add(updatedTask);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при перемещении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                await LoadTasksAsync(); // Перезагружаем все задачи в случае ошибки
            }
        }

        private ObservableCollection<KanbanTask> GetCollectionForStatus(KanbanTaskStatus status)
        {
            return status switch
            {
                KanbanTaskStatus.Backlog => BacklogTasks,
                KanbanTaskStatus.Request => RequestTasks,
                KanbanTaskStatus.Selected => SelectedTasks,
                KanbanTaskStatus.InProgress => InProgressTasks,
                KanbanTaskStatus.Completed => CompletedTasks,
                _ => BacklogTasks
            };
        }

        private async Task LoadTasksAsync()
        {
            try
            {
                var backlogTasks = await _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Backlog);
                var requestTasks = await _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Request);
                var selectedTasks = await _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Selected);
                var inProgressTasks = await _taskService.GetTasksByStatusAsync(KanbanTaskStatus.InProgress);
                var completedTasks = await _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Completed);

                BacklogTasks = new ObservableCollection<KanbanTask>(backlogTasks);
                RequestTasks = new ObservableCollection<KanbanTask>(requestTasks);
                SelectedTasks = new ObservableCollection<KanbanTask>(selectedTasks);
                InProgressTasks = new ObservableCollection<KanbanTask>(inProgressTasks);
                CompletedTasks = new ObservableCollection<KanbanTask>(completedTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 