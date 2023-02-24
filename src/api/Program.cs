using Azure.Identity;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Hackathon.Api.Services.WalletProvider;
using Hackathon.Api.Services.WalletProvider.Niftory;
using Hackathon.Api.Users;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if(!builder.Environment.IsDevelopment())
{
    var credential = new ChainedTokenCredential(new AzureDeveloperCliCredential(), new DefaultAzureCredential());
    builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"]), credential);
}

builder.Services.AddScoped<UsersRepository>();
builder.Services.AddDbContext<HackathonDb>(options =>
{
    var connectionString = builder.Configuration[builder.Configuration["AZURE_SQL_CONNECTION_STRING_KEY"]];
    options.UseNpgsql(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddScoped<IGraphQLClient>(s => {
    var graphQLClient = new GraphQLHttpClient(builder.Configuration["NIFTORY-ENDPOINT"], new SystemTextJsonSerializer());
    graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Niftory-Client-Secret", builder.Configuration["NIFTORY-CLIENT-SECRET"]);
    graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Niftory-API-Key", builder.Configuration["NIFTORY-API-KEY"]);
    return graphQLClient;
});
builder.Services.AddScoped<IWalletProvider, Niftory>();

builder.Services.AddControllers();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

builder.Services.AddSingleton(f => new FlowClientOptions { ServerUrl = ServerUrl.MainnetHost });
builder.Services.AddHttpClient<IFlowClient, FlowHttpClient>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HackathonDb>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});    

app.MapControllers();
app.Run();