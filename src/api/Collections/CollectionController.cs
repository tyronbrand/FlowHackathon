using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Hackathon.Api.Collections;
using Hackathon.Api.Services.WalletProvider;
using Hackathon.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace Hackathon.Api;

[ApiController]
[Route("/api/collections")]
public class CollectionController : ControllerBase
{
    private readonly IFlowClient _flowClient;

    public CollectionController(IFlowClient flowClient)
    {
        _flowClient = flowClient;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<List<CollectionInfo>>> GetCollections([FromQuery] string accountAddress)
    {
        var assetLocations = await GetCollectionIds(accountAddress);

        return Ok(assetLocations);
    }

    private async Task<List<CollectionInfo>> GetCollectionIds(string accountAddress)
    {
        var topshotIds = await GetTopShotCollectionIds(accountAddress);
        //await GetNflCollectionIds(accountAddress);

        var locations = new List<CollectionInfo>();
        Random rnd = new Random();

        foreach (var id in topshotIds)
        {
            var playerName = await GetTopShotCollectionMetadata(accountAddress, id);            
            
            locations.Add(new CollectionInfo
            {
                Name = playerName ?? "Unknown",
                CustomId = $"TopShot_{id}",
                Url = $"https://assets.nbatopshot.com/media/{id}/image",
                Stats = new CardStats
                {
                    Attack = rnd.Next(1, 6),
                    Cost = rnd.Next(1, 4),
                    Defence = rnd.Next(1, 4),
                }
            });
        }

        return locations;
    }

    private async Task<string?> GetTopShotCollectionMetadata(string accountAddress, string Id)
    {
        var script = @"import TopShot from 0x0b2a3299cc857e29

pub fun main(account: Address, id: UInt64): {String: String} {

    let collectionRef = getAccount(account).getCapability(/public/MomentCollection)
        .borrow<&{TopShot.MomentCollectionPublic}>()
        ?? panic(""Could not get public moment collection reference"")

    let token = collectionRef.borrowMoment(id: id)
        ?? panic(""Could not borrow a reference to the specified moment"")

    let data = token.data

    let metadata = TopShot.getPlayMetaData(playID: data.playID) ?? panic(""Play doesn't exist"")

    return metadata
}";

        var flowAddress = new FlowAddress(accountAddress);

        var arguments = new List<ICadence>
        {
            new CadenceAddress(flowAddress.Address),
            new CadenceNumber(CadenceNumberType.UInt64, Id)
        };

        try
        {
            var response = await _flowClient.ExecuteScriptAtLatestBlockAsync(
                new FlowScript
                {
                    Script = script,
                    Arguments = arguments
                });

            return response.As<CadenceDictionary>().Value.FirstOrDefault(s => s.Key.As<CadenceString>().Value == "FullName")?.Value.As<CadenceString>().Value.ToString();
        }
        catch (Exception)
        {
            //TODO
            throw new Exception("Failed");
        }
    }

    private async Task<List<string>> GetTopShotCollectionIds(string accountAddress)
    {
        var script = @"import TopShot from 0x0b2a3299cc857e29
pub fun main(account: Address): [UInt64] {

    let acct = getAccount(account)

    let collectionRef = acct.getCapability(/public/MomentCollection)
                            .borrow<&{TopShot.MomentCollectionPublic}>()!

    return collectionRef.getIDs()
}";

        var flowAddress = new FlowAddress(accountAddress);

        var arguments = new List<ICadence>
        {
            new CadenceAddress(flowAddress.Address)
        };

        try
        {
            var response = await _flowClient.ExecuteScriptAtLatestBlockAsync(
                new FlowScript
                {
                    Script = script,
                    Arguments = arguments
                });

            return response.As<CadenceArray>().Value.Select(l => ((CadenceNumber)l).Value).ToList();
        }
        catch (Exception)
        {
            //TODO
            throw new Exception("Failed");
        }
    }

    private async Task GetNflCollectionIds(string accountAddress)
    {
        var script = @"import AllDay from 0xe4cf4bdc1751c65d
pub fun main(account: Address): [UInt64] {

    let acct = getAccount(account)

    let collectionRef = acct.getCapability(/public/AllDayNFTCollection)
                            .borrow<&{AllDay.MomentNFTCollectionPublic}>()!

    return collectionRef.getIDs()
}";

        var flowAddress = new FlowAddress(accountAddress);

        var arguments = new List<ICadence>
        {
            new CadenceAddress(flowAddress.Address)
        };

        try
        {
            var response = await _flowClient.ExecuteScriptAtLatestBlockAsync(
            new FlowScript
            {
                Script = script,
                Arguments = arguments
            });
        }
        catch (Exception)
        {
            //TODO
        }
    }
}