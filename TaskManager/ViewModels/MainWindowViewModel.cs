using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    // Основной класс для управления задачами в главном окне
    public class MainWindowViewModel : BindableBase
    {
        // Сервис для работы с задачами
        private readonly ITaskService _taskService;
        private readonly IDialogService _dialogService;

        // Список задач в бэклоге
        private ObservableCollection<KanbanTaskViewModel> _backlogTasks;
        public ObservableCollection<KanbanTaskViewModel> BacklogTasks
        {
            get => _backlogTasks;
            set => SetProperty(ref _backlogTasks, value);
        }

        // Список запрошенных задач
        private ObservableCollection<KanbanTaskViewModel> _requestTasks;
        public ObservableCollection<KanbanTaskViewModel> RequestTasks
        {
            get => _requestTasks;
            set => SetProperty(ref _requestTasks, value);
        }

        // Список выбранных задач
        private ObservableCollection<KanbanTaskViewModel> _selectedTasks;
        public ObservableCollection<KanbanTaskViewModel> SelectedTasks
        {
            get => _selectedTasks;
            set => SetProperty(ref _selectedTasks, value);
        }

        // Список задач в работе
        private ObservableCollection<KanbanTaskViewModel> _inProgressTasks;
        public ObservableCollection<KanbanTaskViewModel> InProgressTasks
        {
            get => _inProgressTasks;
            set => SetProperty(ref _inProgressTasks, value);
        }

        // Список выполненных задач
        private ObservableCollection<KanbanTaskViewModel> _completedTasks;
        public ObservableCollection<KanbanTaskViewModel> CompletedTasks
        {
            get => _completedTasks;
            set => SetProperty(ref _completedTasks, value);
        }

        private KanbanTaskViewModel _editingTask;
        public KanbanTaskViewModel EditingTask
        {
            get => _editingTask;
            set => SetProperty(ref _editingTask, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        // Команды для добавления задач в разные колонки
        public DelegateCommand AddBacklogTaskCommand { get; private set; }
        public DelegateCommand AddRequestTaskCommand { get; private set; }
        public DelegateCommand AddSelectedTaskCommand { get; private set; }
        public DelegateCommand AddInProgressTaskCommand { get; private set; }
        public DelegateCommand AddCompletedTaskCommand { get; private set; }
        public DelegateCommand<KanbanTaskViewModel> EditTaskCommand { get; private set; }
        public DelegateCommand<KanbanTaskViewModel> SaveTaskCommand { get; private set; }
        public DelegateCommand<KanbanTaskViewModel> CancelEditCommand { get; private set; }
        public DelegateCommand<KanbanTaskViewModel> DeleteTaskCommand { get; private set; }

        // Конструктор с инициализацией команд и загрузкой задач
        public MainWindowViewModel(ITaskService taskService, IDialogService dialogService)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            InitializeCollections();
            InitializeCommands();
        }

        private void InitializeCollections()
        {
            BacklogTasks = new ObservableCollection<KanbanTaskViewModel>();
            RequestTasks = new ObservableCollection<KanbanTaskViewModel>();
            SelectedTasks = new ObservableCollection<KanbanTaskViewModel>();
            InProgressTasks = new ObservableCollection<KanbanTaskViewModel>();
            CompletedTasks = new ObservableCollection<KanbanTaskViewModel>();
        }

        private void InitializeCommands()
        {
            AddBacklogTaskCommand = new DelegateCommand(() => CreateNewTask(KanbanTaskStatus.Backlog));
            AddRequestTaskCommand = new DelegateCommand(() => CreateNewTask(KanbanTaskStatus.Request));
            AddSelectedTaskCommand = new DelegateCommand(() => CreateNewTask(KanbanTaskStatus.Selected));
            AddInProgressTaskCommand = new DelegateCommand(() => CreateNewTask(KanbanTaskStatus.InProgress));
            AddCompletedTaskCommand = new DelegateCommand(() => CreateNewTask(KanbanTaskStatus.Completed));
            EditTaskCommand = new DelegateCommand<KanbanTaskViewModel>(StartEditingTask);
            SaveTaskCommand = new DelegateCommand<KanbanTaskViewModel>(async (task) => await SaveTaskAsync(task));
            CancelEditCommand = new DelegateCommand<KanbanTaskViewModel>(CancelEditing);
            DeleteTaskCommand = new DelegateCommand<KanbanTaskViewModel>(async (task) => await DeleteTaskAsync(task));
        }

        public async Task InitializeAsync()
        {
            await LoadTasksAsync();
        }

        private void CreateNewTask(KanbanTaskStatus status)
        {
            var newTask = new KanbanTask
            {
                Status = status,
                Title = "",
                Description = "",
                CreatedAt = DateTime.Now
            };
            var taskViewModel = new KanbanTaskViewModel(newTask) { IsEditing = true };
            GetCollectionForStatus(status).Insert(0, taskViewModel);
        }

        private void StartEditingTask(KanbanTaskViewModel task)
        {
            if (task == null) return;
            task.IsEditing = true;
        }

        private async Task SaveTaskAsync(KanbanTaskViewModel task)
        {
            try
            {
                if (task == null) return;

                if (string.IsNullOrWhiteSpace(task.Title))
                {
                    _dialogService.ShowWarning("Название задачи не может быть пустым");
                    return;
                }

                if (task.Id == 0)
                {
                    // Новая задача
                    var addedTask = await _taskService.AddTaskAsync(task.Model);
                    var collection = GetCollectionForStatus(task.Status);
                    var index = collection.IndexOf(task);
                    var newTaskViewModel = new KanbanTaskViewModel(addedTask);
                    collection[index] = newTaskViewModel;
                }
                else
                {
                    // Обновление существующей задачи
                    await _taskService.UpdateTaskAsync(task.Model);
                }

                task.IsEditing = false;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при сохранении задачи: {ex.Message}");
            }
        }

        private void CancelEditing(KanbanTaskViewModel task)
        {
            if (task == null) return;

            if (task.Id == 0) // Новая задача
            {
                var collection = GetCollectionForStatus(task.Status);
                collection.Remove(task);
            }
            else
            {
                task.IsEditing = false;
            }
        }

        private async Task DeleteTaskAsync(KanbanTaskViewModel task)
        {
            try
            {
                if (task == null || task.Id == 0) return;

                await _taskService.DeleteTaskAsync(task.Id);
                var collection = GetCollectionForStatus(task.Status);
                collection.Remove(task);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при удалении задачи: {ex.Message}");
            }
        }

        // Перемещение задачи в другую колонку
        public async Task MoveTaskAsync(KanbanTaskViewModel task, KanbanTaskStatus newStatus)
        {
            if (task == null) return;

            try
            {
                task.Status = newStatus;
                await _taskService.UpdateTaskAsync(task.Model);

                BacklogTasks.Remove(task);
                RequestTasks.Remove(task);
                SelectedTasks.Remove(task);
                InProgressTasks.Remove(task);
                CompletedTasks.Remove(task);

                GetCollectionForStatus(newStatus).Add(task);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при перемещении задачи: {ex.Message}");
                await LoadTasksAsync();
            }
        }

        // Получение коллекции задач по статусу
        private ObservableCollection<KanbanTaskViewModel> GetCollectionForStatus(KanbanTaskStatus status)
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

        // Загрузка всех задач из базы данных
        public async Task LoadTasksAsync()
        {
            try
            {
                var loadTasks = new[]
                {
                    _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Backlog),
                    _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Request),
                    _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Selected),
                    _taskService.GetTasksByStatusAsync(KanbanTaskStatus.InProgress),
                    _taskService.GetTasksByStatusAsync(KanbanTaskStatus.Completed)
                };

                var results = await Task.WhenAll(loadTasks);

                BacklogTasks = new ObservableCollection<KanbanTaskViewModel>(results[0].Select(t => new KanbanTaskViewModel(t)));
                RequestTasks = new ObservableCollection<KanbanTaskViewModel>(results[1].Select(t => new KanbanTaskViewModel(t)));
                SelectedTasks = new ObservableCollection<KanbanTaskViewModel>(results[2].Select(t => new KanbanTaskViewModel(t)));
                InProgressTasks = new ObservableCollection<KanbanTaskViewModel>(results[3].Select(t => new KanbanTaskViewModel(t)));
                CompletedTasks = new ObservableCollection<KanbanTaskViewModel>(results[4].Select(t => new KanbanTaskViewModel(t)));
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при загрузке задач: {ex.Message}");
            }
        }
    }
} 