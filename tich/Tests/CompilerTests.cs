using libtich;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class CompilerTests
{
    // TODO: flatten these out to expression strings -> final values
    
    [Test] // a function on one side of an operation, a value on the other
    public void very_basic_expression_1()
    {
        var postfix = Compiler.InfixToPostfix("length(p) - 3.0"); // circle centred at 0,0 with radius 3
        Assert.That(postfix, Is.Not.Null);
        
        var result = Compiler.CompilePostfix(postfix).ToList();
        Assert.That(result, Is.Not.Null);
        
        Console.WriteLine(result.PrettyPrint());
        
        Assert.Inconclusive("not done");
    }
    
    
    [Test] // two values on either side of an operation
    public void very_basic_expression_2()
    {
        var postfix = Compiler.InfixToPostfix("5.0 - 3.0");
        Assert.That(postfix, Is.Not.Null);
        
        var result = Compiler.CompilePostfix(postfix).ToList();
        Assert.That(result, Is.Not.Null);
        
        Console.WriteLine(result.PrettyPrint());
        
        Assert.Inconclusive("not done");
    }
    
    [Test] // two expression on either side of an operation
    public void very_basic_expression_3()
    {
        var postfix = Compiler.InfixToPostfix("length(vec2(5, 8)) - length(p)");
        Assert.That(postfix, Is.Not.Null);
        
        var result = Compiler.CompilePostfix(postfix).ToList();
        Assert.That(result, Is.Not.Null);
        
        Console.WriteLine(result.PrettyPrint());
        
        Assert.Inconclusive("not done");
    }
    
    
    [Test] // using constants
    public void very_basic_expression_4()
    {
        var postfix = Compiler.InfixToPostfix("pi - length(p)");
        Assert.That(postfix, Is.Not.Null);
        
        var result = Compiler.CompilePostfix(postfix).ToList();
        Assert.That(result, Is.Not.Null);
        
        Console.WriteLine(result.PrettyPrint());
        
        Assert.Inconclusive("not done");
    }
}