using ExampleBot;
using ExampleBot.Modules;
using Fergun.Interactive.Extensions;
using GScraper.Google;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDiscordShardedGateway(options => options.Intents = GatewayIntents.AllNonPrivileged | GatewayIntents.MessageContent);
builder.Services.AddApplicationCommands();
builder.Services.AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>(options => options.ResultHandler = new EmptyResultHandler<StringMenuInteractionContext>());
builder.Services.AddInteractiveService();

builder.Services.AddSingleton<GoogleScraper>();

var host = builder.Build();

host.AddApplicationCommandModule<PaginatorModule>();
host.AddApplicationCommandModule<SelectionModule>();
host.AddApplicationCommandModule<WaitModule>();
host.AddApplicationCommandModule<CustomModule>();
host.AddComponentInteractionModule<SelectMenuModule>();

await host.RunAsync();
