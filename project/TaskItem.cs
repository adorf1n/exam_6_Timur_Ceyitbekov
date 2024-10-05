// TaskItem.cs
using System;

public class TaskItem
{
    public int ID { get; set; }
    public string Title { get; set; }
    public string Assignee { get; set; }
    public string Description { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public TaskStatus Status { get; set; }
}
