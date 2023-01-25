using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class GmpFloatOpTest
{
    private readonly ITestOutputHelper _console;

    public GmpFloatOpTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void Add()
    {
        GmpFloat r = GmpFloat.Parse("1.5") + GmpFloat.Parse("3.25");
        Assert.Equal(4.75, r.ToDouble());
    }

    [Fact]
    public void AddInt32()
    {
        GmpFloat r = GmpFloat.Parse("1.5") + 100;
        Assert.Equal(101.5, r.ToDouble());
    }

    [Fact]
    public void Subtract()
    {
        GmpFloat r = GmpFloat.From(7.25) - GmpFloat.From(3.125);
        Assert.Equal(4.125, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32()
    {
        GmpFloat r = GmpFloat.From(7.25) - 10;
        Assert.Equal(-2.75, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32Reverse()
    {
        GmpFloat r = 10 - GmpFloat.From(7.25);
        Assert.Equal(2.75, r.ToDouble());
    }

    [Fact]
    public void Multiple()
    {
        GmpFloat r = GmpFloat.From(2.5) * GmpFloat.From(2.5);
        Assert.Equal(6.25, r.ToDouble());
    }

    [Fact]
    public void MultipleInt32()
    {
        GmpFloat r = GmpFloat.From(2.5) * 2147483647;
        Assert.Equal(5368709117.5, r.ToDouble());
    }

    [Fact]
    public void Divide()
    {
        GmpFloat r = GmpFloat.Parse(long.MinValue.ToString()) / GmpFloat.From(int.MinValue);
        Assert.Equal(1L << 32, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32()
    {
        GmpFloat r = GmpFloat.Parse((1L << 57).ToString()) / (1u << 31);
        _console.WriteLine(r.ToString());
        Assert.Equal(1 << 26, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32Reverse()
    {
        GmpFloat r = 5 / GmpFloat.From(1 << 10);
        Assert.Equal(0.0048828125, r.ToDouble());
    }

    [Fact]
    public void PowerInt32()
    {
        GmpFloat r = GmpFloat.From(2.5) ^ 10;
        Assert.Equal(9536.7431640625, r.ToDouble());
    }

    [Fact]
    public void Negate()
    {
        GmpFloat r = -GmpFloat.From(2.5);
        Assert.Equal(-2.5, r.ToDouble());
    }
}