using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Ei;
using Google.Protobuf;
using Newtonsoft.Json;

/*------------ OVERVIEW ------------
This program serves to watch for new coops posted to the Egg, Inc Discord (either the #ads-only-standard or 
#ads-only-elite channels) and report back what the highest roles are - ensuring you can pick the best coop.
It is against the ToS of Discord to automatically interact with a server without a bot being added to that
server, so this works by
1. Taking a screenshot of the Discord channel
2. Sending to ocr.space for text recognition
3. Parsing that text recognition for eicoop.netlify.app urls (thereby giving us the contract id & coop id)
4. Using that contract id & coop id in an API request to the wasmegg Egg, Inc API to check the 
   status of the coop
5. Reporting back to the console with that status (contract id, coop id, open spots, and highest role)
6. Repeat

- Found coops will be stored and checked again every interval (so they will still be checked even if
  the url is no longer in the screenshot)
- Full or invalid coops (because sometimes OCR messes up) will be stored so they aren't checked again
- There is an option to filter by soul power (reference table below) so you will only see coops that
  have a highest role above a certain threshold
----------------------------------*/

dynamic settings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json"));

Dictionary<int, string> roles = new Dictionary<int, string>
{
  { 0, "Farmer" },
  { 1, "Farmer II" },
  { 2, "Farmer III" },
  { 3, "Kilofarmer" },
  { 4, "Kilofarmer II" },
  { 5, "Kilofarmer III" },
  { 6, "Megafarmer" },
  { 7, "Megafarmer II" },
  { 8, "Megafarmer III" },
  { 9, "Gigafarmer" },
  { 10, "Gigafarmer II" },
  { 11, "Gigafarmer III" },
  { 12, "Terafarmer" },
  { 13, "Terafarmer II" },
  { 14, "Terafarmer III" },
  { 15, "Petafarmer" },
  { 16, "Petafarmer II" },
  { 17, "Petafarmer III" },
  { 18, "Exafarmer" },
  { 19, "Exafarmer II" },
  { 20, "Exafarmer III" },
  { 21, "Zettafarmer" },
  { 22, "Zettafarmer II" },
  { 23, "Zettafarmer III" },
  { 24, "Yottafarmer" },
  { 25, "Yottafarmer II" },
  { 26, "Yottafarmer III" },
  { 27, "Xennafarmer" },
  { 28, "Xennafarmer II" },
  { 29, "Xennafarmer III" },
  { 30, "Weccafarmer" },
  { 31, "Weccafarmer II" },
  { 32, "Weccafarmer III" },
  { 33, "Vendafarmer" },
  { 34, "Vendafarmer II" },
  { 35, "Vendafarmer III" },
  { 36, "Uadafarmer" },
  { 37, "Uadafarmer II" },
  { 38, "Uadafarmer III" },
  { 39, "Treidafarmer" },
  { 40, "Treidafarmer II" },
  { 41, "Treidafarmer III" },
  { 42, "Quadafarmer" },
  { 43, "Quadafarmer II" },
  { 44, "Quadafarmer III" },
  { 45, "Pendafarmer" },
  { 46, "Pendafarmer II" },
  { 47, "Pendafarmer III" },
  { 48, "Exedafarmer" },
  { 49, "Exedafarmer II" },
  { 50, "Exedafarmer III" },
  { 51, "Infinifarmer" },
};

List<string> openCoops = new List<string>();
List<string> invalidOrFullCoopMatches = new List<string>(); //"coops" found by OCR that are full or don"t actually exist
while (true) //MAIN LOOP
{
    bool printSpace = false;

    TakeScreenshot(settings.screenshotArea.startX, settings.screenshotArea.startY, settings.screenshotArea.endX, settings.screenshotArea.endY);

    string ocr = await DoOCR("screenshot.jpg", settings.ocrSpaceApiKey);
    IEnumerable<string> parsedCoops = ParseCoops(ocr);
    IEnumerable<string> newCoops = parsedCoops.Except(openCoops).Except(invalidOrFullCoopMatches);
    if (newCoops.Any())
    {
        Console.WriteLine($"NEW COOPS:\n  {string.Join("\n  ", newCoops)}\n");
        openCoops.AddRange(newCoops);
        printSpace = true;
    }

    foreach (string contractCoopId in openCoops.ToList())
    {
        string[] contractAndCoopIds = contractCoopId.Split(":");
        CoopStats coopStats = await GetCoopStats(contractAndCoopIds[0], contractAndCoopIds[1]);
        if (coopStats == null || coopStats.OpenSpots == 0)
        {
            openCoops.Remove(contractCoopId);
            invalidOrFullCoopMatches.Add(contractCoopId);
        }
        else if (coopStats.HighestSoulPower > (double)settings.soulPowerFilter)
        {
            Console.WriteLine($"{contractCoopId} currently has {coopStats.OpenSpots} open spots and the highest role of {SoulPowerToFarmerRole(coopStats.HighestSoulPower)} (SP: {Math.Round(coopStats.HighestSoulPower, 3)})");
            printSpace = true;
        }
    }

    if (printSpace)
    {
        Console.WriteLine(); //Spacing
    }

    Thread.Sleep(TimeSpan.FromSeconds((double)settings.intervalSeconds));
}

static void TakeScreenshot(int startX, int startY, int endX, int endY)
{
    using (var bitmap = new Bitmap(endX - startX, endY - startY))
    {
        using (var g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(startX, startY, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
        }

        bitmap.Save("screenshot.jpg", ImageFormat.Jpeg);
    }
}

static async Task<string> DoOCR(string fileName, string apikey)
{
    string resp = null;
    try
    {
        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = new TimeSpan(1, 1, 1);

        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(apikey), "apikey");
        form.Add(new StringContent("eng"), "language");
        form.Add(new StringContent("2"), "ocrengine"); 
        form.Add(new StringContent("true"), "scale");
        form.Add(new StringContent("true"), "istable");

        if (string.IsNullOrEmpty(fileName) == false)
        {
            byte[] imageData = File.ReadAllBytes(fileName);
            form.Add(new ByteArrayContent(imageData, 0, imageData.Length), "image", "image.jpg");
        }

        HttpResponseMessage response = await httpClient.PostAsync("https://api.ocr.space/Parse/Image", form);

        resp = await response.Content.ReadAsStringAsync();
        dynamic ocrResult = JsonConvert.DeserializeObject<dynamic>(resp);

        if (ocrResult.OCRExitCode == 1)
        {
            string results = "";
            foreach (var parsedResult in ocrResult.ParsedResults)
            {
                results += parsedResult.ParsedText;
            }

            return results;
        }
        else
        {
            Console.WriteLine($"OCR error: {ocrResult.ErrorMessage[0]}");
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"OCR exception: {exception.Message}\n{exception.StackTrace}");
        Console.WriteLine($"OCR response: {resp}");
    }

    return "";
}

IEnumerable<string> ParseCoops(string fullText)
{
    string input = fullText.ToLower().Replace(" ", ""); //Clean up the string a bit before matching

    foreach (Match match in new Regex(@"eicoop\.netlify\.app\/(?<contractid>[^\/]*)\/(?<coopid>[^\/\s]*)").Matches(input))
    {
        yield return $"{match.Groups["contractid"]}:{match.Groups["coopid"]}";
    }
}

async Task<CoopStats> GetCoopStats(string contractId, string coopId)
{
    try
    {
        ContractCoopStatusRequest coopStatusRequest = new ContractCoopStatusRequest();
        coopStatusRequest.ContractIdentifier = contractId;
        coopStatusRequest.CoopIdentifier = coopId;

        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            coopStatusRequest.WriteTo(stream);
            bytes = stream.ToArray();
        }

        string baseUrl = "https://wasmegg.zw.workers.dev/?url="; 
        string response = await PostRequest(baseUrl + "https://www.auxbrain.com/ei/coop_status", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data", Convert.ToBase64String(bytes) }
        }));

        AuthenticatedMessage authenticatedMessage = AuthenticatedMessage.Parser.ParseFrom(Convert.FromBase64String(response));

        ContractCoopStatusResponse contractCoopStatusResponse = ContractCoopStatusResponse.Parser.ParseFrom(authenticatedMessage.Message);

        EggIncFirstContactRequest firstContactRequest = new EggIncFirstContactRequest();
        firstContactRequest.EiUserId = contractCoopStatusResponse.Contributors[0].UserId;
        firstContactRequest.ClientVersion = 36;

        using (var stream = new MemoryStream())
        {
            firstContactRequest.WriteTo(stream);
            bytes = stream.ToArray();
        }

        response = await PostRequest(baseUrl + "https://www.auxbrain.com/ei/first_contact", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data", Convert.ToBase64String(bytes) }
        }));

        authenticatedMessage = AuthenticatedMessage.Parser.ParseFrom(Convert.FromBase64String(response));

        EggIncFirstContactResponse firstContactResponse = EggIncFirstContactResponse.Parser.ParseFrom(authenticatedMessage.Message);

        return new CoopStats
        {
            OpenSpots = (int)firstContactResponse.Backup.Contracts.Contracts.FirstOrDefault(c => c.Contract.Identifier == contractId).Contract.MaxCoopSize - contractCoopStatusResponse.Contributors.Count,
            HighestSoulPower = contractCoopStatusResponse.Contributors.MaxBy(c => c.SoulPower).SoulPower,
        };
    }
    catch
    {
        Console.WriteLine($"Exception encountered when checking stats of {contractId}:{coopId} (maybe OCR messed up the recognition?)");
        return null;
    }
}

string SoulPowerToFarmerRole(double soulPower)
{
    return roles[(int)Math.Floor(soulPower)];
}

async Task<string> PostRequest(string url, FormUrlEncodedContent json)
{
    using (var client = new HttpClient())
    {
        var response = await client.PostAsync(url, json);
        return await response.Content.ReadAsStringAsync();
    }
}

public class CoopStats
{
    public double HighestSoulPower { get; set; }
    public int OpenSpots { get; set; }
}