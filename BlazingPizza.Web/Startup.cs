using BlazingPizza.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
                    // you can enable a default challenge scheme by using the line below:
                    // options.DefaultChallengeScheme = "microsoft";
                })
                .AddCookie()
                .AddMicrosoftAccount("microsoft", options => {
                    options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                    options.CallbackPath = "/mstoken";
                })
                .AddGoogle("google", options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    options.CallbackPath = "/gtoken";
                })
                .AddFacebook("facebook", options =>
                {
                    options.ClientId = Configuration["Authentication:Facebook:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Facebook:ClientSecret"];
                    options.CallbackPath = "/ftoken";
                })
                .AddOAuth("github", options =>
                {
                    options.ClientId = Configuration["Authentication:Github:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Github:ClientSecret"];
                    options.CallbackPath = "/gittoken";
                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey("urn:github:login", "login");
                    options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                    options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var user = JObject.Parse(await response.Content.ReadAsStringAsync());
                            
                            var nameToken = user.SelectToken("name");
                            // no name value, use login instead
                            if (string.IsNullOrEmpty(nameToken.ToString()))
                            {
                                var jprop = nameToken.Parent as JProperty;
                                jprop.Value = user.Value<string>("login");
                            }
                            
                            context.RunClaimActions(user);
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

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
