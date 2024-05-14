using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Candidate;
using Services;// Import the OrganizationServices namespace

namespace YourNamespace
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // Add MVC controllers
            services.AddSingleton<IConfiguration>(Configuration); // Add configuration service
            services.AddScoped<OrganizationServices>(); // Register the OrganizationService
            services.AddHttpClient("default", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5); // Set timeout to 30 seconds
            });
            // Add other services as needed
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Use the developer exception page
            }
            else
            {
                app.UseExceptionHandler("/Home/Error"); // Use a custom error page for non-development environments
                app.UseHsts(); // Use HSTS (HTTP Strict Transport Security)
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
            app.UseStaticFiles(); // Enable serving static files like HTML, CSS, JavaScript, etc.

            app.UseRouting(); // Enable routing

            app.UseAuthorization(); // Enable authorization

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"); // Configure default routing
            });
        }
    }
}
