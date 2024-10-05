using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;

public class my
{
    private string _siteDir = Path.Combine(Directory.GetCurrentDirectory(), "Views");
    private ITaskService _taskService;

    public my(ITaskService taskService) 
    {
        _taskService = taskService;
    }

    public void Start()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:7777/");
        listener.Start();

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            Process(context);
        }
    }

    private void Process(HttpListenerContext context)
    {
        string fileName = context.Request.RawUrl.TrimStart('/');
        string content = "";

        if (context.Request.HttpMethod == "POST" && fileName.Equals("addTask.html", StringComparison.OrdinalIgnoreCase))
        {
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                var json = reader.ReadToEnd();
                var newTask = JsonConvert.DeserializeObject<TaskItem>(json);
                _taskService.AddTask(newTask);
                content = BuildToDoListHtml(); 
            }
        }
        else if (fileName.Equals("taskDetail.html", StringComparison.OrdinalIgnoreCase))
        {
            var query = context.Request.QueryString;
            int taskId = int.Parse(query["taskId"]);
            content = BuildTaskDetailHtml(taskId);
        }
        else
        {
            content = BuildToDoListHtml();
        }

        byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(content);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = htmlBytes.Length;
        context.Response.OutputStream.Write(htmlBytes, 0, htmlBytes.Length);
        context.Response.Close();
    }

    private string BuildToDoListHtml()
    {
        string layoutPath = Path.Combine(_siteDir, "Shared", "layout.html");
        string filePath = Path.Combine(_siteDir, "Pages", "toDoList.html");

        var razorService = Engine.Razor;

        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));

        if (!razorService.IsTemplateCached("toDoList", null))
        {
            razorService.AddTemplate("toDoList", File.ReadAllText(filePath));
            razorService.Compile("toDoList");
        }

        string html = razorService.Run("toDoList", null, new { Tasks = _taskService.GetTasks() });

        return html;
    }

    private string BuildTaskDetailHtml(int taskId)
    {
        var task = _taskService.GetTasks().FirstOrDefault(t => t.ID == taskId);
        string layoutPath = Path.Combine(_siteDir, "Shared", "layout.html");
        string filePath = Path.Combine(_siteDir, "Pages", "taskDetail.html");

        var razorService = Engine.Razor;

        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));

        if (!razorService.IsTemplateCached("taskDetail", null))
        {
            razorService.AddTemplate("taskDetail", File.ReadAllText(filePath));
            razorService.Compile("taskDetail");
        }

        string html = razorService.Run("taskDetail", null, new { Task = task });
        return html;
    }
}
