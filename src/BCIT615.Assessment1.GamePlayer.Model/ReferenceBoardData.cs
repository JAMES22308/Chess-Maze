using System.Collections.ObjectModel;
using BCIT615.Assessment1.GamePlayer;

namespace BCIT615.Assessment1.GamePlayer.Model;

/// <summary>Provides the known-good in-memory board used by the Assessment 1 reference workflow.</summary>
public static class ReferenceBoardData
{
    public const int Rows = 6;
    public const int Columns = 6;

    public static Position Start => new Position(5, 0);
    public static Position Target => new Position(0, 5);

    public static IReadOnlyDictionary<Position, PieceType> Pieces { get; } =
        new ReadOnlyDictionary<Position, PieceType>(
            new Dictionary<Position, PieceType>
            {
                [new Position(5, 0)] = PieceType.Rook,
                [new Position(5, 3)] = PieceType.Bishop,
                [new Position(3, 5)] = PieceType.Knight,
                [new Position(1, 4)] = PieceType.King
            });
}
