using ExploringChannelsAndDataflow.Common;
using ExploringChannelsAndDataflow.Dataflow.BackgroundWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ExploringChannelsAndDataflow.Dataflow
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHostedService, BackgroundWorkerHost>();
            services.AddSingleton<ThingBackgroundWorker>();
            services.AddSingleton<IBackgroundWorkerManager>(s => s.GetRequiredService<ThingBackgroundWorker>());
            services.AddSingleton<IBackgroundWorkerHandler<Thing>>(s => s.GetRequiredService<ThingBackgroundWorker>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
