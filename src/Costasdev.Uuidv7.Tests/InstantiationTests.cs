namespace Costasdev.Uuidv7.Tests;

public class InstantiationTests
{
    [Fact]
    public void TestDatetimeoffsetInstantiation()
    {
        DateTime datetime = new(2024, 06, 13, 11, 29, 15);
        DateTimeOffset expected = new(datetime);

        var uuid = Uuid7.NewUuid(expected);
        var actual = uuid.GetDateTimeOffset();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void TestDatetimeInstantiation()
    {
        DateTime datetime = new(2024, 06, 13, 11, 29, 15, DateTimeKind.Utc);

        var uuid = Uuid7.NewUuid(datetime.ToUniversalTime());
        var actual = uuid.GetDateTimeOffset().UtcDateTime;
        Assert.Equal(datetime, actual);
    }
}