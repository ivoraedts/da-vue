# Da-Vue

## About Da-Vue

It was about time to play around a bit with Vue. And by adding Da in front, it sounds like a Vietnames city. 🇻🇳 - Like Da Lat, which I visited in 2014. Just for fun and for reminding that Vietnam was a very interesting country to see and experience.
<p align="center" width="100%">
    <img width="40%" src="images/DaLat-22dec2014.JPG">
</p>


## Combining the following topics
- Vue.JS
- TypeScript
- Material Styling (via Vuetify)
- .NET 10 of ASP.NET API
- Docker
- Synology

<p align="center" width="100%">
    <img width="50%" src="vuetify-project/src/assets/logo-small.png">
</p>

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

## Connecting to a Database

Due to some recent experience with [another project](https://github.com/ivoraedts/docker-mudblazor/) running on Docker / Synology with a database, I could repeat the same tricks on getting things to work including a database. In that case I was working with MudBlazor and combined it with PostgreSQL and PG Admin locally and with Adminer on the Synology (for easier installation).

### Running PostgreSQL and PG Admin in Docker

As I don't feel like local installations for this, I started again with just running PostgreSQL and PG Admin in Docker Desktop on such a way that I can later connect from both local and from within the same docker network. So, just for getting the database and administration to run, it resulted in this docker-compose file, that I have named ``docker-compose-db.yml``. For this I used the latest versions of:
1. PostgreSQL (starting with the latest version of the [official image](https://hub.docker.com/_/postgres) )
2. PG Admin (starting with the latest version of the [most pulled image](https://hub.docker.com/r/dpage/pgadmin4) )

For both of them, I defined a volume mapping, so the data is stored on disk.
So, when the containers and images are destroyed and replaced, the data will still be there.
I added some simple username plus password for both postrgres and pgadmin.
As I have no postgres or so running on the localhost, I have exposed the default port (5433).
For pgadmin (which runs on 80, I defined the host port as 5051).
```
services:
  postgres:
    image: postgres:latest
    container_name: docker-da-vue.postgres
    #command: postgres -c max_connections=100
    volumes:  
    # This fix applies to version 18 and later. before it could mount to /var/lib/postgresql/data
    # This fix applies to version 18 and later: mount to /var/lib/postgresql/docker works, but does not persist
      - ./docker/pgdata:/var/lib/postgresql
    networks:
      - docker-da-vue
    environment:
      POSTGRES_USER: user-name
      POSTGRES_PASSWORD: strong-password
    ports:
      # <Host Port>:<Container Port>
      - "5433:5432"     

  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: docker-da-vue.pgadmin
    volumes:
      - ./docker/pgadmin:/var/lib/pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: user-name@domain-name.com
      PGADMIN_DEFAULT_PASSWORD: strong-password
    networks:
      - docker-da-vue
    ports:
      # <Host Port>:<Container Port>
      - "5051:80"

networks:
  docker-da-vue:
    driver: bridge
```

So when running ``docker compose -f docker-compose-local.yml up`` , the images are downloaded and pushed into containers within the same network.
When opening PG Admin via the ``http://localhost:5051/``, you must use the login and password from the dockerfile. (so ``user-name@domain-name.com`` as login and ``strong-password`` as password).
Then via Servers, add a new Registry to the postgres database. Then you can connect via the servicename ``postgres`` over port ``5432`` via username ``user-name`` and ``strong-password`` as password.
If this all works, you can create a database and if desired a table in it.

### Accessing the Database from the Local Web Api application

Off course it's nice to arrange the data access via Entity Framework Core, so I added references to ``Microsoft.EntityFrameworkCore``, ``Microsoft.EntityFrameworkCore.Tools`` and ``Npgsql.EntityFrameworkCore.PostgreSQL``.
In order to be able to access the database from the server-part of the application, some trivial things need to be added to the MyApplication (Server-part) project.
As I did some stuff with TodoItems, I made a data folder and a TodoItem class in it to define the columns of the new table.

```
namespace TheWeb.API.Data;

public class TodoItem
{
    public long Id { get; set; }
    public required string Task { get; set; }
    public bool IsCompleted { get; set; }
}
```

Then it's time to add a Database context and add a DbSet of that type. I named it ``DaVueDbContext`` :
```
using Microsoft.EntityFrameworkCore;

namespace TheWeb.API.Data
{
    public class DaVueDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DaVueDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = Configuration.GetConnectionString("DaVueDb");
            options.UseNpgsql(connectionString);
        }

        public DbSet<TodoItem> TodoItems { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDaVueDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DaVueDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DaVueDb"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
                )
            );
            return services;
        }
    }
}
```

That DbSet of TodoItem, named TodoItems, will ensure there is going to be a table, named TodoItems. (after migrations)
To wire it up, in the ``Program.cs``, I added:
```
builder.Services.AddDaVueDbContext(builder.Configuration);
```

before ``var app = builder.Build();`` and after that I added:
```
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DaVueDbContext>();
    dbContext.Database.Migrate();
}
```

that Migrate() line makes sure the migrations will be run on start-up.

Then in ``appsettings.json`` // ``appsettings.Development.json`` I need to add the connection string that I want to use for making a connection from my local-running Application.
In order to connect from my local-running application to the postgres database running on my local docker, I must use port 5433 that I exposed, so it looks like:

```
    "ConnectionStrings": {
    "DaVueDb": "Host=localhost:5433;Database=DaVueDb;Username=user-name;Password=strong-password"
  }
```
Keep in mind, that in the later stage, when MudBlazor also runs in docker, we can directly connect to ``Host=postgres:5432;`` , like how we connected from PgAdmin to PostgresQL before. So that is how we connect it in the regular ``appsettings.json``.

And than from the Power Shell, I needed to first install the Entitiy Framework tooling:
```
dotnet tool install --global dotnet-ef
```
and then add that migration:
```
dotnet ef migrations add InitialDatbaseMigration
```

Then if all went well, running the application should result in running the migrations.
But before running, you should first arrange that the database exist (which you can do via pgadmin).

### Accessing the Database from the MudBlazor application running in Docker

Instead of connecting to the database via ``localhost:5433`` as configured in ``appsettings.Development.json``, we can now arrange that we connect via ``postgres:5432`` as configured in ``appsettings.json``.
This is arranged in the Docker-Compose file and for this, we use a docker-network that we named ``docker-da-vue``.
Also we specify that theweb-api service runs in Production mode, so it uses the regular ``appsettings.json``.
As we learned from previous chicken-egg problems, we define that theweb-api depends on the postgres service. And in that postgres service we define a health check.

```
services:
  theweb-api:
    build:
      context: ./TheWeb.API
      dockerfile: Dockerfile
    ports:
      - "5160:8080"
    depends_on:
      postgres: 
        condition: service_healthy
        restart: true
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
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

  postgres:
    image: postgres:latest
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -d DaVueDb -U user-name -h localhost -p 5432"]
      interval: 10s
      retries: 5
      start_period: 10s
      timeout: 10s
    container_name: docker-da-vue.postgres
    #command: postgres -c max_connections=100
    volumes:  
    # This fix applies to version 18 and later. before it could mount to /var/lib/postgresql/data
    # This fix applies to version 18 and later: mount to /var/lib/postgresql/docker works, but does not persist
      - ./docker/pgdata:/var/lib/postgresql
    networks:
      - docker-da-vue
    environment:
      POSTGRES_USER: user-name
      POSTGRES_PASSWORD: strong-password
    ports:
      # <Host Port>:<Container Port>
      - "5433:5432"     

  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: docker-da-vue.pgadmin
    volumes:
      - ./docker/pgadmin:/var/lib/pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: user-name@domain-name.com
      PGADMIN_DEFAULT_PASSWORD: strong-password
    networks:
      - docker-da-vue
    ports:
      # <Host Port>:<Container Port>
      - "5051:80"

networks:
  docker-da-vue:
    driver: bridge
```

Then via Power Shell you can build and run the application as follows:
```
docker compose -f docker-compose.yml build
```
```
docker compose -f docker-compose.yml up
```

### Accessing the Database from the Da Vue application running in Container Manager on the Synology

So, the first thing to do after those migrations were added to the code, is similar to how we earlier published the application image to the [Docker Hub](https://hub.docker.com/) via the Containers tab of Visual Studio Code.
Note that we only need to push the [theweb-api image](https://hub.docker.com/repository/docker/ivoraedts/da-vue-theweb-api), as the postgres and pgadmin images are already present in the Docker Hub and theweb-au image is not modified.
On the Synology on the Container Manager we will use the Project-tab to get this to work.

From the Project Tab, you can create a new project by clicking on Create (or Maken in Dutch 😁).
Then you give the project a name, like ``da-vue``, you can define the root-path / volume of the project, like ``/volume1/docker/da-vue``, and can choose to create a new docker-compose.yml.
This is similar to the docker-compose files that we were using for getting things to work on Docker Desktop.
Only now the volumes will be relative to the project-volume.
So when I mount postgres on ``./docker/pgdata`` I can find the data on ``/volume1/docker/da-vue/docker/pgdata``.
This is nice, so if you run more projects, the volumes are not mixed in the same folders.
Make sure the folder exists before starting... So go to File Station and create the folders. (so docker in the root, then da-vue and then pgdata)

So I ended up with the following docker-compose project yml that I have stored in git as ``synology-docker-compose-yml``:
```
services:
  theweb-api:
    image: ivoraedts/da-vue-theweb-api:latest
    ports:
      # <Host Port>:<Container Port>
      - "5160:8080"
    depends_on:
      postgres: 
        condition: service_healthy
        restart: true
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

  postgres:
    image: postgres:latest
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -d DaVueDb -U user-name -h localhost -p 5432"]
      interval: 10s
      retries: 5
      start_period: 10s
      timeout: 10s
    container_name: docker-da-vue.postgres
    #command: postgres -c max_connections=100
    volumes:  
    # This fix applies to version 18 and later. before it could mount to /var/lib/postgresql/data
    # This fix applies to version 18 and later: mount to /var/lib/postgresql/docker works, but does not persist
      - ./docker/pgdata:/var/lib/postgresql
    networks:
      - docker-da-vue
    environment:
      POSTGRES_USER: user-name
      POSTGRES_PASSWORD: strong-password
    ports:
      # <Host Port>:<Container Port>
      - "5433:5432"

  adminer:
    image: adminer:latest
    container_name: docker-da-vue.adminer    
    networks:
      - docker-da-vue
    ports:
      # <Host Port>:<Container Port>
      - "8081:8080"

networks:
  docker-da-vue:
    driver: bridge
```

As I am not sure if I will use postgres from the outside, I decided to map the port to 5433. But maybe I could remove that mapping as well and prevent access from the outside.
Then for Adminer, I just mapped the 8081 to 8080, so over that port I can access adminer.
The web app itself is accesible of port 3000 and even the API is directly accesible over port 5160.
Off course for security reasons, you could remove the port mappings for the API and postgresql.

## Some clean starting point

Up until now, this might be a good template to start with using Vue + TypeScript + Vuetify Material Styling, ASP.NET 10 API, Docker and a PostgresQL Database. So before adding more that is probably not interesting as a starting point, it made sense to create a branch, so that can be used (forked or whatever) as a nice starting point:

[A branch named nice-clean-starting-point](https://github.com/ivoraedts/da-vue/tree/nice-clean-starting-point)

## Temperature and Humidity tracking via Tado

I was thinking about what to do next. Maybe connect to some data that I can read from my home equipment. I considered the 'Slimme meter'. But reading the smart reader (for actual gas usage, power usage and power feedback) required some extra hardware and effort. So I came up with reading the actual temperature and humidity as I have a 'slimme thermostaat' (smart thermostat) from TADO. This is one of the market leaders in the Netherlands. After some searching, I did find a .net-library that is capable of reading those values. [It is open source via Github](https://github.com/KoenZomers/TadoApi) and available on NuGet, so it was worth trying it.

It took me a while to understand on how to best use that library. Setting up the connection is already a bit tricky as you need more than just a login and a password. For setting up a new connection, I could build in some wizard using a [Stepper component](https://vuetifyjs.com/en/components/steppers/#usage). And I also needed some running before I knew which errors to catch. So first step was setting up a connection using both the WEB.API and the Front-end project. Once that is done, it was time to create some back-end task for retrieving the data. And then subsequently some more back-end task for making interesting aggregations. Than for displaying the data on the front-end I decided to use the [Sparklines component](https://vuetifyjs.com/en/components/sparklines/#usage). And I have done my best to find a beautiful, consistent, and fitting color scheme for it.

I made stuff via both Rider and VS Code. And at some point when I finished showing hourly data for a given day, I used AI functionality to help doing the same stuff but then for weekly and monthly data. Rider was bitching about linking a credit card before enabling the AI-agent chat, so I decided to stick with VS Code for that part. Some work was done by the Raptor mini AI and some work was done by Grok Code fast. I cannot really tell/remember which one worked better. They both did OK and both made some mistakes as well.

I also did some work for making it look good on my tiny iPhone 13 mini. And it looks fine. Maybe I would need to do some more work to make it look better on a regular computer monitor, but that's maybe for another time.

## Commenting the stuff in GitHub

When making all this documentation, I sometimes peaked at this documentation of the [markdown stuff](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax).
