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
        var prog = TichProgram.Compile("len(p - 10)-5"); // radius 5 circle at (10,10)
        var ser = prog.Serialise();
        Console.WriteLine($"Program {ser.Length} bytes: {string.Join("", ser.Select(v=>v.ToString("X2")))}");
        
        var sw = new Stopwatch();
        sw.Start();
        var result = TichRenderer.RenderLayer(prog, 24, 24);
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
}