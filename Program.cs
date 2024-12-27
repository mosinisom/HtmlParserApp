using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
  static async Task Main()
  {
    string url = "https://izhevsk.cian.ru/cat.php?deal_type=sale&engine_version=2&offer_type=flat&p=2&region=4770";

    HttpClient client = new HttpClient();

    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

    HttpResponseMessage response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();
    string html = await response.Content.ReadAsStringAsync();

    HtmlDocument document = new HtmlDocument();
    document.LoadHtml(html);

    var adverts = document.DocumentNode.SelectNodes("//article"); 
    if (adverts != null)
    {
      foreach (var advert in adverts)
      {
        var mainInfoNode = advert.SelectSingleNode(".//a/span/span");
        string mainInfo = mainInfoNode?.InnerText?.Trim() ?? "Информация не найдена";

        var addressNodes = advert.SelectNodes(".//a[@data-name='GeoLabel']");

        string fullAddress = addressNodes != null
            ? string.Join(", ", addressNodes.Select(node => node.InnerText.Trim()))
            : "Адрес не найден";

        var priceNode = advert.SelectSingleNode(".//span[@data-mark='MainPrice']//span");
        string price = "Цена не найдена";

        if (priceNode != null)
        {
          price = priceNode.InnerText.Trim()
              .Replace("&nbsp;", " ")  
              .Replace(" ₽", "")       
              .Trim();
        }

        Console.WriteLine($"Информация: {mainInfo}");
        Console.WriteLine($"Адрес: {fullAddress}");
        Console.WriteLine($"Цена: {price}");
        Console.WriteLine(new string('-', 50));
      }
    }
    else
    {
      Console.WriteLine("Объявления не найдены.");
    }
  }
}
