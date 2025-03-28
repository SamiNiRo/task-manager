using System;

namespace TaskManager.Models
{
    public class KanbanTask : ICloneable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public KanbanTaskStatus Status { get; set; }

        public object Clone()
        {
            return new KanbanTask
            {
                Id = this.Id,
                Title = this.Title,
                Description = this.Description,
                Status = this.Status
            };
        }
    }

    public enum KanbanTaskStatus
    {
        Backlog,
        Request,
        Selected,
        InProgress,
        Completed
    }
} 