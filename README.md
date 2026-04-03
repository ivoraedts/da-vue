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

## Getting it to work in Docker

First thing to do, is add Dockerfiles for both projects.

### Dockerfile for Web Api

For this one, we can use `mcr.microsoft.com/dotnet/aspnet:10.0-preview` as the base image and the related sdk as the build image.
It came down to the following Dockerfile:
```
# Stage 1: Base - Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build - SDK for compiling
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
# Copy specifically the csproj first to leverage Docker caching
COPY ["TheWeb.API.csproj", "."]
RUN dotnet restore "./TheWeb.API.csproj"
# Copy everything else and build
COPY . .
RUN dotnet build "TheWeb.API.csproj" -c Release -o /app/build

# Stage 3: Publish - Prepare for final image
FROM build AS publish
RUN dotnet publish "TheWeb.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final - Lean runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TheWeb.API.dll"]
```

### Dockerfile for Vue front-end

I know it should run on some Linux, but had no clue which images to pick for building the image and producing the final image.
So I was lazy enough to ask AI, and this one picked `node:lts-alpine` for the build work and `nginx:stable-alpine` as the final image.
Both are based on alpine, which is a bare-version of Linux. The base one contains stuff that is needed for running Node, which is needed for the faster builds.
The final image is a tiny version of Alpine which contains Nginx for serving the Vue files, which are static HTML, CSS and JS files.
It came down to the following Dockerfile for the Vue front-end:
```
# Stage 1: Build the Vue app
FROM node:lts-alpine AS build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: Serve with Nginx
FROM nginx:stable-alpine AS production-stage
# Copy the custom nginx config
COPY default.conf /etc/nginx/conf.d/default.conf 
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

With these Docker files in place, I could run the individual apps in Docker. But off course there is a little more to be done. First thing is off course the docker-Compose file...

### docker-Compose file

For both services, we point to the Dockerfile, map ports from host to container, define the docker network, set some environment variables and configure that the front-end depends on the back-end.
It came down to the following docker-compose file:
```
services:
  theweb-api:
    build:
      context: ./TheWeb.API
      dockerfile: Dockerfile
    ports:
      - "5160:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_HTTP_PORTS=8080
    networks:
      - docker-da-vue

  theweb-ui:
    build:
      context: ./vuetify-project
      dockerfile: Dockerfile
    ports:
      - "3000:80"
    depends_on:
      - theweb-api
    networks:
      - docker-da-vue

networks:
  docker-da-vue:
    driver: bridge
```

So from the outside, you can connect to port 3000 (front-end) which will map to internal port 80. And to port 5160 (back-end) which will map to internal port 8080. It is also specified in the environment variable that it needs to run on 8080, so that should just match.
Within the docker network, theweb-ui (service name of front-end) can communicate to theweb-api (service name of back-end) using the servicename on internal port 8080.
In order to achieve that theweb-ui uses this servicename, the production config needs to be configured. (this is different to the vite.config.mts that is used for development).
It came down to the following default.conf:
```
server {
    listen 80;
    server_name localhost;

    location / {
        root /usr/share/nginx/html;
        index index.html index.htm;
        try_files $uri $uri/ /index.html;
    }

    # This is the "Proxy" for Nginx
    location /api/ {
        proxy_pass http://theweb-api:8080/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```
So all requests to `/api/` are replaced by calls to `http://theweb-api:8080/api/`, so on this way it communicates to the web-api via the servicename on port 8080.

## Getting it to work on the Synology

The easiest way to get this to work on the Synology, is by first pushing the images to the public docker hub.
After playing around with the Synology, which runs on DSM 7.3.2 it was time to get Docker Stuff running on it.
Via Package Center, you can find it, named as Container Manager, which is a bit like the Docker Desktop for Synology.
Via the GUI, you find public container images that are stored on the [Docker Hub](https://hub.docker.com/), which is actually the same 'shared library' on which we found the base images for creating our own docker image, via the Register tab.
After installing the [Container Tools extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-containers) to Visual Studio Code, you can find the created images in the Containers tab.
And from there, you can push these images to the [Docker Hub](https://hub.docker.com/).
I needed to do this for both the `theweb-api` and `theweb-ui`. I just kept the names that I defined first in that docker-compose file.
You can find them on the Docker Hub now... [theweb-api on Docker Hub](https://hub.docker.com/repository/docker/ivoraedts/da-vue-theweb-api) and [theweb-ui on Docker Hub](https://hub.docker.com/repository/docker/ivoraedts/da-vue-theweb-ui/general).

After these were on Docker-Hub, it was time to define the project file in Synology, which is a special docker-compose file.
This time, we don't build the file, but we just collect it from the docker hub.
It came down to the following file and it worked directly :grin: :

```
networks:
  docker-da-vue:
    driver: bridge

services:
  theweb-api:
    image: ivoraedts/da-vue-theweb-api:latest
    ports:
      # <Host Port>:<Container Port>
      - "5160:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_HTTP_PORTS=8080
    networks:
      - docker-da-vue

  theweb-ui:
    image: ivoraedts/da-vue-theweb-ui:latest
    ports:
    # <Host Port>:<Container Port>
      - "3000:80"
    depends_on:
      - theweb-api
    networks:
      - docker-da-vue
```

## Commenting the stuff in GitHub

When making all this documentation, I sometimes peaked at this documentation of the [markdown stuff](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax).
