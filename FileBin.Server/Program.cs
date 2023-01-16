using FileBin.Server.Config;
using FileBin.Server.Data;
using FileBin.Server.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<LocalFileStorage.Options>(builder.Configuration.GetSection("LocalFileStorage"));
builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection("Authorization"));

builder.Services.AddScoped<FileStorage, LocalFileStorage>();

builder.Services.AddDbContext<FileDbContext>(dbcob => {
	string connectionString = builder.Configuration.GetValue<string>("Database");
	dbcob.UseNpgsql(connectionString);
});

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope()) {
	var dbContext = scope.ServiceProvider.GetRequiredService<FileDbContext>();
	await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", ctx => {
	ctx.Response.Redirect(app.Configuration.GetValue<string>("IndexRedirect"));
	
	return Task.CompletedTask;
});

app.Run();
