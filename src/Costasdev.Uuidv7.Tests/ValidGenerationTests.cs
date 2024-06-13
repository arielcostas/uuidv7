namespace Costasdev.Uuidv7.Tests;

public class ValidGenerationTests
{
    [Fact]
    public void TestUuidIsValid()
    {
        var uuid = Uuid7.NewUuid().ToString();
        Assert.Equal('7', uuid[14]);
        
        var variant = uuid[19];
        Assert.True(variant is '8' or '9' or 'a' or 'b');
    }
}