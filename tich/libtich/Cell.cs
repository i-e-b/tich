using System.Text;

namespace libtich;

/// <summary>
/// A command, plus any parameters read
/// </summary>
public class Cell
{
    /// <summary>
    /// Command for this cell
    /// </summary>
    public Command Cmd = Command.Invalid;
    /// <summary>
    /// Parameters for the command
    /// </summary>
    public double[] Params = Array.Empty<double>();

    /// <summary>
    /// Return a human readable string representation of this cell
    /// </summary>
    public override string ToString()
    {
        return $"{Cmd}({string.Join(", ", Params)})";
    }

    /// <summary>
    /// Serialise this cell to a compact representation
    /// </summary>
    public byte[] ToByteString()
    {
        // TODO: either int8, fix8:8, or fix16:16?
        // Could also have an int4 for swizzle indexes
        // 6 bits would give 63 commands and 2 spare to encode the param types
        
        // Initially, we just do it the simple way
        var result = new List<byte>();
        result.Add((byte)Cmd);
        foreach (var param in Params)
        {
            result.AddRange(To16_16(param));
        }
        
        return result.ToArray();
    }

    /// <summary>
    /// Deserialise from compact representation
    /// </summary>
    public static Cell FromByteString(byte[] data, int offset, out int used)
    {
        used = 1;
        var cell = new Cell
        {
            Cmd = (Command)data[offset]
        };
        
        var ac = ArgCount(cell.Cmd);
        if (ac == 0) return cell;
        
        var end = offset+1+(ac*4);
        if (end > data.Length) throw new Exception($"Unexpected end of cell ({end} vs {data.Length})"); // TODO: switch to recover once working
        
        var idx = offset+1;
        cell.Params = new double[ac];
        for (int i = 0; i < ac; i++)
        {
            cell.Params[i] = From16_16(data, idx);
            idx += 4;
            used += 4;
        }
        return cell;
    }

    private static double From16_16(byte[] data, int idx)
    {
        var i = data[idx+3] + (data[idx + 2]<<8) + (data[idx+1]<<16) + (data[idx]<<24);
        return i / 65536.0;
    }

    private static byte[] To16_16(double d)
    {
        var result = new byte[4];
        var i = (int)(d * 65536);
        result[0] = (byte)((i >> 24) & 0xff);
        result[1] = (byte)((i >> 16) & 0xff);
        result[2] = (byte)((i >>  8) & 0xff);
        result[3] = (byte)((i >>  0) & 0xff);
        return result;
    }

    private static int ArgCount(Command cmd)
    {
        return GetAttributeOfType<ArgCountAttribute>(cmd)?.Count ?? 0;
    }
    
    /// <summary>
    /// Gets an attribute on an enum field value
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
    /// <param name="enumVal">The enum value</param>
    /// <returns>The attribute of type T that should exist on the enum value</returns>
    private static T? GetAttributeOfType<T>(Enum enumVal) where T:Attribute
    {
        var type = enumVal.GetType();
        var name = enumVal.ToString();
        var memInfo = type.GetMember(name);
        if (memInfo.Length < 1) return null;
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? (T?)attributes[0] : null;
    }
}

/// <summary>
/// Helpers around the program cell
/// </summary>
public static class CellExtensions
{
    /// <summary>
    /// Generate a human readable string from a program
    /// </summary>
    public static string PrettyPrint(this IEnumerable<Cell> program)
    {
        var sb = new StringBuilder();

        foreach (var cell in program)
        {
            sb.AppendLine(cell.ToString());
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate a human readable string from a set of parser tokens
    /// </summary>
    public static string PrettyPrint(this IEnumerable<Token> tokens)
    {
        var sb = new StringBuilder();

        foreach (var t in tokens)
        {
            if (t.Class == TokenClass.Function) sb.AppendLine($"{t.Value} ({t.Class}/{t.ParameterCount})");
            else sb.AppendLine($"{t.Value} ({t.Class})");
        }
        
        return sb.ToString();
    }
    
    
    /// <summary>
    /// Generate a human readable string from a set of program line descriptions
    /// </summary>
    public static string PrettyPrint(this IEnumerable<string> program)
    {
        var sb = new StringBuilder();

        foreach (var str in program)
        {
            sb.AppendLine(str);
        }
        
        return sb.ToString();
    }
}