using System.Net.Http.Json;
using WebAPIModels.Models;

string uri = $"https://localhost:7026/api/Customers";
HttpClient client = new HttpClient();
var chapter = await client.GetFromJsonAsync<IEnumerable<Customer>>(uri);
if (chapter is not null)
{
    foreach(var c in chapter)
    {
        Console.WriteLine($"{c.City}: {c.ContactName} ");
    }
}
Console.ReadLine();



