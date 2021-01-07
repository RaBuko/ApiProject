using ApiProject;
using ApiTest._TestPrepare;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ApiTest
{
    public class ApiFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                var descrp = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApiContext>));

                services.Remove(descrp);

                services.AddDbContext<ApiContext>(o => o.UseInMemoryDatabase("InMemoryDb"));

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApiContext>();
                db.Database.EnsureCreated();

                DbUtilities.InitDbForTests(db);

                services.AddSingleton<IStartupFilter, CustomStartupFilter>();
            });
        }
    }
}
