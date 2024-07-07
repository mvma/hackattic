using System.Drawing;
using System.Text;
using Newtonsoft.Json.Linq;
using ZXing.Common;

var HttpClient = new HttpClient();

var problemUri = "https://hackattic.com/challenges/reading_qr/problem?access_token=";

var httpResponseMessageProblem = await HttpClient.GetAsync(problemUri);

if (!httpResponseMessageProblem.IsSuccessStatusCode)
{
    Console.WriteLine($"Failed to fetch data. Status code: {httpResponseMessageProblem.StatusCode}");
    return;
}

var problemContent = await httpResponseMessageProblem.Content.ReadAsStringAsync();
var input = JObject.Parse(problemContent);

if (!input.TryGetValue("image_url", out var image_url))
{
    Console.WriteLine("Key 'image_url' not found in the response.");
    return;
}

var httpResponseMessageImageUrl = await HttpClient.GetAsync(image_url.ToString());

if (!httpResponseMessageImageUrl.IsSuccessStatusCode)
{
    Console.WriteLine($"Failed to fetch data. Status code: {httpResponseMessageImageUrl.StatusCode}");
    return;
}

var bitmap = new Bitmap(Image.FromStream(await httpResponseMessageImageUrl.Content.ReadAsStreamAsync()));
bitmap.Save(@"C:\Users\marcus.arruda\Downloads\qrcode.bmp");
var reader = new ZXing.Windows.Compatibility.BarcodeReader()
{
    AutoRotate = true,
    Options = new DecodingOptions
    {
        TryHarder = true
    }
};

var result = reader.Decode(bitmap);

Console.WriteLine(result.Text);

var ouput = new JObject()
            {
                { "code", result.Text }
            };

var solveUri = "https://hackattic.com/challenges/reading_qr/solve?access_token=";

var httpResponseMessageSolve = await HttpClient
    .PostAsync(solveUri, new StringContent(ouput.ToString(), Encoding.UTF8, "application/json"));

var solveContent = await httpResponseMessageSolve.Content.ReadAsStringAsync();

Console.WriteLine(solveContent);