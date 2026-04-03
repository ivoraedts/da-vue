# Da-Vue

## About Da-Vue

It was about time to play around a bit with Vue. And by adding Da in front, it sounds like a Vietnames city. 🇻🇳 - Like Da Lat, which I visited in 2014. Just for fun and for reminding that Vietnam was a very interesting country to see and experience.

## Combining the following topics
- Vue.JS
- TypeScript
- Material Styling (via Vuetify)
- .NET 10 of ASP.NET API
- Docker
- Synology

## Getting Started

After creating a root-folder, the idea was to combine a VUE.JS project with a dot-net back-end. And then later get it to work together first local, then with docker, etc.

### Vue.JS via Vuetify

As I like working on front-end stuff with Visual Studio Code, I initially found a [tutorial](https://code.visualstudio.com/docs/nodejs/vuejs-tutorial) for getting started with Vue. So after installing Node.js, you could just do the scaffolding via the Vite tooling to get the default Vue application. However the result does not look so good and you need to manually add styling or a styling framework. Then I started to look a bit into Tailwind CSS, which looks to be the most popular framework. But if you really want to do something with that, you need Tailwind Plus. While its free to use for open-source applications, I prefer a free framework and as I liked frameworks, like Material in the past, I learned that you could arrange a better start via [Vuetify](https://vuetifyjs.com/en/) via [some simple Vite command](https://vuetifyjs.com/en/getting-started/installation/#using-vite) working via NPM.
```
npm create vuetify@latest
```
...and then via some wizard adding just TypeScript. (and choosing `vuetify-project` as a name for the folder)
And directly after that, you can just run it from the `vuetify-project` folder (via the installed Vite tooling) via the following command.
```
npm run dev
```

And then play-around with the vue-files a bit and directly see what changes in the Browser. (or sometimes just restart manually if you don't trust what you are seeing)

### ASP.NET WEB.API (.NET10)

Then, next to the frond-end folder, it was time to create a back-end folder/project.
And all that I needed to do in PowerShell (from the earlier mentioned root-folder) was typing the following command:
```
dotnet new webapi -n TheWeb.API
```
And again, from the subfolder, you can directly run it via the follwing command:
```
dotnet run
```

Then later, the default weatherforecast stuff can be ditched from the Program.cs and the controllers need to be linked so Program.cs can be as simple as this:
```
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(); 
var app = builder.Build();
app.MapControllers(); 
app.Run();
```

Then you add a simple controller that spits out a hard-coded list:

```
namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/todo
public class TodoController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<TodoItem>> Get()
    {
        Console.WriteLine("Received GET request for /api/todo");
        // This is your hardcoded list
        var items = new List<TodoItem>
        {
            new TodoItem { Id = 1, Task = "Learn Vue with .NET", IsCompleted = false },
            new TodoItem { Id = 2, Task = "Configure VS Code", IsCompleted = true },
            new TodoItem { Id = 3, Task = "Build an Awesome App", IsCompleted = false }
        };

        return Ok(items); // Returns a 200 OK status with the JSON list
    }
}
```

and then get to see that list by opening it in the browser:
```
http://localhost:5160/api/todo
```

### Accessing the WEB.API via the Vue app
When running in develop-mode, that vite.config.mts must be modified, so calls to the API are directed to the ASP.NET WEB API.
To the server config, you just add this proxy part to get that arranged:

```
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5160', // .NET app URL
        changeOrigin: true,            // Recommended for local proxying
        secure: false
      }
    }
  },
```

Then in some scripting block, you can just use the fetch command to get the data.

```
  const response = await fetch('/api/todo')
```
...and then if the response is OK, await the response.json
```
  const data = await response.json() as TodoItem[]
```
...and then dump it into some Vue ref object so it can be displayed.

## Commenting the stuff in GitHub

When making all this documentation, I sometimes peaked at this documentation of the [markdown stuff](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax).
