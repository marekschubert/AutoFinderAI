using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AutoFinderAI.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        => _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        => _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await _dbContext.Users.AddAsync(user, cancellationToken);
}
