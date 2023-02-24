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
    public async Task<ActionResult<List<AssetLocation>>> GetCollections([FromQuery] string accountAddress)
    {
        var assetLocations = await GetCollectionIds(accountAddress);

        return Ok(assetLocations);
    }

    private async Task<List<AssetLocation>> GetCollectionIds(string accountAddress)
    {
        var topshotIds = await GetTopShotCollectionIds(accountAddress);
        //await GetNflCollectionIds(accountAddress);

        var locations = new List<AssetLocation>();

        foreach(var id in topshotIds)
        {
            locations.Add(new AssetLocation
            {
                CustomId = $"TopShot_{id}",
                Url = $"https://assets.nbatopshot.com/media/{id}/image"
            });
        }

        return locations;
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