using System.Security.Cryptography;

namespace Costasdev.Uuidv7.Tests;

public class ConstructorTests
{
    [Fact]
    public void TestValid()
    {
        const long timePart = 1718313494L * 1000;
        var randomPart = new byte[10];
        RandomNumberGenerator.Fill(randomPart);

        randomPart[0] = (byte)(randomPart[0] & 0x0F | 0x70);
        randomPart[2] = (byte)(randomPart[2] & 0x3F | 0x80);

        var uuid = new Uuid7(timePart, randomPart);
        Assert.Equal(timePart, uuid.GetDateTimeOffset().ToUnixTimeMilliseconds());
    }

    [Fact]
    public void TestInvalidRandomLength()
    {
        const long timePart = 1718313494L * 1000;
        var randomPart = new byte[9];

        randomPart[0] = (byte)(randomPart[0] & 0x0F | 0x70);
        randomPart[2] = (byte)(randomPart[2] & 0x3F | 0x80);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => new Uuid7(timePart, randomPart));
    }

    [Fact]
    public void TestInvalidVersion()
    {
        const long timePart = 1718313494L * 1000;
        var randomPart = new byte[10];
        RandomNumberGenerator.Fill(randomPart);

        randomPart[0] = (byte)(randomPart[0] & 0x0F | 0x40);
        randomPart[2] = (byte)(randomPart[2] & 0x3F | 0x80);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => new Uuid7(timePart, randomPart));
    }
    
    [Fact]
    public void TestInvalidVariant()
    {
        const long timePart = 1718313494L * 1000;
        var randomPart = new byte[10];
        RandomNumberGenerator.Fill(randomPart);

        randomPart[0] = (byte)(randomPart[0] & 0x0F | 0x70);
        randomPart[2] = (byte)(randomPart[2] & 0x3F | 0x40);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => new Uuid7(timePart, randomPart));
    }
}