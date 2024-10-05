using System.Collections.Generic;

public interface ITaskService
{
    void AddTask(TaskItem task);
    void SaveTasks(List<TaskItem> tasks);
    List<TaskItem> GetTasks();
}
