﻿using System.Diagnostics;
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

    [Test]
    [TestCase("ring", "abs(len(p) - 52) - 5", 128, 128)]
    [TestCase("non-square_wide", "length(abs(p)-min(abs(p).x+abs(p).y, 64)*0.5) - 16", 256, 128)]
    [TestCase("non-square_tall", "length(abs(p)-min(abs(p).x+abs(p).y, 64)*0.5) - 16", 128, 256)]
    [TestCase("large", "len(p - 448)-384", 1024, 1024)]
    public void test_cases(string name, string expression, int width, int height)
    {
        var sw = new Stopwatch();
        sw.Start();
        var prog = TichProgram.Compile(expression); // radius 5 circle at (10,10)
        var ser = prog.Serialise();

        Console.WriteLine($"Compile took {sw.Elapsed}");
        Console.WriteLine($"Program {ser.Length} bytes: {string.Join("", ser.Select(v => v.ToString("X2")))}");

        sw.Restart();
        var result = TichRenderer.RenderLayer(prog, width, height, 64, 64);
        sw.Stop();
        Console.WriteLine($"Render took {sw.Elapsed}");

        result.ToImage($"C:\\temp\\{name}.bmp");
    }
}