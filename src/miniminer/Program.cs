using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

using var httpClient = new HttpClient();
var problemUri = $"https://hackattic.com/challenges/mini_miner/problem?access_token=";

var response = await httpClient.GetAsync(problemUri);
if (!response.IsSuccessStatusCode) return;

var content = await response.Content.ReadAsStringAsync();
var input = JObject.Parse(content);

if (!input.TryGetValue("block", out var block)) return;

var difficulty = input.Value<int>("difficulty");
var portionOfBytes = (int)Math.Ceiling(difficulty / 8.0);

using var sha256 = SHA256.Create();
var nonce = 0;

var modifiedBlock = new JObject
{
    { "data", block["data"] },
    { "nonce", null }
};

var blockJson = JsonConvert.SerializeObject(modifiedBlock, new JsonSerializerSettings { Formatting = Formatting.None });

while (true)
{
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockJson));
    if (CheckLeadingZeros(bytes, portionOfBytes, difficulty))
    {
        Console.WriteLine($"Nonce found: {nonce}");
        break;
    }
    nonce++;
    modifiedBlock["nonce"] = nonce;
    blockJson = JsonConvert.SerializeObject(modifiedBlock, new JsonSerializerSettings { Formatting = Formatting.None });
}

var solveUri = $"https://hackattic.com/challenges/mini_miner/solve?access_token=&playground=1";
var solveContent = new JObject { ["nonce"] = nonce };

var solveResponse = await httpClient.PostAsync(solveUri, new StringContent(solveContent.ToString(), Encoding.UTF8, "application/json"));

Console.WriteLine(solveResponse.IsSuccessStatusCode ? await solveResponse.Content.ReadAsStringAsync() : $"Failed to solve challenge. Status code: {solveResponse.StatusCode}");

bool CheckLeadingZeros(byte[] bytes, int portionOfBytes, int difficulty)
{
    var chunk = bytes[..portionOfBytes];
    foreach (var currentValue in chunk)
    {
        if (currentValue == 0)
        {
            difficulty -= 8;
            continue;
        }
        var shiftedValue = currentValue;
        while ((shiftedValue & 0x80) == 0)
        {
            difficulty--;
            shiftedValue <<= 1;
        }
        if (difficulty <= 0) return true;
    }
    return difficulty <= 0;
}