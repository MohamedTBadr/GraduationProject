using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OllamaService : IOllamaService
    {
        private readonly OllamaApiClient _client;

        public OllamaService(IConfiguration config)
        {
            _client = new OllamaApiClient(config["Ollama:Url"]!);
            _client.SelectedModel = config["Ollama:Model"]!;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var sb = new StringBuilder();

            await foreach (var chunk in _client.GenerateAsync(prompt))
            {
                sb.Append(chunk?.Response);
            }

            return sb.ToString();
        }
    }

}
