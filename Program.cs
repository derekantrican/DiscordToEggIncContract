using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Ei;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//Can use ShareX screenshot to get screen coords
int startX = 2870;
int startY = 115;
int endX = 3570;
int endY = 1333;

TimeSpan interval = TimeSpan.FromSeconds(30);

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
    using (var bitmap = new Bitmap(endX - startX, endY - startY))
    {
        using (var g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(startX, startY, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
        }

        bitmap.Save("screenshot.jpg", ImageFormat.Jpeg);
        string ocr = await DoOCR("screenshot.jpg");
        IEnumerable<string> parsedCoops = ParseCoops(ocr);
        IEnumerable<string> newCoops = parsedCoops.Except(openCoops).Except(invalidOrFullCoopMatches);
        if (newCoops.Any())
        {
            Console.WriteLine($"NEW COOPS:\n\n{string.Join("\n", newCoops)}\n");
            openCoops.AddRange(newCoops);
        }
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
        else
        {
            Console.WriteLine($"{contractCoopId} currently has {coopStats.OpenSpots} open spots and the highest role of {SoulPowerToFarmerRole(coopStats.HighestSoulPower)}");
        }
    }

    Console.WriteLine(); //Spacing

    Thread.Sleep(interval);
}

static async Task<string> DoOCR(string fileName)
{
    string resp = null;
    try
    {
        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = new TimeSpan(1, 1, 1);

        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent("K88095770188957"), "apikey"); //Added api key in form data
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
        // var json = JObject.Parse(strContent);

        // if (json.Value<int>("OCRExitCode") == 1)
        // {
        //     string results = "";
        //     foreach (JObject parsedResult in json["ParsedResults"])
        //     {
        //         results += parsedResult
        //     }

        //     for (int i = 0; i < ocrResult.ParsedResults.Count() ; i++)
        //     {
        //         txtResult.Text = txtResult.Text + ocrResult.ParsedResults[i].ParsedText ;
        //     }
        // }

        //Todo: I would prefer to use Newtonsoft over the Rootobject stuff
        Rootobject ocrResult = JsonConvert.DeserializeObject<Rootobject>(resp);

        if (ocrResult.OCRExitCode == 1)
        {
            string results = "";
            for (int i = 0; i < ocrResult.ParsedResults.Count() ; i++)
            {
                results += ocrResult.ParsedResults[i].ParsedText ;
            }

            return results;
        }
        else
        {
            Console.WriteLine($"OCR error: {resp}");
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"OCR exception: {exception.Message}\n{exception.StackTrace}");
        Console.Write($"OCR response: {resp}");
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

public class Rootobject
{
    public Parsedresult[] ParsedResults { get; set; }
    public int OCRExitCode { get; set; }
    public bool IsErroredOnProcessing { get; set; }
    public string[] ErrorMessage { get; set; }
    public string ErrorDetails { get; set; }
}

public class Parsedresult
{
    public object FileParseExitCode { get; set; }
    public string ParsedText { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorDetails { get; set; }
}