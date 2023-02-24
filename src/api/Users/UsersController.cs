using Hackathon.Api.Services.WalletProvider;
using Hackathon.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace Hackathon.Api;

[ApiController]
[Route("/api/users")]
public class UsersController : ControllerBase
{
    private readonly UsersRepository _repository;
    private readonly IWalletProvider _walletProvider;

    public UsersController(UsersRepository repository, IWalletProvider walletProvider)
    {
        _repository = repository;
        _walletProvider = walletProvider;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<User>> GetUsers([FromQuery] string unity_Id)
    {
        return Ok(await _repository.GetUserAsync(unity_Id));
    }

    [HttpPost]
    [ProducesResponseType(201)]
    public async Task<ActionResult> CreateUsers([FromBody]CreateUser user)
    {
        var wallet = await _walletProvider.CreateWalletAsync();
        var test = await _walletProvider.GetWalletByIdAsync(wallet.Id);

        var unityUser = new User
        {
            UnityId = user.unityId,
            WalletId = wallet.Id
        };

        await _repository.AddUserAsync(unityUser);

        return CreatedAtAction(nameof(CreateUsers), new { unity_id = unityUser.UnityId }, null);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    public async Task<ActionResult> BindUsers([FromBody] BindUserToAccount user)
    {
        var wallet = await _walletProvider.CreateWalletAsync();
        var test = await _walletProvider.GetWalletByIdAsync(wallet.Id);

        var unityUser = new User
        {
            UnityId = user.unityId,
            WalletId = wallet.Id            
        };

        await _repository.AddUserAsync(unityUser);

        return CreatedAtAction(nameof(CreateUsers), new { unity_id = unityUser.UnityId }, null);
    }

    public record CreateUser(string unityId);
    public record BindUserToAccount(string unityId, string externalAccountAddress);
}