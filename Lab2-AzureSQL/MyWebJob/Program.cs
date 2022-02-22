using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

public class Program
{
	static async Task Main(string[] args)
	{
		// Setup Host
		var host = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(app =>
			{
				app.AddJsonFile("appsettings.json");
				app.AddEnvironmentVariables();
			})
			.ConfigureServices(services =>
			{
				services.AddHostedService<Worker>();
			})
			.Build();

		await host.RunAsync();
	}
}

public class Worker : IHostedService, IDisposable
{
	private PeriodicTimer? timer;
	private readonly IConfiguration configuration;

	public Worker(IConfiguration configuration)
	{
		this.configuration = configuration;
	}

	public async Task StartAsync(CancellationToken stoppingToken)
	{
		Console.WriteLine("Worker started...");

		timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

		do
		{
			await ProcessEmailQueueAsync(stoppingToken);

			Console.WriteLine("Waiting for next tick...");
		}
		while (await timer.WaitForNextTickAsync(stoppingToken));
	}

	private async Task ProcessEmailQueueAsync(CancellationToken stoppingToken)
	{
		Console.WriteLine("Checking for new e-mails to be sent...");

		using var conn = new SqlConnection(configuration.GetConnectionString("MyDatabase"));
		await conn.OpenAsync(stoppingToken);

		var cmd = new SqlCommand("SELECT * FROM EmailQueue WHERE Sent IS NULL", conn);

		using var reader = await cmd.ExecuteReaderAsync(stoppingToken);

		while (await reader.ReadAsync(stoppingToken))
		{
			Console.WriteLine($"Email ID:{reader["ID"]} found...");

			string recipients = reader["Recipient"].ToString() ?? throw new InvalidOperationException("Recipient not provided");
			string? subject = reader["Subject"].ToString();
			string? body = reader["Body"].ToString();
			int mailId = Convert.ToInt32(reader["ID"]);

			await SendMailAsync(recipients, subject, body, mailId, stoppingToken);
		}
	}

	private async Task SendMailAsync(string recipients, string? subject, string? body, int mailId, CancellationToken stoppingToken)
	{
		using var smtpClient = new SmtpClient();
		smtpClient.Host = configuration.GetValue<string>("MailSettings:SmtpHost");
		smtpClient.Credentials = new NetworkCredential(configuration.GetValue<string>("MailSettings:SmtpUsername"), configuration.GetValue<string>("MailSettings:SmtpPassword"));

		await smtpClient.SendMailAsync(
			from: configuration.GetValue<string>("MailSettings:SmtpFrom"),
			recipients: recipients,
			subject: subject,
			body: body,
			stoppingToken);

		// mark mail as sent
		using var conn = new SqlConnection(configuration.GetConnectionString("MyDatabase"));
		await conn.OpenAsync(stoppingToken);
		var cmd = new SqlCommand("UPDATE EmailQueue SET Sent = GETDATE()  WHERE Id = @ID", conn);
		cmd.Parameters.AddWithValue("@Id", mailId);
		await cmd.ExecuteNonQueryAsync();

		Console.WriteLine($"Email ID:{mailId} sent...");
	}

	public Task StopAsync(CancellationToken stoppingToken)
	{
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		timer?.Dispose();
	}
}