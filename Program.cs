#define test1

using HtmlAgilityPack;
using Knapcode.TorSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Torbook
{
    class Program
    {
        static string ip = "";

        static async Task Main(string[] args)
        {
#if !test
            Console.Title = "Ładowanie...";

            #region Connect Tor

            var settings = new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorZipped"),
                ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorExtracted"),
                PrivoxyPort = 1337,
                TorSocksPort = 1338,
                TorControlPort = 1339,
                TorControlPassword = "foobar"
            };

            // download tools
            await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();

            // execute
            var proxy = new TorSharpProxy(settings);
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxyPort))
            };
            var httpClient = new HttpClient(handler);
            await proxy.ConfigureAndStartAsync();

            ip = await httpClient.GetStringAsync("http://api.ipify.io/");

            #endregion

            #region Facebook login
            Clear();
            Console.Title = "Torbook :: Logowanie";
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Login: ");
            Console.ResetColor();
            string login = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Hasło: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            string pass = Console.ReadLine();
            Console.ResetColor();
            Clear();
            Console.WriteLine("Logowanie...");
            Console.Title = "Torbook :: Logowanie...";

            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", login),
                new KeyValuePair<string, string>("pass", pass),
                //new KeyValuePair<string, string>("email", "mariuszk312"),
                //new KeyValuePair<string, string>("pass", "MACIEK123d"),
                new KeyValuePair<string, string>("login", "Zaloguj+się"),
                new KeyValuePair<string, string>("try_number", "0"),
                new KeyValuePair<string, string>("unrecognized_tries", "0"),
                new KeyValuePair<string, string>("m_ts", unixTimestamp.ToString())
            });
            var result = await httpClient.PostAsync("https://m.facebookcorewwwi.onion/login/device-based/regular/login/", content);
            string resultContent = await result.Content.ReadAsStringAsync();
            #endregion
#else
            ip = "";
#endif
            Clear();
            Console.Title = "Torbook";

            var doc = new HtmlDocument();
#if !test
            doc.LoadHtml(resultContent);
            //StreamWriter file = new StreamWriter("C:\\Users\\Maciek\\Desktop\\Projects\\test.html");
            //file.WriteLine(resultContent);
            //file.Close();
#else
            doc.Load("C:\\Users\\Maciek\\Desktop\\Projects\\test.html");
#endif

            string uname = doc.DocumentNode
                .SelectNodes("/html/body/div/div/div[2]/div/div[5]/div/table/tbody/tr/td[2]/a[1]").First().InnerText;
            int notread = 0;
            try
            {
                int.TryParse(doc.DocumentNode.SelectNodes("/html/body/div/div/div[1]/div/div/a[3]").First().InnerHtml.Split('(')[1].Split(')')[0], out notread);
            }
            catch (Exception)
            {
                notread = 0;
            }

            Console.WriteLine("Witaj, " + uname + "!");
            ip = uname;
            if (notread > 1)
                Console.WriteLine("Masz " + notread + " nieprzeczytane wiadomości.");
            else if (notread == 1)
                Console.WriteLine("Masz nieprzeczytaną wiadomość.");

            bool running = true;
            int select = -1;
            int chselect = -1;

            while (running)
            {
                try
                {
                    if (select == -1 || select == 3)
                    {
                        if (select == 3) Clear();

                        Console.WriteLine("Zakładki: ");
                        int i = 0;
                        string[] menu = new[] { "Strona główna", "Wiadomości", "Menu", "Wyjdź" };
                        foreach (var node in menu)
                        {
                            i++;
                            Console.WriteLine(i + ". " + node);
                        }

                        Console.Write(": ");
                        if (!int.TryParse(Console.ReadLine(), out select)) select = 3;
                    }
                    else if (select == 1)
                    {
#if !test
                        doc.LoadHtml(resultContent);
                        resultContent = httpClient.GetStringAsync("https://m.facebookcorewwwi.onion/").Result;
#else
                    doc.Load("C:\\Users\\Maciek\\Desktop\\Projects\\test.html");
#endif
                        try
                        {
                            foreach (var node in doc.DocumentNode.SelectNodes("/html/body/div/div/div[2]/div/div[4]/div[3]/div"))
                            {
                                // /html/body/div/div/div[2]/div/div[4]/div[3]/div
                                // /html/body/div/div/div[2]/div/div[4]/div[3]/div[1]/div[2]/div[2]/span/p[1]
                                try
                                {
                                    if (node.ChildNodes.Count == 0) continue;

                                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine(" " + node.SelectSingleNode("div[2]/div[1]/table/tbody/tr/td[2]/div/h3").InnerText + " ");
                                    Console.ResetColor();
                                    foreach (var n in node.SelectNodes("div[2]/div[2]/span/p"))
                                        Console.WriteLine(n.InnerText);
                                }
                                catch (Exception) { }
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Wystąpił błąd podczas ładowania feedu");
                        }

                        Console.Write(": ");
                        if (!int.TryParse(Console.ReadLine(), out select)) select = 3;
                    }
                    else if (select == 2)
                    {
                        #region Chat
                        Clear();

#if !test
                        Console.WriteLine("Ładowanie...");
                        string chats_result = await httpClient.GetStringAsync("https://m.facebookcorewwwi.onion/messages/");
                        doc.LoadHtml(chats_result);
                        Clear();
#else
                    doc.Load("C:\\Users\\Maciek\\Desktop\\Projects\\chat.html");
#endif

                        HtmlNodeCollection conversations = doc.DocumentNode
                           .SelectNodes("/html/body/div/div/div[3]/div[2]/div[2]/div[2]/div[1]/table");

                        Console.WriteLine("Rozmowy: ");
                        int i = 0;
                        foreach (var node in conversations)
                        {
                            i++;
                            Console.Write(i + ". ");
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" " + node.SelectSingleNode("tr/td/div/h3").InnerText + " ");
                            Console.ResetColor();
                            Console.Write(" [" + node.SelectSingleNode("tr/td/div/h3[3]/span/abbr").InnerText + "]\n");
                            Console.WriteLine("   " + EntityToUnicode(node.SelectSingleNode("tr/td/div/h3[2]/span").InnerText.Split('\n')[0]));
                        }

                        Console.WriteLine(" ======= ");
                        i++;
                        Console.WriteLine(i + ". Menu");
                        i++;
                        Console.WriteLine(i + ". Odśwież");

                        Console.Write(": ");

                        if (!int.TryParse(Console.ReadLine(), out chselect)) chselect = -1;
                        if (chselect == i - 1)
                        {
                            select = 10;
                        }
                        else if (chselect < i)
                        {
                            bool chatting = true;
                            string chat_name = conversations.ElementAt(chselect - 1).SelectSingleNode("tr/td/div/h3").InnerText;
                            while (chatting)
                            {
                                Clear();
                                Console.WriteLine("Chat z: " + chat_name);

                                Console.WriteLine("Ładowanie...");
#if !test
                                string chat_result = await httpClient.GetStringAsync("https://m.facebookcorewwwi.onion" + conversations.ElementAt(chselect - 1).SelectSingleNode("tr/td/div/h3/a").GetAttributeValue("href", ""));
                                doc.LoadHtml(chat_result);
#else
                            doc.Load("C:\\Users\\Maciek\\Desktop\\Projects\\jacob.html");
#endif
                                Clear();
                                Console.WriteLine("Chat z: " + chat_name + "\n");

                                HtmlNodeCollection msgs = doc.DocumentNode
                                    .SelectNodes("/html/body/div/div/div[3]/div/div[1]/div[2]/div[2]/div");

                                foreach (var node in msgs)
                                {
                                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.Write(" " + node.SelectSingleNode("div[1]/a/strong").InnerText + " ");
                                    Console.ResetColor();
                                    Console.Write(" [" + node.SelectSingleNode("div[2]/abbr").InnerText + "]\n");
                                    foreach (var n in node.SelectNodes("div[1]/div"))
                                    {
                                        if (n.InnerHtml.Contains("attachment"))
                                        {
                                            Console.WriteLine(" [Obrazek] ");
                                        }
                                        else
                                        {
                                            Console.WriteLine(" " + HttpUtility.HtmlDecode(n.InnerText));
                                        }
                                    }
                                }

                                Console.Write(": ");
                                string msg = Console.ReadLine();
                                if (msg.StartsWith("/"))
                                {
                                    if (msg == "/exit")
                                    {
                                        chatting = false;
                                        chselect = -1;
                                        select = 2;
                                    }
                                    else
                                    {
                                        Clear();
                                        Console.WriteLine("Nieznana komenda");
                                        Console.ReadLine();
                                    }
                                }
                                else
                                {
                                    var form = doc.GetElementbyId("composer_form").SelectNodes("input");
#if !test
                                    var msg_c = new FormUrlEncodedContent(new[]
                                    {
                                    new KeyValuePair<string, string>("fb_dtsg", form.ElementAt(0).GetAttributeValue("value", "")),
                                    new KeyValuePair<string, string>(form.ElementAt(4).GetAttributeValue("name", ""), form.ElementAt(4).GetAttributeValue("value", "")),
                                    new KeyValuePair<string, string>("body", msg)
                                });
                                    Clear();
                                    Console.WriteLine("Wysyłanie...");
                                    await httpClient.PostAsync("https://m.facebookcorewwwi.onion/messages/send/?icm=1&refid=12", msg_c);
#endif
                                }
                            }
                        }
                        #endregion
                    }
                    else if (select == 4)
                    {
                        running = false;
                    }
                    else
                    {
                        select = 3;
                    }
                } catch (Exception)
                {
                    Clear();
                    Console.WriteLine("Wystąpił błąd krytyczny. Kliknij se coś żeby kontynuować...");
                    select = -1;
                    chselect = -1;
                    Console.Read();
                }
            }

#if !test
            proxy.Stop();
#endif
        }

        static string EntityToUnicode(string html)
        {
            var replacements = new Dictionary<string, string>();
            var regex = new Regex("(&[a-z]{2,5};)");
            foreach (Match match in regex.Matches(html))
            {
                if (!replacements.ContainsKey(match.Value))
                {
                    var unicode = HttpUtility.HtmlDecode(match.Value);
                    if (unicode.Length == 1)
                    {
                        replacements.Add(match.Value, string.Concat("&#", Convert.ToInt32(unicode[0]), ";"));
                    }
                }
            }
            foreach (var replacement in replacements)
            {
                html = html.Replace(replacement.Key, replacement.Value);
            }
            return html;
        }
        static void Clear()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Torbook, v. 0.0.28 [" + ip + "]");
            Console.ResetColor();
        }
    }
}
