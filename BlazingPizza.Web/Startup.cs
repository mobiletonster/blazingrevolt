using BlazingPizza.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazingPizza.Web
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.AddControllers(); 
            //services.AddControllersWithViews();
            services.AddMvc();

            services.AddDbContext<PizzaStoreContext>(options => options.UseSqlite("Data Source=pizza.db"));
            services.AddScoped<SpecialsService>();
            services.AddScoped<ToppingsService>();
            services.AddScoped<OrdersService>();
            services.AddScoped<UserService>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "churchaccount";
                })
                .AddCookie()
                .AddOpenIdConnect("churchaccount", options =>
                {
                    options.ClientId = Configuration["OAuth:ClientId"];
                    options.ClientSecret = Configuration["OAuth:ClientPassword"];
                    options.Authority = Configuration["OAuth:Issuer"];
                    options.CallbackPath = new PathString("/token");
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.UseTokenLifetime = true;
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey("lang", "ui_locales");
                    options.ClaimActions.MapJsonKey("id_token", "id_token");
                    string access_token = string.Empty;
                    string id_token = string.Empty;
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenResponseReceived = async (context) =>
                        {
                            // stash the access token for later use so we can stuff it into the Identity
                            access_token = context.TokenEndpointResponse.AccessToken;
                            id_token = context.TokenEndpointResponse.IdToken;
                            await Task.CompletedTask;
                        }
                        ,
                        OnTokenValidated = async (context) =>
                        {
                            // It's later now, so place the access token into the Identity Principal
                            (context.Principal.Identity as ClaimsIdentity)?.AddClaim(new Claim("access_token", access_token));
                            (context.Principal.Identity as ClaimsIdentity)?.AddClaim(new Claim("id_token", id_token));
                            await Task.CompletedTask;
                        }
                    };

                })
                .AddTwitter("twitter",twitterOptions =>
                {
                    twitterOptions.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                    twitterOptions.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                    twitterOptions.CallbackPath = "/token";
                    twitterOptions.Events.OnRemoteFailure = (context) =>
                    {
                        context.HandleResponse();
                        return context.Response.WriteAsync("<script>window.close();</script>");
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.EnvironmentName.StartsWith("Dev"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();

            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            // app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
