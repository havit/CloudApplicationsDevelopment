using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace ZodiacFunction
{
    public class ZodiacFunction(ILogger<ZodiacFunction> logger)
	{
		[Function("GetSign")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            logger.LogInformation("GetSign: request received.");

            string? dateOfBirthQuery = req.Query["dateOfBirth"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DateTime dateOfBirth = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateOfBirthQuery) &&
                !DateTime.TryParse(dateOfBirthQuery, out dateOfBirth))
            {
                logger.LogWarning("GetSign: invalid dateOfBirth value '{Value}'.", dateOfBirthQuery);
                return new BadRequestObjectResult("Invalid date of birth");
            }

            if (!string.IsNullOrEmpty(requestBody) &&
                !DateTime.TryParse(requestBody, out dateOfBirth))
            {
                logger.LogWarning("GetSign: invalid dateOfBirth in request body.");
                return new BadRequestObjectResult("Invalid date of birth");
            }

            ZodiacSign sign = GetSign(dateOfBirth);
            logger.LogInformation("GetSign: dateOfBirth={DateOfBirth}, sign={Sign}.", dateOfBirth.ToShortDateString(), sign);
            return new OkObjectResult(sign.ToString());
        }

        [Function("GetHoroscope")]
        public async Task<IActionResult> GetHoroscope([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            logger.LogInformation("GetHoroscope: request received.");

            string? dateOfBirthQuery = req.Query["dateOfBirth"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DateTime dateOfBirth = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateOfBirthQuery) &&
                !DateTime.TryParse(dateOfBirthQuery, out dateOfBirth))
            {
                logger.LogWarning("GetHoroscope: invalid dateOfBirth value '{Value}'.", dateOfBirthQuery);
                return new BadRequestObjectResult("Invalid date of birth");
            }

            if (!string.IsNullOrEmpty(requestBody) &&
                !DateTime.TryParse(requestBody, out dateOfBirth))
            {
                logger.LogWarning("GetHoroscope: invalid dateOfBirth in request body.");
                return new BadRequestObjectResult("Invalid date of birth");
            }

            if (dateOfBirth == DateTime.MinValue)
            {
                logger.LogWarning("GetHoroscope: missing dateOfBirth parameter.");
                return new BadRequestObjectResult("Missing dateOfBirth parameter");
            }

            AzureOpenAIClient azureClient = new(
                new Uri(Config.AzureOpenAiEndpoint),
                new AzureKeyCredential(Config.AzureOpenAiApiKey));
            ChatClient chatClient = azureClient.GetChatClient(Config.AzureOpenAiDeployment);

            var sign = GetSign(dateOfBirth);
            logger.LogInformation("GetHoroscope: calling AI for sign={Sign}.", sign);
            string today = DateTime.UtcNow.ToString("d. M. yyyy");
            ChatCompletion completion = await chatClient.CompleteChatAsync(
                new SystemChatMessage(
                    "Jsi zkušený astrolog s poetickým, ale věcným stylem. " +
                    "Píšeš horoskopu v češtině. Každý horoskop musí: " +
                    "1) reflektovat typické vlastnosti daného znamení, " +
                    "2) zmínit konkrétní planetu nebo aspekt (např. Merkur, Venuše, opozice Marsu), " +
                    "3) dát praktickou radu nebo výhled na daný den. " +
                    "Délka: 3–4 věty. Nesměřuj odpověď na konkrétní osobu, piš obecně pro celé znamení."),
                new UserChatMessage($"Dnešní datum je {today}. Napiš horoskop pro znamení {sign}.")
            );

            var horoscope = completion.Content[0].Text;
            logger.LogInformation("GetHoroscope: AI response received, length={Length} chars.", horoscope.Length);
            return new OkObjectResult(new { sign = sign.ToString(), horoscope });
        }

        private static ZodiacSign GetSign(DateTime dateOfBirth) =>
			dateOfBirth switch
			{
				{ Day: <= 20, Month: <= 1 } => ZodiacSign.Capricornus,
				{ Day: <= 20, Month: <= 2 } => ZodiacSign.Aquarius,
				{ Day: <= 20, Month: <= 3 } => ZodiacSign.Pisces,
				{ Day: <= 20, Month: <= 4 } => ZodiacSign.Aries,
				{ Day: <= 20, Month: <= 5 } => ZodiacSign.Taurus,
				{ Day: <= 21, Month: <= 6 } => ZodiacSign.Gemini,
				{ Day: <= 22, Month: <= 7 } => ZodiacSign.Cancer,
				{ Day: <= 22, Month: <= 8 } => ZodiacSign.Leo,
				{ Day: <= 22, Month: <= 9 } => ZodiacSign.Virgo,
				{ Day: <= 23, Month: <= 10 } => ZodiacSign.Libra,
				{ Day: <= 22, Month: <= 11 } => ZodiacSign.Scorpius,
				{ Day: <= 21, Month: <= 12 } => ZodiacSign.Sagittarius,
				_ => ZodiacSign.Capricornus
			};

		private enum ZodiacSign
        {
            Aries,
            Taurus,
            Gemini,
            Cancer,
            Leo,
            Virgo,
            Libra,
            Scorpius,
            Sagittarius,
            Capricornus,
            Aquarius,
            Pisces
        }
    }
}
