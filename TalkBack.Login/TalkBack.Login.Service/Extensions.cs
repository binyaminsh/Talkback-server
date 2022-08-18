using Polly.Timeout;
using Polly;
using TalkBack.Login.Service.Clients;
using TalkBack.Login.Service.Dtos;
using TalkBack.Login.Service.Entities;

namespace TalkBack.Login.Service
{
    public static class Extensions
    {
        public static LoginUserDto AsDto(this LoginUser user)
        {
            return new LoginUserDto(user.Id, user.Username);
        }


        public static IServiceCollection AddCatalogClient(this IServiceCollection services)
        {
            Random jitterer = new();

            services.AddHttpClient<UserClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7255");
            })
                // "b.Or" means if we fail because of a timeout produced by the timeout policy, it will fire a timeout exception,
                // then it will also retry
                .AddTransientHttpErrorPolicy(b => b.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                    5, // retry count
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                    + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))
                    ))
                // circuit breaker pattern
                .AddTransientHttpErrorPolicy(b => b.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                    3, // number of failed requests that will go through the circuit breaker befroe opening the circuit
                    TimeSpan.FromSeconds(15)
                    ))
                // Timeout between http inter-microservices calls
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

            return services;
        }
    }
}
