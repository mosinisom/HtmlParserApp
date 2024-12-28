using HtmlAgilityPack;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

public class Advertisement
{
  public string MainInfo { get; set; }
  public string Address { get; set; }
  public string Price { get; set; }
  public string Url { get; set; }
}

class Program
{
  static async Task Main()
  {
    int pageNumber = 1;
    int totalAds = 0;
    var advertisements = new List<Advertisement>();
    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

    while (true)
    {
      string url = $"https://izhevsk.cian.ru/cat.php?deal_type=sale&engine_version=2&offer_type=flat&p={pageNumber}&region=4770";
      Console.WriteLine($"Обработка страницы {pageNumber}...");

      try
      {
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string html = await response.Content.ReadAsStringAsync();

        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(html);

        var adverts = document.DocumentNode.SelectNodes("//article");

        if (adverts == null || !adverts.Any())
        {
          Console.WriteLine("Больше объявлений не найдено. Парсинг завершен.");
          break;
        }

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

          var linkNode = advert.SelectSingleNode(".//a[contains(@class, '_93444fe79c--link--eoxce')]");
          string advertisementUrl = linkNode?.GetAttributeValue("href", "Ссылка не найдена") ?? "Ссылка не найдена";

          advertisements.Add(new Advertisement
          {
            MainInfo = mainInfo,
            Address = fullAddress,
            Price = price,
            Url = advertisementUrl
          });

          totalAds++;
          Console.WriteLine($"Обработано объявлений: {totalAds}");
        }

        pageNumber++;
        
        if (pageNumber > 5)
        {
          Console.WriteLine("Обработка завершена. Превышено количество страниц.");
          break;
        }

        Random random = new Random();
        await Task.Delay(random.Next(1000, 3000));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Произошла ошибка: {ex.Message}");
        break;
      }
    }

    var options = new JsonSerializerOptions
    {
      WriteIndented = true,
      Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    string json = JsonSerializer.Serialize(advertisements, options);
    await File.WriteAllTextAsync("advertisements.json", json);

    Console.WriteLine($"Всего обработано и сохранено объявлений: {totalAds}");
  }
}