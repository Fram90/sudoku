using System.Diagnostics;

namespace Sudoku;

public struct Position
{
    public int Row;

    public int Col;

    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public static bool operator ==(Position a, Position b) => a.Col == b.Col && a.Row == b.Row;

    public static bool operator !=(Position a, Position b) => !(a == b);
}