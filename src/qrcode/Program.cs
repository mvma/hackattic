using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Text;
using ZXing.Common;

var httpClient = new HttpClient();
var problemUri = "https://hackattic.com/challenges/reading_qr/problem?access_token=";
var response = await httpClient.GetAsync(problemUri);

if (!response.IsSuccessStatusCode) return;

var content = await response.Content.ReadAsStringAsync();
var input = JObject.Parse(content);

if (!input.TryGetValue("image_url", out var imageUrl)) return;

var imageResponse = await httpClient.GetAsync(imageUrl.ToString());

if (!imageResponse.IsSuccessStatusCode) return;

var bitmap = new Bitmap(Image.FromStream(await imageResponse.Content.ReadAsStreamAsync()));
var reader = new ZXing.Windows.Compatibility.BarcodeReader
{
    AutoRotate = true,
    Options = new DecodingOptions { TryHarder = true }
};

var result = reader.Decode(bitmap);

var output = new JObject { { "code", result.Text } };

var solveUri = "https://hackattic.com/challenges/reading_qr/solve?access_token=";
var solveResponse = await httpClient.PostAsync(solveUri, new StringContent(output.ToString(), Encoding.UTF8, "application/json"));

Console.WriteLine(await solveResponse.Content.ReadAsStringAsync());
