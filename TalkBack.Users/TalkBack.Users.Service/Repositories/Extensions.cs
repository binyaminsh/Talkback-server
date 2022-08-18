using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using TalkBack.Users.Service.Settings;
using TalkBack.Users.Service.Entities;
using MongoDB.Driver;
using MongoDB.Bson;

namespace TalkBack.Users.Service.Repositories
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            // Any time a document of type GUID is stored into mongodb, it will be represented as a string.  
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

            services.AddSingleton(serviceProvider =>
            {
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName)
                where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(database!, collectionName);
            });

            return services;
        }
    }
}
