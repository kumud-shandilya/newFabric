﻿namespace Microsoft.Fabric.Provisioning.Library
{
    using System.Net.Http.Json;
    using System.Net.Http.Headers;
    using System.Runtime.InteropServices;
    using Microsoft.Extensions.Logging;
    using Microsoft.Fabric.Provisioning.Library.Models;
    using System.Text.Json;

    public class Operations
    {
        private readonly ILogger<Operations> logger;
        private readonly HttpClient client;

        public Operations(ILogger<Operations> logger)
        {
            this.logger = logger;
            this.client = new();
        }

        public WorkspaceResponse? Create(string token, 
            WorkspaceRequest payload, 
            [Optional] string correlationId)
        {
            this.logger?.LogInformation("Invoked the 'Create' operation.");
            this.logger?.LogInformation(JsonSerializer.Serialize<WorkspaceRequest>(payload));

            this.client.BaseAddress = new Uri("https://api.fabric.microsoft.com/");
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.client.DefaultRequestHeaders.Add("User-Agent", "Microsoft Fabric Provisioning");
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "key", "secret"))));

            try
            {

                var responseMessage = this.client.PostAsJsonAsync<WorkspaceRequest>($"v1/workspaces", payload).Result;
                // responseMessage.EnsureSuccessStatusCode()

                if (responseMessage.IsSuccessStatusCode)
                {
                    return responseMessage.Content?.ReadFromJsonAsync<WorkspaceResponse>().Result;
                }
                else
                {
                    this.logger?.LogError(500, "Failed to create the resource.");
                    return default;
                }
            }
            catch (Exception ex) {
                this.logger?.LogError(500, ex, message: ex.Message);
                return default;
            }
        }
    }
}
