using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Models;

namespace TaskManager.Services
{
    // Реализация сервиса для работы с задачами (пока что хранит данные в памяти)
    public class TaskService : ITaskService
    {
        // Список всех задач
        private readonly List<KanbanTask> _tasks = new List<KanbanTask>();
        // Счетчик для генерации новых ID
        private int _nextId = 1;

        // Найти все задачи с указанным статусом
        public Task<List<KanbanTask>> GetTasksByStatusAsync(KanbanTaskStatus status)
        {
            return Task.FromResult(_tasks.FindAll(t => t.Status == status));
        }

        // Создать новую задачу и добавить её в список
        public Task<KanbanTask> AddTaskAsync(KanbanTask task)
        {
            task.Id = _nextId++;
            task.CreatedAt = DateTime.Now;
            _tasks.Add(task);
            return Task.FromResult(task);
        }

        // Обновить данные существующей задачи
        public Task UpdateTaskAsync(KanbanTask task)
        {
            var existingTask = _tasks.Find(t => t.Id == task.Id);
            if (existingTask != null)
            {
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Status = task.Status;
                existingTask.DueDate = task.DueDate;
                existingTask.Priority = task.Priority;
            }
            return Task.CompletedTask;
        }

        // Удалить задачу по её ID
        public Task DeleteTaskAsync(int taskId)
        {
            _tasks.RemoveAll(t => t.Id == taskId);
            return Task.CompletedTask;
        }
    }
} 