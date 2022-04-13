using libtich;
using NUnit.Framework;
// ReSharper disable AssignNullToNotNullAttribute

namespace Tests;

[TestFixture]
public class CompilerTests
{
    // TODO: flatten these out to expression strings -> final values

    [Test]
    [TestCase("length(p) - 3.0", 5.602325)]
    public void expression_tests(string expr, double expected)
    {
        Console.WriteLine($"Interpreting ({expr})");
        var postfix = Compiler.InfixToPostfix(expr);
        Assert.That(postfix, Is.Not.Null);
        
        var code = Compiler.CompilePostfix(postfix).ToList();
        Assert.That(code, Is.Not.Null);
        Console.WriteLine(code.PrettyPrint());
        
        var program = new TichProgram(code);
        var result = program.CalculateForPoint(5,7); // length is ~= 8.60232
        Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

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