namespace Costasdev.Uuidv7.Tests;

public class ComparisonTests
{
    [Fact]
    public void CompareEquals()
    {
        var uuid = Uuid7.NewUuid();
        
        Assert.Equal(0, uuid.CompareTo(uuid));
    }
    
    [Fact]
    public void CompareLessThan()
    {
        var uuid1 = Uuid7.NewUuid();
        Thread.Sleep(500);
        var uuid2 = Uuid7.NewUuid();
        
        Assert.Equal(-1, uuid1.CompareTo(uuid2));
    }
    
    [Fact]
    public void CompareGreaterThan()
    {
        var uuid1 = Uuid7.NewUuid();
        Thread.Sleep(500);
        var uuid2 = Uuid7.NewUuid();
        
        Assert.Equal(1, uuid2.CompareTo(uuid1));
    }
    
    [Fact]
    public void CompareEqualsObject()
    {
        var uuid = Uuid7.NewUuid();
        
        Assert.True(uuid.Equals((object)uuid));
    }
    
    [Fact]
    public void CompareNotEqualsObject()
    {
        var uuid1 = Uuid7.NewUuid();
        var uuid2 = Uuid7.NewUuid();
        
        Assert.False(uuid1.Equals((object)uuid2));
    }
    
    [Fact]
    public void EqualsTrue()
    {
        var uuid = Uuid7.NewUuid();
        
        Assert.True(uuid.Equals(uuid));
    }
    
    [Fact]
    public void EqualsFalse()
    {
        var uuid1 = Uuid7.NewUuid();
        var uuid2 = Uuid7.NewUuid();
        
        Assert.False(uuid1.Equals(uuid2));
    }
    
    [Fact]
    public void EqualsNull()
    {
        var uuid = Uuid7.NewUuid();
        
        Assert.False(uuid.Equals(null));
    }
    
}