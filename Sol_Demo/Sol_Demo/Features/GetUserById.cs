using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Sol_Demo.DTO.Responses;

namespace Sol_Demo.Features;

public class GetUserByIdQuery : IRequest<UserResponseDTO>
{
    public GetUserByIdQuery(decimal id)
    {
        Id = id;
    }

    public decimal Id { get; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponseDTO?>
{
    private readonly IDistributedCache distributedCache = null;

    public GetUserByIdQueryHandler(IDistributedCache distributedCache)
    {
        this.distributedCache = distributedCache;
    }

    public async Task<UserResponseDTO?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            string cacheKeyName = $"Users_{request.Id}";
            var userJson = await distributedCache.GetStringAsync(cacheKeyName);

            if (userJson == null)
                return null;

            var user = JsonConvert.DeserializeObject<UserResponseDTO>(userJson);

            return await Task.FromResult(user);
        }
        catch
        {
            throw;
        }
    }
}