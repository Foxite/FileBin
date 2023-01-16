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

builder.Services.AddScoped<FileStorage, LocalFileStorage>();

builder.Services.AddDbContext<FileDbContext>(dbcob => {
	string connectionString = builder.Configuration.GetValue<string>("Database");
	dbcob.UseNpgsql(connectionString);
});

var authConfig = builder.Configuration.GetSection("Authorization").Get<AuthConfig>();

builder.Services
	.AddAuthentication(options => {
		options.DefaultScheme = "Cookies";
		options.DefaultChallengeScheme = "Bearer";
	})
	.AddCookie()
	.AddOAuth("Bearer", options => {
		options.SignInScheme = "Cookies";
		options.CallbackPath = "/signin";
		options.TokenEndpoint = authConfig.TokenEndpoint;
		options.AuthorizationEndpoint = authConfig.AuthEndpoint;
		options.UserInformationEndpoint = authConfig.UserEndpoint;
		options.ClientId = authConfig.ClientId;
		options.ClientSecret = authConfig.ClientSecret;
		foreach (string scope in authConfig.Scopes) {
			options.Scope.Add(scope);
		}
	});

builder.Services.AddAuthorization(options => {
	options.AddPolicy("Upload", policy => {
		if (authConfig.UserRole != null) {
			policy.RequireAuthenticatedUser();
			policy.RequireRole(authConfig.UserRole);
			//policy.RequireClaim("scope", authConfig.Scope);
		} else {
			policy.RequireAssertion(_ => true);
		}
	});
	
	options.AddPolicy("Delete", policy => {
		policy.RequireAuthenticatedUser();
	});
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
