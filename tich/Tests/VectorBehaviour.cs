using libtich;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class VectorBehaviour
{
    [Test]
    public void scalar_times_vector_applies_to_all_values()
    {
        // 2 * vec3(1,2,3) --> vec3(2,4,6)
        var subject = new TichProgram(new[]{
            C(1.0),
            C(2.0),
            C(3.0),
            C(Command.Vec3),
            C(2.0),
            C(Command.Mul)
        });
        
        var stack = new Stack<Variant>();
        subject.Evaluate(stack, Variant.Vec2(0,0));
        
        var result = stack.Pop();
        
        Assert.That(result.Width, Is.EqualTo(3), "output vector should be 3-ary");
        Assert.That(result.X, Is.EqualTo(2), "X doubled");
        Assert.That(result.Y, Is.EqualTo(4), "Y doubled");
        Assert.That(result.Z, Is.EqualTo(6), "Z doubled");
    }

    private Cell C(double p) => new() { Cmd = Command.Scalar, NumberValue = p };
    private Cell C(Command cmd) => new() { Cmd = cmd };
}