using FileBin.Server.Config;
using FileBin.Server.Data;
using FileBin.Server.Storage;
using Microsoft.EntityFrameworkCore;
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
	.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options => {
		options.Authority = authConfig.Authority;
		options.RequireHttpsMetadata = true;
		options.TokenValidationParameters = new TokenValidationParameters {
			ValidateAudience = true,
			ValidAudiences = authConfig.Audiences
		};
		options.MetadataAddress = authConfig.DiscoveryDocument ?? authConfig.Authority + "/.well-known/openid-configuration";
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
