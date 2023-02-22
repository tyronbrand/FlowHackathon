using Microsoft.EntityFrameworkCore;

namespace Hackathon.Api.Users;

public class UsersRepository
{
    private readonly HackathonDb _db;

    public UsersRepository(HackathonDb db)
    {
        _db = db;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task AddUserAsync(User item)
    {
        _db.Users.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task<User?> GetUserAsync(string unityId)
    {
        return await _db.Users.SingleOrDefaultAsync(user => user.UnityId == unityId);
    }
}