using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Sol_Demo.DTO.Requests;
using Sol_Demo.DTO.Responses;
using Sol_Demo.Infrastructure;

namespace Sol_Demo.Features;

public class CreateUserCommand : CreateUsersRequestDTO, IRequest<bool>
{
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, bool>
{
    private readonly ApplicationDBContext applicationDBContext = null;
    private readonly IMediator mediator = null;

    public CreateUserCommandHandler(ApplicationDBContext applicationDBContext, IMediator mediator)
    {
        this.applicationDBContext = applicationDBContext;
        this.mediator = mediator;
    }

    public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
                ArgumentNullException.ThrowIfNullOrEmpty(nameof(request));

            // Mapping
            User users = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            // Save User
            await applicationDBContext.Users.AddAsync(users, cancellationToken);
            await applicationDBContext.SaveChangesAsync(cancellationToken);

            // Get User Id
            decimal id = users.Id;

            // Map User Id in Communication
            UsersCommunication userCommunication = new()
            {
                Id = id,
                EmailId = request.Communication.EmailId,
                MobileNo = request.Communication.MobileNo,
            };

            // Save Communication
            await applicationDBContext.UsersCommunications.AddAsync(userCommunication, cancellationToken);
            await applicationDBContext.SaveChangesAsync(cancellationToken);

            await mediator.Publish(new UserCreatedCacheModifiedDomainEvent(id));

            return true;
        }
        catch
        {
            throw;
        }
    }
}

public class UserCreatedCacheModifiedDomainEvent : INotification
{
    public UserCreatedCacheModifiedDomainEvent(decimal id)
    {
        Id = id;
    }

    public decimal Id { get; }
}

public class UserCreatedCacheDomainEventHandler : INotificationHandler<UserCreatedCacheModifiedDomainEvent>
{
    private readonly IDistributedCache distributedCache = null;
    private readonly ApplicationDBContext applicationDBContext = null;

    public UserCreatedCacheDomainEventHandler(IDistributedCache distributedCache, ApplicationDBContext applicationDBContext)
    {
        this.distributedCache = distributedCache;
        this.applicationDBContext = applicationDBContext;
    }

    public async Task Handle(UserCreatedCacheModifiedDomainEvent notification, CancellationToken cancellationToken)
    {
        string cacheKeyName = $"Users_{notification.Id}";

        await distributedCache.RemoveAsync(cacheKeyName);

        var user = await applicationDBContext
                       .Users
                       .Where((x) => x.Id == notification.Id)
                       .Join(
                            applicationDBContext.UsersCommunications,
                            user => user.Id,
                            communication => communication.Id,
                            (user, communication) => new UserResponseDTO
                            {
                                Id = user.Id,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Communication = new UserCommunicationResponseDTO()
                                {
                                    Id = communication.Id,
                                    EmailId = communication.EmailId,
                                    MobileNo = communication.MobileNo
                                }
                            }
                       )
                       .FirstOrDefaultAsync();

        var cacheOptions = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365 * 7000)
        };

        var userJson = JsonConvert.SerializeObject(user);
        await distributedCache.SetStringAsync(cacheKeyName, userJson, cacheOptions);
    }
}