using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class TaskService : ITaskService
{
    private const string FilePath = "tasks.json";

    public void AddTask(TaskItem task)
    {
        var tasks = GetTasks();
        task.ID = tasks.Count > 0 ? tasks.Max(t => t.ID) + 1 : 1; 
        tasks.Add(task);
        SaveTasks(tasks);
    }

    public List<TaskItem> GetTasks()
    {
        if (!File.Exists(FilePath))
        {
            return new List<TaskItem>();
        }

        var json = File.ReadAllText(FilePath);
        return JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
    }

    public void SaveTasks(List<TaskItem> tasks)
    {
        var json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }
}
