using Microsoft.AspNetCore.Mvc;
using TheWeb.API.Models;

namespace TheWeb.API.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/displaylink
public class DisplayLinkController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<DisplayLink>> Get()
    {
        var items = new List<DisplayLink>
        {
            new DisplayLink
            {
                Title = "Documentation",
                Subtitle = "Learn about all things Vuetify in our documentation.",
                Icon = "mdi-text-box-outline",
                Href = "https://vuetifyjs.com/",
                Color = "primary"
            },
            new DisplayLink
            {
                Title = "Features",
                Subtitle = "Explore available framework Features.",
                Icon = "mdi-star-circle-outline",
                Href = "https://vuetifyjs.com/introduction/why-vuetify/#feature-guides",
                Color = "secondary"
            },
            new DisplayLink
            {
                Title = "Components",
                Subtitle = "Discover components in the API Explorer.",
                Icon = "mdi-widgets-outline",
                Href = "https://vuetifyjs.com/components/all",
                Color = "indigo-darken-3"
            },
            new DisplayLink
            {
                Title = "Playground",
                Subtitle = "Experiment with Vuetify in our online playground.",
                Icon = "mdi-code-tags",
                Href = "https://play.vuetifyjs.com/",
                Color = "teal-darken-3"
            },
            new DisplayLink
            {
                Title = "Community",
                Subtitle = "Connect with Vuetify developers.",
                Icon = "mdi-account-group-outline",
                Href = "https://discord.vuetifyjs.com/",
                Color = "pink-darken-3"
            },
            new DisplayLink
            {
                Title = "Vite",
                Subtitle = "Documentation for Vite, the build tool used in this project.",
                Icon = "mdi-text-box-outline",
                Href = "https://vite.dev/guide/features.html",
                Color = "brown-darken-3"
            },
            new DisplayLink
            {
                Title = "Using Vue in VSCode",
                Subtitle = "Learn how to use Vue in Visual Studio Code.",
                Icon = "mdi-cast-education",
                Href = "https://code.visualstudio.com/docs/nodejs/vuejs-tutorial",
                Color = "blue-darken-3"
            },
            new DisplayLink
            {
                Title = "Middle of Vue tutorial",
                Subtitle = "Learn about Vue.js features and concepts.",
                Icon = "mdi-book-open-page-variant-outline",
                Href = "https://vuejs.org/tutorial/#step-7",
                Color = "green-darken-3"
            },
            new DisplayLink
            {
                Title = "Video",
                Subtitle = "Some Youtube video about Vue with ASP.NET Core",
                Icon = "mdi-youtube",
                Href = "https://www.youtube.com/watch?v=HIIWdxEk_ls",
                Color = "red-darken-3"
            },
            new DisplayLink
            {
                Title = "Opensource TADO API",
                Subtitle = "Opensource Tado Api available on NuGet",
                Icon = "mdi-github",
                Href = "https://github.com/KoenZomers/TadoApi",
                Color = "grey-darken-4"
            },
            new DisplayLink
            {
                Title = "Tado Article",
                Subtitle = "Some article explaining the communication",
                Icon = "mdi-home-thermometer-outline",
                Href = "https://help.tado.com/en/articles/8565472-how-do-i-authenticate-to-access-the-rest-api",
                Color = "blue-accent-4"
            },
        };

        return Ok(items); // Returns a 200 OK status with the JSON list
    }
}