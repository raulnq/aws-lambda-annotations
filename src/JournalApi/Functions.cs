using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2.DataModel;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JournalApi;

public record RegisterNewsRequest(string Description, string Title);

public record RegisterNewsResponse(Guid Id);

[DynamoDBTable("newstable")]
public class News
{
    [DynamoDBHashKey("id")]
    public Guid Id { get; set; }
    [DynamoDBProperty("description")]
    public string? Description { get; set; }
    [DynamoDBProperty("title")]
    public string? Title { get; set; }
}


public class Functions
{
    private readonly DynamoDBContext _dbContext;
    public Functions(DynamoDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [LambdaFunction(MemorySize = 512)]
    [HttpApi(LambdaHttpMethod.Post, "/")]
    public async Task<RegisterNewsResponse> RegisterNews([FromBody]RegisterNewsRequest request)
    {
        var news = new News() { Id = Guid.NewGuid(), Description = request.Description, Title = request.Title };

        await _dbContext.SaveAsync(news);

        return new RegisterNewsResponse(news.Id);
    }

    [LambdaFunction(MemorySize = 512)]
    [HttpApi(LambdaHttpMethod.Get, "/")]
    public async Task<List<News>> ListNews()
    {
        return await _dbContext.ScanAsync<News>(default).GetRemainingAsync();
    }

    [LambdaFunction(MemorySize = 512)]
    [HttpApi(LambdaHttpMethod.Get, "/{id}")]
    public async Task<News> GetNews(string id)
    {
        return await _dbContext.LoadAsync<News>(Guid.Parse(id));
    }
}
