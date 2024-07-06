using System.Text;
using Newtonsoft.Json.Linq;

var HttpClient = new HttpClient();

var problemUri = "https://hackattic.com/challenges/help_me_unpack/problem?access_token=";

var httpResponseMessageProblem = await HttpClient.GetAsync(problemUri);

if (!httpResponseMessageProblem.IsSuccessStatusCode)
{
    Console.WriteLine($"Failed to fetch data. Status code: {httpResponseMessageProblem.StatusCode}");
    return;
}

var problemContent = await httpResponseMessageProblem.Content.ReadAsStringAsync();
var input = JObject.Parse(problemContent);

if (!input.TryGetValue("bytes", out var bytesToken))
{
    Console.WriteLine("Key 'bytes' not found in the response.");
    return;
}

var bytes = Convert.FromBase64String(bytesToken.ToString());

var int32 = BitConverter.ToInt32(bytes, 0);
var uint32 = BitConverter.ToUInt32(bytes, 4);
var int16 = BitConverter.ToInt16(bytes, 8);

/*
 Why am I skipping 2 after int16 (2 bytes)?
float32 (a 4-byte floating-point number) typically requires alignment on a 4-byte boundary 
for efficient memory access. This means it should start at an offset that is a multiple of 4 
(e.g., 0, 4, 8, 12, etc.).

Modern CPUs are optimized to fetch data from memory in chunks aligned 
with their size and memory bus width
*/
var float32 = BitConverter.ToSingle(bytes, 12);

var double64 = BitConverter.ToDouble(bytes, 16);

var lastFourBytes = bytes[24..];
Array.Reverse(lastFourBytes);

var bigEndianDouble64 = BitConverter.ToDouble(lastFourBytes, 0);

var ouput = new JObject()
            {
                { "int", int32 },
                { "uint", uint32 },
                { "short", int16 },
                { "float", float32 },
                { "double", double64 },
                { "big_endian_double", bigEndianDouble64 }
            };

var solveUri = "https://hackattic.com/challenges/help_me_unpack/solve?access_token=";

var httpResponseMessageSolve = await HttpClient
    .PostAsync(solveUri, new StringContent(ouput.ToString(), Encoding.UTF8, "application/json"));

var solveContent = await httpResponseMessageSolve.Content.ReadAsStringAsync();

Console.WriteLine(solveContent);