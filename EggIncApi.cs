using Ei;
using Google.Protobuf;

public class EggIncApi
{
    private const int CLIENTVERSION = 40;
    public static async Task<ContractCoopStatusResponse> GetCoopStatus(string contractId, string coopId)
    {
        ContractCoopStatusRequest coopStatusRequest = new ContractCoopStatusRequest();
        coopStatusRequest.ContractIdentifier = contractId;
        coopStatusRequest.CoopIdentifier = coopId;

        return await MakeEggIncApiRequest("coop_status", coopStatusRequest, ContractCoopStatusResponse.Parser.ParseFrom);
    }

    public static async Task<EggIncFirstContactResponse> GetFirstContact(string userId)
    {
        EggIncFirstContactRequest firstContactRequest = new EggIncFirstContactRequest();
        firstContactRequest.EiUserId = userId;
        firstContactRequest.ClientVersion = CLIENTVERSION;

        return await MakeEggIncApiRequest("first_contact", firstContactRequest, EggIncFirstContactResponse.Parser.ParseFrom);
    }

    private static async Task<T> MakeEggIncApiRequest<T>(string endpoint, IMessage data, Func<ByteString, T> parseMethod)
    {
        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            data.WriteTo(stream);
            bytes = stream.ToArray();
        }

        string baseUrl = "https://wasmegg.zw.workers.dev/?url="; 
        string response = await PostRequest($"{baseUrl}https://www.auxbrain.com/ei/{endpoint}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data", Convert.ToBase64String(bytes) }
        }));

        //Todo: I don't know if we have to check whether the response is an AuthenticatedMessage or not
        AuthenticatedMessage authenticatedMessage = AuthenticatedMessage.Parser.ParseFrom(Convert.FromBase64String(response));

        return parseMethod(authenticatedMessage.Message);
    }

    private static async Task<string> PostRequest(string url, FormUrlEncodedContent json)
    {
        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(url, json);
            return await response.Content.ReadAsStringAsync();
        }
    }

    private static Dictionary<int, string> roles = new Dictionary<int, string>
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

    public static string SoulPowerToFarmerRole(double soulPower)
    {
        return roles[(int)Math.Floor(soulPower)];
    }
}