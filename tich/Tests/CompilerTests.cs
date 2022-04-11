using libtich;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class CompilerTests
{
    [Test]
    public void very_basic_expression()
    {
        var postfix = Compiler.InfixToPostfix("length(p) - 3.0"); // circle centred at 0,0 with radius 3
        Assert.That(postfix, Is.Not.Null);
        
        var result = Compiler.CompilePostfix(postfix);
        Assert.That(result, Is.Not.Null);
    }
}