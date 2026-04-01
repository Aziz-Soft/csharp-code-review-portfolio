// xUnit tests for UserService (After.cs)
// NuGet: xunit, Moq, Microsoft.Extensions.Logging.Abstractions

public class UserServiceTests
{
    private static Mock<IConfiguration> CreateMockConfig(string connString)
    {
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns(connString);
        var mockConfig = new Mock<IConfiguration>();
        mockConfig
            .Setup(c => c.GetSection("ConnectionStrings:UsersDB"))
            .Returns(mockSection.Object);
        return mockConfig;
    }

    // ── Test 1: Happy path — user found ─────────────────────────────────────
    [Fact]
    public async Task GetUserWithProfileAsync_ReturnsUser_WhenUserExists()
    {
        var expectedUser = new User { Id = 42, Name = "Aziz", Email = "a@b.com" };
        var expectedProfile = new Profile { Bio = "Senior .NET Consultant" };

        var mockRepo = new Mock<IUserRepository>();
        mockRepo
            .Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        var profileJson = JsonSerializer.Serialize(expectedProfile);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, profileJson);
        var httpClient = new HttpClient(handler)
        { BaseAddress = new Uri("https://profile-service/") };

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient("ProfileService")).Returns(httpClient);

        var service = new UserService(
            mockFactory.Object,
            CreateMockConfig("Server=test;").Object,
            Mock.Of<ILogger<UserService>>(),
            mockRepo.Object);

        var result = await service.GetUserWithProfileAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Aziz", result.Name);
        Assert.NotNull(result.Profile);
        Assert.Equal("Senior .NET Consultant", result.Profile.Bio);
        mockRepo.Verify(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Test 2: User not found — null returned, HTTP never called ───────────
    [Fact]
    public async Task GetUserWithProfileAsync_ReturnsNull_WhenUserNotFound()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var mockFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<UserService>>();

        var service = new UserService(
            mockFactory.Object,
            CreateMockConfig("Server=test;").Object,
            mockLogger.Object,
            mockRepo.Object);

        var result = await service.GetUserWithProfileAsync(99999);

        Assert.Null(result);
        mockFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("99999")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── Test 3: Invalid userId — throws before reaching DB ──────────────────
    [Fact]
    public async Task GetUserWithProfileAsync_ThrowsArgumentException_WhenUserIdInvalid()
    {
        var mockRepo = new Mock<IUserRepository>();
        var mockFactory = new Mock<IHttpClientFactory>();

        var service = new UserService(
            mockFactory.Object,
            CreateMockConfig("Server=test;").Object,
            Mock.Of<ILogger<UserService>>(),
            mockRepo.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetUserWithProfileAsync(0));
        Assert.Contains("userId", ex.Message);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetUserWithProfileAsync(-1));

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetUserWithProfileAsync(int.MinValue));

        mockRepo.Verify(
            r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

// MockHttpMessageHandler — simulates HTTP without real network
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;

    public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
        => Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        });
}