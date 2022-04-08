namespace libtich;

/// <summary>
/// A command, plus any parameters read
/// </summary>
public class Cell
{
    public Command Cmd = Command.Invalid;
    public double[] Params = Array.Empty<double>();
}