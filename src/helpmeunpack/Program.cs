using Newtonsoft.Json.Linq;
using System.Text;

var httpClient = new HttpClient();
var problemUri = "https://hackattic.com/challenges/help_me_unpack/problem?access_token=";
var response = await httpClient.GetAsync(problemUri);

if (!response.IsSuccessStatusCode) return;

var content = await response.Content.ReadAsStringAsync();
var input = JObject.Parse(content);

if (!input.TryGetValue("bytes", out var bytesToken)) return;

var bytes = Convert.FromBase64String(bytesToken.ToString());

var int32 = BitConverter.ToInt32(bytes, 0);
var uint32 = BitConverter.ToUInt32(bytes, 4);
var int16 = BitConverter.ToInt16(bytes, 8);
var float32 = BitConverter.ToSingle(bytes, 12);
var double64 = BitConverter.ToDouble(bytes, 16);

var lastFourBytes = bytes[24..];
Array.Reverse(lastFourBytes);
var bigEndianDouble64 = BitConverter.ToDouble(lastFourBytes, 0);

var output = new JObject
{
    { "int", int32 },
    { "uint", uint32 },
    { "short", int16 },
    { "float", float32 },
    { "double", double64 },
    { "big_endian_double", bigEndianDouble64 }
};

var solveUri = "https://hackattic.com/challenges/help_me_unpack/solve?access_token=";
var solveResponse = await httpClient.PostAsync(solveUri, new StringContent(output.ToString(), Encoding.UTF8, "application/json"));

Console.WriteLine(await solveResponse.Content.ReadAsStringAsync());