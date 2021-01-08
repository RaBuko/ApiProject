using ApiProject;
using ApiProject.Services;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ApiTest.Helpers
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
                services.AddTransient<IUserService, UserService>();
                services.AddAutoMapper(assemblies: null);

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
