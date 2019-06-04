using ExploringChannelsAndDataflow.Channels.BackgroundWork;
using ExploringChannelsAndDataflow.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ExploringChannelsAndDataflow.Channels
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHostedService, BackgroundWorkerHost>();
            services.AddSingleton<ThingBackgroundWorker>();
            services.AddSingleton<IBackgroundWorkerManager>(s => s.GetRequiredService<ThingBackgroundWorker>());
            services.AddSingleton<IBackgroundWorkerHandler<Thing>>(s => s.GetRequiredService<ThingBackgroundWorker>());
        }

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/submit",
                pathApp => pathApp.Run(async (context) =>
                {
                    var handler = context.RequestServices.GetRequiredService<IBackgroundWorkerHandler<Thing>>();
                    if (await handler.SubmitAsync(new Thing(DateTime.Now.Ticks)))
                    {
                        context.Response.StatusCode = StatusCodes.Status202Accepted;
                        await context.Response.WriteAsync("Work submitted!");
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        await context.Response.WriteAsync("Work cannot be submitted right now!");
                    }
                }));

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
