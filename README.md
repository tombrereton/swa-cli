# Easy Static Web App & Azure Function Development
## Introduction
Running a full stack application locally can be tricky and awkward. In this article we show you how to get a Static Web App talking to an Azure Function locally, using the SWA CLI. The SWA CLI is a local developer tool for Azure Static Web Apps and some things it can do are:
- Serve static app assets, or proxy to your app dev server
- Serve API requests, or proxy to APIs running in Azure Functions Core Tools
- Emulate authentication and authorization
- Emulate Static Web Apps configuration, including routing and ACL roles
- Deploy your app to Azure Static Web Apps

 Once setup, all you need to type is:
```bash
swa start ./client --api-location ./api
```

This article is based off this video:
[Youtube Video](https://www.youtube.com/watch?v=TIh52zbi8Dk)
## Prerequisites
- Npm
- Homebrew
- Azure Cli (to deploy)

## Recommended
- Node 16
- Azure Function 4.x
[Azure Function Versions](https://docs.microsoft.com/en-au/azure/azure-functions/functions-reference-node?tabs=v2-v3-v4-export%2Cv2-v3-v4-done%2Cv2%2Cv2-log-custom-telemetry%2Cv2-accessing-request-and-response%2Cwindows-setting-the-node-version#node-version "Azure Function Versions")

## Install
### Functions Cli
```bash
brew tap azure/functions
brew install azure-functions-core-tools@4
# if upgrading on a machine that has 2.x or 3.x installed:
brew link --overwrite azure-functions-core-tools@4
```
[Azure Functions Cli](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cmacos%2Ccsharp%2Cportal%2Cbash "Azure Functions Core Tools")

### Static Web App (SWA) Cli
```bash
npm install -g @azure/static-web-apps-cli
```
[Static Web App Cli](https://azure.github.io/static-web-apps-cli/docs/use/install/)

## Host SWA Locally with Cli
### Create Directories
```bash
mkdir swa-test && cd swa-test
mkdir client && mkdir api 
```

### Create Basic Web Client
```bash
cd client && curl https://raw.githubusercontent.com/vuejs/vuejs.org/master/src/v2/examples/vue-20-todomvc/index.html -o index.html
```

### Serve Web Client Locally
```bash
cd .. #pwd == swa-test
swa start client
```

Go to the localhost address to see your basic todo website running e.g.   
[http://localhost:4280](http://localhost:4280)

## Connect to Api
### Create Azure Function
```bash
cd api
func init --worker-runtime dotnet
func new --name ToDo --template HttpTrigger
```

### Run Client & Api
```bash
cd .. #pwd == swa-test
swa start ./client --api-location ./api
```

Both the SWA and Azure Function should start running locally and you can view them on localhost.

To test the Azure Function use Postman (or an alternative) with
```bash
GET http://localhost:7071/api/todo?name=test
```

You should get a response similar to:
```bash
Hello, test. This HTTP triggered function executed successfully.
```
 
However, the client is not talking to the api yet so we need to make some changes.

### Update Client
f you manage to follow the code below you’ll have the client and api talking via the GET method and it will load in the ToDo when the client starts up. 

If you want to add the full Add, Edit, Delete, Update functionality (or just want to skip the code below) have a look at the full code at the repo:
[https://github.com/tombrereton/swa-cli](https://github.com/tombrereton/swa-cli)


```js
<script>
	API = "api/todo";
	HEADERS = {'Accept': 'application/json', 'Content-Type': 'application/json'};

...
 
// DELETE
//      var todoStorage = {
//        fetch: function() {
//          var todos = JSON.parse(localStorage.getItem(STORAGE_KEY) || "[]");
//          todos.forEach(function(todo, index) {
//            todo.id = index;
//          });
//          todoStorage.uid = todos.length;
//          return todos;
//        },
//        save: function(todos) {
//         localStorage.setItem(STORAGE_KEY, JSON.stringify(todos));
//        }
//      };

...

	var app = new Vue({
        // app initial state
        data: {
          todos: [],
          newTodo: "",
          editedTodo: null,
          visibility: "all"
        },

	 	created: function() {
	          fetch(API, {headers: HEADERS, method: "GET"})
	          .then(res => {
	            return res.json();
	          })
	          .then(res => {
	            this.todos = res == null ?  [] : res;
	          })
	    },

// DELETE
//        // watch todos change for localStorage persistence
//        watch: {
//          todos: {
//           handler: function(todos) {
//              todoStorage.save(todos);
//            },
//            deep: true
//          }
//        },

...

</script>

```

### Update Api
```cs
public static class ToDoHandler
	{
		static List<ToDo> _db = new List<ToDo>();
		static int _nextId = 1;

		static ToDoHandler()
		{
			_db.Add(new ToDo { Id = 1, Title = "Hello from Azure Function!", Completed = true });
			_db.Add(new ToDo { Id = 1, Title = "Hello, from the other side", Completed = false });

		}

		[FunctionName("Get")]
		public static async Task<IActionResult> Run(
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
	}

	internal class ToDo
	{
		public int Id { get; internal set; }
		public string Title { get; internal set; }
		public bool Completed { get; internal set; }
	}
```
