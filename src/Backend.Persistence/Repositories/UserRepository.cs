using Backend.Application.Abstractions;
using Backend.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        => _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        => _context.Users.FirstOrDefaultAsync(u => u.Email.Value == email, ct);

    public async Task AddAsync(User user, CancellationToken ct)
        => await _context.Users.AddAsync(user, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) 
        => _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
}