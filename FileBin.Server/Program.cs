using FileBin.Server;
using FileBin.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FileDbContext>(dbcob => dbcob.UseNpgsql(builder.Configuration.GetValue<string>("Database")));

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
		policy.RequireAuthenticatedUser();
		policy.RequireRole(authConfig.Role);
		policy.RequireClaim("scope", authConfig.Scope);
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
