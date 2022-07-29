using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace api
{

	public static class ToDoHandler
	{
		static List<ToDo> _db = new List<ToDo>();
		static int _nextId = 3;

		static ToDoHandler()
		{
			_db.Add(new ToDo { Id = 1, Title = "Hello from Azure Function!", Completed = true });
			_db.Add(new ToDo { Id = 2, Title = "Hello, from the other side", Completed = false });

		}

		[FunctionName("Get")]
		[ActionName(nameof(Get))]
		public static async Task<IActionResult> Get(
				[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id:int?}")] HttpRequest req,
				ILogger log,
								int? id)
		{
			if (id == null)
				return new OkObjectResult(_db);

			var todoTask = _db.Find(i => i.Id == id);

			if (todoTask == null)
				return await Task.FromResult<IActionResult>(new NotFoundResult());

			return await Task.FromResult<IActionResult>(new OkObjectResult(todoTask));
		}

		[FunctionName("Post")]
		public static async Task<IActionResult> Post(
				[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
				ILogger log)
		{
			var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var data = JsonConvert.DeserializeObject<ToDo>(requestBody);

			if (data == null)
				return await Task.FromResult<IActionResult>(new BadRequestObjectResult("ToDo not provided in correct format."));

			if (data.Id == 0)
			{
				data.Id = _nextId;
				_nextId++;
			}

			_db.Add(data);
			return new CreatedAtActionResult(nameof(Get), nameof(ToDoHandler), new { id = data.Id }, data);
		}

		[FunctionName("Patch")]
		public static async Task<IActionResult> Patch([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "todo/{id}")] HttpRequest req,
				ILogger log, int id)
		{
			var todoTask = _db.Find(i => i.Id == id);

			if (todoTask == null)
				return await Task.FromResult<IActionResult>(new NotFoundResult());

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			ToDo data = JsonConvert.DeserializeObject<ToDo>(requestBody);

			todoTask.Title = data.Title ?? todoTask.Title;
			todoTask.Completed = data.Completed != false ? data.Completed : todoTask.Completed;

			return await Task.FromResult<IActionResult>(new OkObjectResult(todoTask));
		}

		[FunctionName("Put")]
		public static async Task<IActionResult> Put([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
				ILogger log, int id)
		{
			var todoTask = _db.Find(i => i.Id == id);

			if (todoTask == null)
				return await Task.FromResult<IActionResult>(new NotFoundResult());

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			ToDo data = JsonConvert.DeserializeObject<ToDo>(requestBody);

			todoTask.Title = data.Title ?? todoTask.Title;
			todoTask.Completed = data.Completed != false ? data.Completed : todoTask.Completed;

			return await Task.FromResult<IActionResult>(new OkObjectResult(todoTask));
		}

		[FunctionName("Delete")]
		public static async Task<IActionResult> Delete(
				[HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
				ILogger log,
				int id)
		{
			var todoTask = _db.Find(i => i.Id == id);

			if (todoTask == null)
				return await Task.FromResult<IActionResult>(new NotFoundResult());

			_db.Remove(todoTask);

			return await Task.FromResult<IActionResult>(new OkObjectResult(todoTask));
		}
	}

	public class ToDo
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public bool Completed { get; set; }
	}
}
