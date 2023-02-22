using GraphQL;
using GraphQL.Client.Abstractions;
using Hackathon.Api.Services.WalletProvider.Models;
using Hackathon.Api.Services.WalletProvider.Niftory.Models;

namespace Hackathon.Api.Services.WalletProvider.Niftory
{
    public class Niftory : IWalletProvider
    {
        private readonly IGraphQLClient _graphQLClient;

        public Niftory(IGraphQLClient graphQLClient)
        {
            _graphQLClient = graphQLClient;
        }


        public async Task<WalletDetails> CreateWalletAsync()
        {
            var query = new GraphQLRequest
            {
                Query = @"
                mutation CreateWallet {
                    createNiftoryWallet {
                        id
                        address
                        state
                    }
                }",
            };

            var response = await _graphQLClient.SendMutationAsync<CreateWalletResponse>(query);
            return response.Data.CreateNiftoryWallet;
        }

        public async Task<WalletDetails> GetWalletByIdAsync(string id)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                query WalletById($id: ID!) {
                  walletById(id: $id) {
                    id
                    address
                    state
                  }
                }",
                Variables = new { id }
            };

            var response = await _graphQLClient.SendQueryAsync<WalletByIdResponse>(query);
            return response.Data.WalletById;
        }
    }
}
