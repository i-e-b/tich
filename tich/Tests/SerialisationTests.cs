using libtich;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class SerialisationTests
{
    [Test]
    public void cell_data_survives_round_trip()
    {
        var original = new Cell { Cmd = Command.Vec4, Params = new[] { 1.0, 2.0, 3.0, 4.0 } };
        Console.WriteLine(original);
        
        var bytes = original.ToByteString();
        Console.WriteLine(string.Join("", bytes.Select(b=>b.ToString("X2"))));
        
        var restored = Cell.FromByteString(bytes, 0, out var used);
        Console.WriteLine(restored);
        
        Assert.That(used, Is.EqualTo(bytes.Length), "bytes consumed");
        Assert.That(restored.Cmd, Is.EqualTo(original.Cmd), "cmd");
        for (int i = 0; i < 4; i++)
        {
            Assert.That(restored.Params[i], Is.EqualTo(original.Params[i]), $"params[{i}]");
        }
    }
}