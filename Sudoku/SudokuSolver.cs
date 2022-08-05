using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku;

public class SudokuSolver
{
    private int[][] _hypBoard;
    private int _cycle = 0;
    private int _currentHypothesisDepth = 0;

    private void Print(char[][] board)
    {
        for (int row = 0; row < board.Length; row++)
        {
            for (int col = 0; col < board[row].Length; col++)
            {
                var current = board[row][col];
                if (col is 1 or 4 or 7)
                    Console.Write($" {current} ");
                else
                    Console.Write(current);

                if (col < 8 && (col + 1) % 3 == 0)
                    Console.Write('|');
            }

            if (row < 8 && (row + 1) % 3 == 0)
            {
                Console.WriteLine();
                Console.Write("-----------------");
            }

            Console.WriteLine();
        }
    }

    public void SolveSudoku(char[][] board)
    {
        Init(board);
        Print(board);

        while (true)
        {
            for (int row = 0; row < board.Length; row++)
            {
                for (int col = 0; col < board[row].Length; col++)
                {
                    if (Char.IsDigit(board[row][col]))
                        continue;

                    var opts = CellOptions(board, row, col);
                    if (opts.Count == 1)
                    {
                        board[row][col] = opts.First();
                        _hypBoard[row][col] = 0;

                        Console.WriteLine("===========");
                        Console.WriteLine($"Cycle: {_cycle}. Row: {row}, Col: {col}");
                        Print(board);
                    }
                }
            }

            _cycle++;
        }


        Console.WriteLine();
    }

    private void Init(char[][] board)
    {
        _hypBoard = new int[9][];
        for (int i = 0; i < board.Length; i++)
        {
            _hypBoard[i] = new int[board[0].Length];
        }
    }

    private readonly HashSet<char> _validSet = new() {'1', '2', '3', '4', '5', '6', '7', '8', '9'};

    private HashSet<char> CellOptions(char[][] board, int row, int col)
    {
        if (Char.IsDigit(board[row][col])) //todo проверка по массиву глубины гипотезы
            return new HashSet<char>();

        var canWriteValues = new HashSet<char>();

        var rowOpts = GetRowOptions(board, row);
        var colOpts = GetColOptions(board, col);
        var boxOpts = GetBoxOptions(board, row, col);

        foreach (var c in _validSet)
        {
            if (rowOpts.Contains(c) && colOpts.Contains(c) && boxOpts.Contains(c))
            {
                canWriteValues.Add(c);
            }
        }

        var unfilledItems = GetSubBoxUnfilled(board, row, col).Where(x => x != new Position(row, col)).ToList();
        var cannotWriteInOtherCells = new HashSet<char>(_validSet);
        foreach (var unfilled in unfilledItems)
        {
            //todo memoization
            var rowOptsInner = GetRowOptions(board, unfilled.Row);
            var colOptsInner = GetColOptions(board, unfilled.Col);
            var subBoxOptsInner = GetBoxOptions(board, row, col);

            var intersection = rowOptsInner.Intersect(colOptsInner).Intersect(subBoxOptsInner).ToHashSet();

            var cannotWrite = _validSet.Except(intersection).ToHashSet();
            cannotWriteInOtherCells = cannotWriteInOtherCells.Intersect(cannotWrite).ToHashSet();
        }

        var result = canWriteValues.Intersect(cannotWriteInOtherCells).ToHashSet();
        return result;
    }

    private List<Position> GetSubBoxUnfilled(char[][] board, int row, int col)
    {
        var (middleRow, middleCol) = GetSubBoxMiddleByCoords(row, col);

        var result = new List<Position>();
        for (int i = middleRow - 1; i <= middleRow + 1; i++)
        {
            for (int j = middleCol - 1; j <= middleCol + 1; j++)
            {
                if (!Char.IsDigit(board[i][j]))
                    result.Add(new Position(i, j));
            }
        }

        return result;
    }

    private (int row, int col) GetSubBoxMiddleByCoords(int row, int col)
    {
        int Calc(int index) => index / 3 * 3 + 1;

        return (Calc(row), Calc(col));
    }

    private HashSet<char> GetRowOptions(char[][] board, int row)
    {
        var result = new HashSet<char>(_validSet);

        for (int i = 0; i < board[row].Length; i++)
        {
            var val = board[row][i];
            if (result.Contains(val))
            {
                result.Remove(val);
            }
        }

        return result;
    }

    private HashSet<char> GetColOptions(char[][] board, int col)
    {
        var result = new HashSet<char>(_validSet);

        for (int i = 0; i < board.Length; i++)
        {
            var val = board[i][col];
            if (result.Contains(val))
            {
                result.Remove(val);
            }
        }

        return result;
    }

    private HashSet<char> GetBoxOptions(char[][] board, int row, int col)
    {
        var (centerRow, centerCol) = GetSubBoxMiddleByCoords(row, col);
        var result = new HashSet<char>(_validSet);

        for (int i = centerRow - 1; i <= centerRow + 1; i++)
        {
            for (int j = centerCol - 1; j <= centerCol + 1; j++)
            {
                var val = board[i][j];
                if (result.Contains(val))
                    result.Remove(val);
            }
        }

        return result;
    }

    #region checks

    private bool SubBoxIsValid(char[][] board, int centerRow, int centerCol)
    {
        var set = new HashSet<char>();
        for (int i = centerRow - 1; i <= centerRow + 1; i++)
        {
            for (int j = centerCol - 1; j <= centerCol + 1; j++)
            {
                char c = board[i][j];
                if (set.Contains(c))
                    return false;
                if (Char.IsDigit(c))
                    set.Add(c);
            }
        }

        return true;
    }

    private bool RowIsValid(char[][] board, int rowIndex)
    {
        var set = new HashSet<char>();
        for (int i = 0; i < board[rowIndex].Length; i++)
        {
            char c = board[rowIndex][i];
            if (set.Contains(c))
                return false;
            if (Char.IsDigit(c))
                set.Add(c);
        }

        return true;
    }

    private bool ColIsValid(char[][] board, int colIndex)
    {
        var set = new HashSet<char>();
        for (int i = 0; i < board.Length; i++)
        {
            char c = board[i][colIndex];
            if (set.Contains(c))
                return false;
            if (Char.IsDigit(c))
                set.Add(c);
        }

        return true;
    }

    #endregion
}