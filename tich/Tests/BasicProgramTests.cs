using libtich;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class BasicProgramTests
{
    
    [Test]
    public void empty_program_returns_input_x_coord()
    {
        var subject = new TichProgram(Array.Empty<Cell>());
        
        var result = subject.CalculateForPoint(10, 20);
        
        Assert.That(result, Is.EqualTo(10));
    }
    
    [Test]
    public void return_a_single_value()
    {
        var subject = new TichProgram(new[]{
            C(1.23)
        });
        
        var result = subject.CalculateForPoint(10, 10);
        
        Assert.That(result, Is.EqualTo(1.23));
    }
    
    
    [Test]
    public void can_push_target_point()
    {
        var subject = new TichProgram(new[]{
            C(Command.P),
            C(Command.Add)
        });
        
        var result = subject.CalculateForPoint(10, 1);
        
        Assert.That(result, Is.EqualTo(20));
    }
    
    [Test]
    public void calculate_distance()
    {
        var subject = new TichProgram(new[]{
            C(5),
            C(3),
            C(Command.Vec2),
            C(Command.Sub),
            C(Command.Length)
        });
        
        var result = subject.CalculateForPoint(10, 10);
        var expected = Math.Sqrt(5*5 + 7*7);
        Assert.That(result, Is.EqualTo(expected));
        
        result = subject.CalculateForPoint(3, 3);
        expected = Math.Sqrt(2*2);
        Assert.That(result, Is.EqualTo(expected));
    }
    
    [Test]
    public void incorrect_program_stops()
    {
        var subject = new TichProgram(new[]{
            C(Command.Invalid)
        });
        var result = subject.CalculateForPoint(10, 10);
        Assert.That(result, Is.EqualTo(10)); // stops before pushing the value, P is still top of stack
    }

    [Test]
    public void other_values_on_stack_are_ignored()
    {
        var subject = new TichProgram(new[]{
            C((double)0),
            C(1),
            C(2),
            C(3),
            C(4),
            C(5),
        });
        var result = subject.CalculateForPoint(10, 10);
        Assert.That(result, Is.EqualTo(5)); // returns only top of stack
    }

    private Cell C(double p) => new() { Cmd = Command.Scalar, NumberValue = p};
    private Cell C(Command cmd) => new() { Cmd = cmd};
}