using System.Text;

namespace libtich;

/// <summary>
/// A command, plus any parameters read
/// </summary>
public class Cell
{
    public Command Cmd = Command.Invalid;
    public double[] Params = Array.Empty<double>();

    public override string ToString()
    {
        return $"{Cmd}({string.Join(", ", Params)})";
    }
}

/// <summary>
/// Helpers around the program cell
/// </summary>
public static class CellExtensions
{
    public static string PrettyPrint(this IEnumerable<Cell> program)
    {
        var sb = new StringBuilder();

        foreach (var cell in program)
        {
            sb.AppendLine(cell.ToString());
        }
        
        return sb.ToString();
    }
}