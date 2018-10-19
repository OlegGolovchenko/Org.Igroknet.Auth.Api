using Newtonsoft.Json;
using Org.Igroknet.Auth.Models;
using System;
using System.Net.Http;
using System.Text;

namespace TestApi
{
    class Program
    {
        static void Main(string[] args)
        {
            SendMessagesAsync();
            Console.ReadLine();
        }

        static async void SendMessagesAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/api/")
            };

            Console.WriteLine("Adding user");

            var result = await client.PostAsync("user/login", new StringContent(JsonConvert.SerializeObject(new AddUserViewModel
            {
                UserName = "admin@local.site",
                Password = "Admin"
            }), Encoding.UTF8, "application/json"));

            var token = "";

            if (result.IsSuccessStatusCode)
            {
                token = await result.Content.ReadAsStringAsync();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
                Console.WriteLine(token);
            }
            else
            {
                Console.WriteLine(result.StatusCode);
            }

            result = await client.GetAsync("user/test");
            if (result.IsSuccessStatusCode)
            {
                Console.WriteLine(await result.Content.ReadAsStringAsync());
            }
        }
    }
}
