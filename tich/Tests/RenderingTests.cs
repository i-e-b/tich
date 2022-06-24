using System.Diagnostics;
using System.Drawing;
using System.Text;
using libtich;
using NUnit.Framework;
using Tests.Helpers;

namespace Tests;

[TestFixture]
public class RenderingTests
{
    [Test]
    public void basic_circle()
    {
        var prog = TichProgram.Compile("len(p)-5"); // radius 5 circle at (0,0)
        var ser = prog.Serialise();
        Console.WriteLine($"Program {ser.Length} bytes: {string.Join("", ser.Select(v=>v.ToString("X2")))}");
        
        var sw = new Stopwatch();
        sw.Start();
        var result = TichRenderer.RenderLayer(prog, 24, 24, 10, 10);
        sw.Stop();
        Console.WriteLine($"Render took {sw.Elapsed}");
        
        result.ToImage(@"C:\temp\basic_circle.bmp");
        
        for (int y = 0; y < 24; y++)
        {
            var sb = new StringBuilder();
            for (int x = 0; x < 24; x++)
            {
                var value = (int)(result[x,y] * 255);
                if (value > 0.99) sb.Append('#');
                else if (value < 0.01) sb.Append(' ');
                else sb.Append('.');
            }
            Console.WriteLine(sb.ToString());
        }
    }
    
    /*

float d = sdStar( p, 0.7, 5, 5.0/2.0 );
float sdStar(in vec2 p, in float r, in int n, in float m) // m=[2,n]
{
    // these 4 lines can be precomputed for a given shape
    float an = 3.141593/float(n);
    float en = 3.141593/m;
    vec2  acs = vec2(cos(an),sin(an));
    vec2  ecs = vec2(cos(en),sin(en)); // ecs=vec2(0,1) and simplify, for regular polygon,

    // reduce to first sector
    float bn = mod(atan(p.x,p.y),2.0*an) - an;
    p = length(p)*vec2(cos(bn),abs(sin(bn)));

    // line sdf
    p -= r*acs;
    p += ecs*clamp( -dot(p,ecs), 0.0, r*acs.y/ecs.y);
    return length(p)*sign(p.x);
}

to

float d = sdStar( p, 0.7, 5, 5.0/2.0 );
float sdStar(in vec2 p, in float k, in int n, in float m) // m=[2,n]
{
    n:5;
    m:5.0/2.0;
    k: 10.0;
    
    a : 3.141593/n;
    b : 3.141593/m;
    c : vec2(cos(a),sin(a));
    d : vec2(cos(b),sin(b));

    e : mod(atan(p.x,p.y),2.0*a) - a;
    p : length(p)*vec2(cos(e),abs(sin(e)));

    p: p - k*c;
    p: p + d*clamp( -dot(p,d), 0.0, k*c.y/d.y);
    length(p)*sign(p.x);
}

n:5; m:5.0/2.0; k: 10.0; a : 3.141593/n; b : 3.141593/m; c : vec2(cos(a),sin(a)); d : vec2(cos(b),sin(b));
e : mod(atan(p.x,p.y),2.0*a) - a; p : length(p)*vec2(cos(e),abs(sin(e))); p: p - k*c; p: p + d*clamp( -dot(p,d), 0.0, k*c.y/d.y);
length(p)*sign(p.x);


     */

    [Test]
    [TestCase("ring", "abs(len(p) - 52) - 5", 128, 128)]
    [TestCase("large", "len(p - 448)-384", 1024, 1024)]
    [TestCase("non-square_wide", "length(abs(p)-min(abs(p).x+abs(p).y, 64)*0.5) - 16", 256, 128)]
    [TestCase("non-square_tall", "length(abs(p)-min(abs(p).x+abs(p).y, 64)*0.5) - 16", 128, 256)]
    
    [TestCase("with-subexpressions", "n:5; m:5.0/2.0; k: 64.0; a : pi/n; b : pi/m; c : vec2(cos(a),sin(a)); d : vec2(cos(b),sin(b));" +
                                     "e : (atan(p.x,p.y) % (2.0*a)) - a; p : length(p)*vec2(cos(e),abs(sin(e))); p: p - k*c; p:p + d*clamp( -dot(p,d), 0.0, k*c.y/d.y);" +
                                     "length(p)*sign(p.x);", 128, 128)]
    public void test_cases(string name, string expression, int width, int height)
    {
        var sw = new Stopwatch();
        sw.Start();
        var prog = TichProgram.Compile(expression); // radius 5 circle at (10,10)
        sw.Stop();
        var ser = prog.Serialise();
        var desc = prog.Describe();

        Console.WriteLine($"Compile took {sw.Elapsed}");
        Console.WriteLine($"Program {ser.Length} bytes: {string.Join("", ser.Select(v => v.ToString("X2")))}");
        Console.WriteLine(desc);

        sw.Restart();
        var result = TichRenderer.RenderLayer(prog, width, height, 64, 64);
        sw.Stop();
        Console.WriteLine($"Render took {sw.Elapsed}");

        sw.Restart();
        result.ToImage($"C:\\temp\\{name}.bmp");
        sw.Stop();
        Console.WriteLine($"dotnet image write took {sw.Elapsed}");
    }
}