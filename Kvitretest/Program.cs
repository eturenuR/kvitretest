using System.Text.Json;
using Kvitretest;
using Kvitretest.Models;
using Kvitretest.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.IISIntegration;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddControllers();
        builder.Services.AddTransient<IDatabaseService, JsonUnsecureDatabaseService>();
        builder.Services.AddTransient<IDatabaseService, SqliteDatabaseService>();

        builder.Services.AddAuthentication(options => {
            options.DefaultChallengeScheme = "base64";
            options.AddScheme<MyAuthenticationHandler>("base64", "encode, decode base64");
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            //app.UseDeveloperExceptionPage();
        }
        app.UseStatusCodePages();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.UseAuthorization();

        // The manual way to do this.
        //app.UseEndpoints(endpoints =>
        //{
        //    //endpoints.MapRazorPages();

        //    ////endpoints.MapGet("/api/v1/posts/all", (context) =>
        //    //endpoints.MapGet("/allposts", (context) =>
        //    //{
        //    //    var alle_postene = app.Services.GetService<SqliteDatabaseService>().GetAllPosts();
        //    //    var json = JsonSerializer.Serialize<IEnumerable<OnePost>>(alle_postene, new JsonSerializerOptions()
        //    //    {
        //    //        WriteIndented = true
        //    //    });
        //    //    return context.Response.WriteAsync(json);
        //    //});
        //});

        app.MapRazorPages();

        app.MapControllers();

        app.Run();
    }
}