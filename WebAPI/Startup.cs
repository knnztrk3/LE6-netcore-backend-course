using Core.DependencyResolvers;
using Core.Extensions;
using Core.Utilities.IoC;
using Core.Utilities.Security.Encryption;
using Core.Utilities.Security.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI
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
            //services.AddMemoryCache();
            services.AddControllers();

            // ENABLE CORS
            services.AddCors(options=> {
                options.AddPolicy("AllowOrigin", 
                    builder=>builder.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowCredentials().AllowAnyMethod());// Origin istek yap�lan yer demek. domainimiz ne ise onu yaz�yoruz.
            });

            //JSON Serializer
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling=Newtonsoft
            .Json.ReferenceLoopHandling.Ignore)
            .AddNewtonsoftJson();

            var tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options=> {
                options.TokenValidationParameters = new TokenValidationParameters 
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey)
                };
            });

            services.AddDependencyResolvers(new ICoreModule[] 
            {
                new CoreModule(),
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureCustomExceptionMiddleware();

            app.UseCors(builder => builder.WithOrigins("http://localhost:4200").AllowCredentials().AllowAnyMethod().AllowAnyHeader()); // buradan gelen t�rl� talebe cevap ver -> Header get post put delete gibi istekler bunlar�n hepsine izin ver
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication(); // Anahtar, giri� anahtar� -> kullan�c�y� authenticate edicez -> kullan�c� ad� �ifresiyle ve token vas�tas�yla nerelere ne yapabilir kim neye yetkili bunlar� izliyo olucaz
            app.UseAuthorization(); // Ne yapabilir, yetki

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
