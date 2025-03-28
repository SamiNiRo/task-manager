using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services
{
    public interface ITaskService
    {
        Task<List<KanbanTask>> GetTasksByStatusAsync(KanbanTaskStatus status);
        Task<KanbanTask> AddTaskAsync(KanbanTask task);
        Task UpdateTaskAsync(KanbanTask task);
        Task DeleteTaskAsync(int taskId);
    }

    public class TaskService : ITaskService
    {
        private readonly TaskManagerDbContext _context;

        public TaskService(TaskManagerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<KanbanTask>> GetTasksByStatusAsync(KanbanTaskStatus status)
        {
            try
            {
                return await _context.Tasks
                    .AsNoTracking()
                    .Where(t => t.Status == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении задач со статусом {status}: {ex.Message}", ex);
            }
        }

        public async Task<KanbanTask> AddTaskAsync(KanbanTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            try
            {
                var entry = await _context.Tasks.AddAsync(task);
                await _context.SaveChangesAsync();
                
                // Отсоединяем сущность от контекста после сохранения
                _context.Entry(task).State = EntityState.Detached;
                
                return task;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при добавлении задачи: {ex.Message}", ex);
            }
        }

        public async Task UpdateTaskAsync(KanbanTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            try
            {
                var existingTask = await _context.Tasks.FindAsync(task.Id);
                if (existingTask != null)
                {
                    _context.Entry(existingTask).CurrentValues.SetValues(task);
                    await _context.SaveChangesAsync();
                    _context.Entry(existingTask).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении задачи: {ex.Message}", ex);
            }
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task != null)
                {
                    _context.Tasks.Remove(task);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении задачи: {ex.Message}", ex);
            }
        }
    }
} 