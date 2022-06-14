var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
	builder.Configuration.AddAzureKeyVault(new Uri("https://cloud-app-dev-vault.vault.azure.net/"), new Azure.Identity.DefaultAzureCredential());
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
