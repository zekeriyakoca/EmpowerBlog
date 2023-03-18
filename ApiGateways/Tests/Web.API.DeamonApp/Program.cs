
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

//Wait for trigger to make the call
//Console.ReadLine();

var clientID = "0a5e8bc9-571c-4cfd-9f46-f0bce243836c";
var clientSecret = "tBr8Q~8RZRTzp4vZ9s4SvYA2TbnwAYTq48OQQbxi";
var aadTenantDomain = "95090b01-7b00-476f-8a70-c6820af4a2d1";
var authority = $"https://login.microsoftonline.com/{aadTenantDomain}";

var clientApplication = ConfidentialClientApplicationBuilder.Create(clientID)
  .WithClientSecret(clientSecret)
  .WithAuthority(authority)
  .Build();

var webApiTenantId = "42f32859-4731-4e08-b8c7-8cd3d1fa3e51";
var scopes = new[] { $"api://{webApiTenantId}/.default" };
var result = await clientApplication.AcquireTokenForClient(scopes)
                .ExecuteAsync();

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

var apiBaseUrl = "https://localhost:44370";
var endpointUrl = $"{apiBaseUrl}/api/Blog/28013d52-5436-4c96-9d0d-8612d8725000";
//var endpointUrl = $"{apiBaseUrl}/api/Blog/Test";

// Call the web API.
HttpResponseMessage response = await httpClient.GetAsync(endpointUrl);
var resp = response.Content.ReadAsStringAsync().Result;


Console.WriteLine(resp);

