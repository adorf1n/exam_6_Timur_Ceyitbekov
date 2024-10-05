using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;

public class HttpServer
{
    private string _siteDir = Path.Combine(Directory.GetCurrentDirectory(), "Views");
    private ITaskService _taskService;

    public HttpServer(ITaskService taskService)
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

        if (fileName.Equals("toDoList.html", StringComparison.OrdinalIgnoreCase))
        {
            content = BuildToDoListHtml();
        }
        else if (fileName.Equals("showText.html", StringComparison.OrdinalIgnoreCase) && context.Request.HttpMethod == "POST")
        {
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                var body = reader.ReadToEnd();
                var requestData = JsonConvert.DeserializeObject<RequestData>(body);
                content = $"<html><body><h1>{requestData.Text}</h1></body></html>";
            }
        }
        else
        {
            if (fileName.Contains("html"))
            {
                content = BuildHtml(fileName);
            }
            else if (File.Exists(fileName))
            {
                content = File.ReadAllText(fileName);
            }
        }

        byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(content);
        Stream fileStream = new MemoryStream(htmlBytes);

        context.Response.ContentType = "text/html";
        fileStream.CopyTo(context.Response.OutputStream);
        context.Response.Close();
    }

    private string BuildToDoListHtml()
    {
        var tasks = _taskService.GetTasks();
        string layoutPath = Path.Combine(_siteDir, "Pages", "toDoList.html");
        var razorService = Engine.Razor;

        if (!razorService.IsTemplateCached("toDoList", null))
        {
            razorService.AddTemplate("toDoList", File.ReadAllText(layoutPath));
        }

        string html = razorService.Run("toDoList", null, new { Tasks = tasks });
        return html;
    }

    private string BuildHtml(string filename)
    {
        string layoutPath = Path.Combine(_siteDir, "Shared", "layout.html");
        string filePath = Path.Combine(_siteDir, "Pages", filename);
        var razorService = Engine.Razor;

        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));

        if (!razorService.IsTemplateCached(filename, null))
        {
            razorService.AddTemplate(filename, File.ReadAllText(filePath));
        }

        string html = razorService.Run(filename, null, new { IndexTitle = "My Index Title" });
        return html;
    }
}

public class RequestData
{
    public string Text { get; set; }
}
