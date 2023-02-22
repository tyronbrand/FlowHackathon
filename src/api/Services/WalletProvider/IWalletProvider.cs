using Hackathon.Api.Services.WalletProvider.Models;

namespace Hackathon.Api.Services.WalletProvider
{
    public interface IWalletProvider
    {
        Task<WalletDetails> CreateWalletAsync();
        Task<WalletDetails> GetWalletByIdAsync(string id);
    }
}
