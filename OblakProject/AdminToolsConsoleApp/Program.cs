using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AdminToolsConsoleApp
{
    internal static class Endpoints
    {
        // Ako kolege promene rute – izmeni ovde:
        public const string GetEmails = "/api/alerts/emails";
        public const string PostEmail = "/api/alerts/emails";          // body: { email }
        public const string DeleteEmailFmt = "/api/alerts/emails/{0}";      // {email}
        public const string PutEmails = "/api/alerts/emails";          // body: [ "a@x", "b@y" ]

        public const string VerifyByIdFmt = "/api/users/{0}/verify-author"; // {userId}
        public const string VerifyByEmail = "/api/users/verify-author-by-email"; // body: { email }
    }

    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0) { PrintHelp(); return 1; }

            var dryRun = args.Any(a => a.Equals("--dry-run", StringComparison.OrdinalIgnoreCase));
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "";
            var apiKey = ConfigurationManager.AppSettings["ApiKey"] ?? "";

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                Console.WriteLine("❌ BaseUrl nije podešen u App.config.");
                return 2;
            }

            using (var http = new HttpClient { BaseAddress = new Uri(baseUrl) })
            {
                if (!string.IsNullOrWhiteSpace(apiKey))
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var cmd = args[0].ToLowerInvariant();
                var tail = args.Skip(1).Where(a => !a.Equals("--dry-run", StringComparison.OrdinalIgnoreCase)).ToArray();

                try
                {
                    switch (cmd)
                    {
                        case "emails":
                            await HandleEmails(http, tail, dryRun);
                            return 0;

                        case "verify-author":
                            await HandleVerifyAuthor(http, tail, dryRun);
                            return 0;

                        case "help":
                        case "-h":
                        case "--help":
                            PrintHelp();
                            return 0;

                        default:
                            Console.WriteLine("Nepoznata komanda.\n");
                            PrintHelp();
                            return 1;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("HTTP greška: " + ex.Message);
                    return 3;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška: " + ex.Message);
                    return 4;
                }
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("AdminToolsConsoleApp – komande:\n");
            Console.WriteLine(" emails list [--dry-run]");
            Console.WriteLine(" emails add <email@domain> [--dry-run]");
            Console.WriteLine(" emails remove <email@domain> [--dry-run]");
            Console.WriteLine(" emails set <email1,email2,...> [--dry-run]");
            Console.WriteLine();
            Console.WriteLine(" verify-author --userId <id> [--dry-run]");
            Console.WriteLine(" verify-author --email <email@domain> [--dry-run]");
            Console.WriteLine();
            Console.WriteLine(" Primeri:");
            Console.WriteLine("   AdminToolsConsoleApp.exe emails list");
            Console.WriteLine("   AdminToolsConsoleApp.exe emails add ops@firma.com");
            Console.WriteLine("   AdminToolsConsoleApp.exe emails set ops@firma.com,dev@firma.com");
            Console.WriteLine("   AdminToolsConsoleApp.exe verify-author --userId 42");
            Console.WriteLine("   AdminToolsConsoleApp.exe verify-author --email user@example.com");
        }

        static async Task HandleEmails(HttpClient http, string[] args, bool dry)
        {
            if (args.Length == 0) { PrintHelp(); return; }
            var sub = args[0].ToLowerInvariant();

            switch (sub)
            {
                case "list":
                    if (dry) { Console.WriteLine($"[dry-run] GET {Endpoints.GetEmails}"); return; }
                    {
                        var resp = await http.GetAsync(Endpoints.GetEmails);
                        await PrintResponseArray(resp, "Email adrese za upozorenja");
                    }
                    break;

                case "add":
                    if (args.Length < 2) { Console.WriteLine("Nedostaje email."); return; }
                    {
                        var body = new { email = args[1] };
                        var json = JsonConvert.SerializeObject(body);
                        if (dry) { Console.WriteLine($"[dry-run] POST {Endpoints.PostEmail}\n{json}"); return; }

                        var resp = await http.PostAsync(Endpoints.PostEmail,
                            new StringContent(json, Encoding.UTF8, "application/json"));
                        await PrintStatus(resp, "Dodato.");
                    }
                    break;

                case "remove":
                    if (args.Length < 2) { Console.WriteLine("Nedostaje email."); return; }
                    {
                        var url = string.Format(Endpoints.DeleteEmailFmt, Uri.EscapeDataString(args[1]));
                        if (dry) { Console.WriteLine($"[dry-run] DELETE {url}"); return; }

                        var resp = await http.DeleteAsync(url);
                        await PrintStatus(resp, "Uklonjeno.");
                    }
                    break;

                case "set":
                    if (args.Length < 2) { Console.WriteLine("Nedostaju email-ovi."); return; }
                    {
                        var emails = args[1]
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .ToArray();
                        var json = JsonConvert.SerializeObject(emails);
                        if (dry) { Console.WriteLine($"[dry-run] PUT {Endpoints.PutEmails}\n{json}"); return; }

                        var resp = await http.PutAsync(Endpoints.PutEmails,
                            new StringContent(json, Encoding.UTF8, "application/json"));
                        await PrintStatus(resp, "Postavljeno.");
                    }
                    break;

                default:
                    PrintHelp();
                    break;
            }
        }

        static async Task HandleVerifyAuthor(HttpClient http, string[] args, bool dry)
        {
            string userId = null, email = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("--userId", StringComparison.OrdinalIgnoreCase)) userId = args[i + 1];
                if (args[i].Equals("--email", StringComparison.OrdinalIgnoreCase)) email = args[i + 1];
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var url = string.Format(Endpoints.VerifyByIdFmt, Uri.EscapeDataString(userId));
                if (dry) { Console.WriteLine($"[dry-run] POST {url} (empty body)"); return; }

                var resp = await http.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));
                await PrintStatus(resp, "Korisnik verifikovan kao autor.");
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                var body = new { email };
                var json = JsonConvert.SerializeObject(body);
                if (dry) { Console.WriteLine($"[dry-run] POST {Endpoints.VerifyByEmail}\n{json}"); return; }

                var resp = await http.PostAsync(Endpoints.VerifyByEmail,
                    new StringContent(json, Encoding.UTF8, "application/json"));
                await PrintStatus(resp, "Korisnik verifikovan kao autor.");
            }
            else
            {
                Console.WriteLine("Navedite --userId ili --email.");
                PrintHelp();
            }
        }

        static async Task PrintResponseArray(HttpResponseMessage resp, string title)
        {
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var arr = JsonConvert.DeserializeObject<string[]>(json) ?? new string[0];
                Console.WriteLine(title + ":");
                foreach (var it in arr) Console.WriteLine(" - " + it);
            }
            else
            {
                Console.WriteLine($"Greška: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                Console.WriteLine(await resp.Content.ReadAsStringAsync());
            }
        }

        static async Task PrintStatus(HttpResponseMessage resp, string okMsg)
        {
            if (resp.IsSuccessStatusCode)
                Console.WriteLine(okMsg);
            else
            {
                Console.WriteLine($"Greška: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                Console.WriteLine(await resp.Content.ReadAsStringAsync());
            }
        }
    }
}
