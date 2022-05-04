using libtich;
using NUnit.Framework;
// ReSharper disable AssignNullToNotNullAttribute

namespace Tests;

[TestFixture]
public class SerialisationTests
{
    [Test]
    public void command_cell_data_survives_round_trip()
    {
        var original = new Cell { Cmd = Command.Vec4 };
        Console.WriteLine(original);
        
        var bytes = original.ToByteString();
        Console.WriteLine(string.Join("", bytes.Select(b=>b.ToString("X2"))));
        
        var restored = Cell.FromByteString(bytes, 0, out var used);
        Console.WriteLine(restored);
        
        Assert.That(used, Is.EqualTo(bytes.Length), "bytes consumed");
        Assert.That(restored.Cmd, Is.EqualTo(original.Cmd), "cmd");
    }
    
    [Test]
    public void numeric_cell_data_survives_round_trip()
    {
        var original = new Cell { Cmd = Command.Scalar, NumberValue = 3.1415};
        Console.WriteLine(original);
        
        var bytes = original.ToByteString();
        Console.WriteLine(string.Join("", bytes.Select(b=>b.ToString("X2"))));
        
        var restored = Cell.FromByteString(bytes, 0, out var used);
        Console.WriteLine(restored);
        
        Assert.That(used, Is.EqualTo(bytes.Length), "bytes consumed");
        Assert.That(restored.Cmd, Is.EqualTo(original.Cmd), "cmd");
        Assert.That(restored.NumberValue, Is.EqualTo(original.NumberValue).Within(0.001), "NumberValue");
    }

    [Test]
    public void entire_program_can_be_serialised_and_restored()
    {
        Assert.Inconclusive("not yet implemented");
    }
}