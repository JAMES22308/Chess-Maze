
using BCIT615.Assessment1.GamePlayer;
using AssessmentGamePlayer = BCIT615.Assessment1.GamePlayer.Model.GamePlayer;
using ChessMaze.Persistence;
using ChessMaze.Model;

using AssessmentPosition = BCIT615.Assessment1.GamePlayer.Position;
using AssessmentPieceType = BCIT615.Assessment1.GamePlayer.PieceType;
using AssessmentMoveRecord = BCIT615.Assessment1.GamePlayer.MoveRecord;

using SavedPosition = ChessMaze.Model.Position;
using SavedPieceType = ChessMaze.Model.PieceType;
using SavedMoveRecord = ChessMaze.Model.MoveRecord;

namespace Wpf;

public enum SavedGameLoadStatus
{
    Loaded,
    NotFound,
    InvalidData,
    Unavailable
}

public sealed record SavedGameLoadResult(
    SavedGameLoadStatus Status,
    AssessmentGamePlayer? Game
);

public sealed class Assessment1GamePersistenceAdapter(
    ISavedGameRepository repository)
{
    private const string BoardId = "Assessment1-Reference-v1";

    public async Task SaveAsync(
        string name,
        AssessmentGamePlayer game)
    {
        ValidateBoard(game);

        var history = game.MoveHistory.Select(move =>
            new SavedMoveRecord(
                move.SequenceNumber,
                new SavedPosition(move.From.Row, move.From.Column),
                new SavedPosition(move.To.Row, move.To.Column),
                ToSavedPiece(move.MovementPiece)
            )).ToList();

        var snapshot = new GameSnapshot(
            BoardId,
            new SavedPosition(
                game.CurrentPosition.Row,
                game.CurrentPosition.Column),
            history,
            game.IsComplete
        );

        await repository.SaveAsync(name, snapshot);
    }

    public async Task<SavedGameLoadResult> LoadAsync(string name)
    {
        GameSnapshot? snapshot;

        try
        {
            snapshot = await repository.LoadAsync(name);
        }
        catch (Exception ex) when (
            ex is Microsoft.Data.Sqlite.SqliteException
            or Microsoft.EntityFrameworkCore.DbUpdateException
            or InvalidOperationException)
        {
            return new(SavedGameLoadStatus.Unavailable, null);
        }

        if (snapshot is null)
            return new(SavedGameLoadStatus.NotFound, null);

        if (snapshot.BoardId != BoardId)
            return new(SavedGameLoadStatus.InvalidData, null);

        try
        {
            var restoredGame = new AssessmentGamePlayer();

            for (int i = 0; i < snapshot.History.Count; i++)
            {
                var savedMove = snapshot.History[i];

                if (savedMove.SequenceNumber != i + 1)
                    return new(SavedGameLoadStatus.InvalidData, null);

                var expectedFrom = new AssessmentPosition(
                    savedMove.From.Row,
                    savedMove.From.Column);

                var destination = new AssessmentPosition(
                    savedMove.To.Row,
                    savedMove.To.Column);

                if (restoredGame.CurrentPosition != expectedFrom)
                    return new(SavedGameLoadStatus.InvalidData, null);

                var currentPiece = restoredGame.GetPieceAt(
                    restoredGame.CurrentPosition);

                if (currentPiece is null ||
                    currentPiece.Value != ToAssessmentPiece(savedMove.MovementPiece))
                {
                    return new(SavedGameLoadStatus.InvalidData, null);
                }

                var result = restoredGame.TryMove(destination);

                if (result != BCIT615.Assessment1.GamePlayer.MoveResult.Success &&
                    result != BCIT615.Assessment1.GamePlayer.MoveResult.GameCompleted)
                {
                    return new(SavedGameLoadStatus.InvalidData, null);
                }
            }

            var savedPosition = new AssessmentPosition(
                snapshot.CurrentPosition.Row,
                snapshot.CurrentPosition.Column);

            if (restoredGame.CurrentPosition != savedPosition ||
                restoredGame.IsComplete != snapshot.IsComplete)
            {
                return new(SavedGameLoadStatus.InvalidData, null);
            }

            return new(SavedGameLoadStatus.Loaded, restoredGame);
        }
        catch (Exception ex) when (
            ex is ArgumentException or InvalidOperationException)
        {
            return new(SavedGameLoadStatus.InvalidData, null);
        }
    }
/*
    private static BCIT615.Assessment1.GamePlayer.MoveResult GetSuccess()
    {
        return BCIT615.Assessment1.GamePlayer.MoveResult.Success;
    }
*/
    private static void ValidateBoard(AssessmentGamePlayer game)
    {
        if (game.StartPosition != new AssessmentPosition(5, 0) ||
            game.TargetPosition != new AssessmentPosition(0, 5) ||
            game.GetPieceAt(new AssessmentPosition(5, 0)) != AssessmentPieceType.Rook ||
            game.GetPieceAt(new AssessmentPosition(5, 3)) != AssessmentPieceType.Bishop ||
            game.GetPieceAt(new AssessmentPosition(3, 5)) != AssessmentPieceType.Knight ||
            game.GetPieceAt(new AssessmentPosition(1, 4)) != AssessmentPieceType.King)
        {
            throw new InvalidOperationException(
                "This saved-game adapter only supports the Assessment 1 reference board.");
        }
    }

    private static SavedPieceType ToSavedPiece(
        AssessmentPieceType piece) =>
        piece switch
        {
            AssessmentPieceType.Rook => SavedPieceType.Rook,
            AssessmentPieceType.Bishop => SavedPieceType.Bishop,
            AssessmentPieceType.Knight => SavedPieceType.Knight,
            AssessmentPieceType.King => SavedPieceType.King,
            _ => throw new ArgumentOutOfRangeException(nameof(piece))
        };

    private static AssessmentPieceType ToAssessmentPiece(
        SavedPieceType piece) =>
        piece switch
        {
            SavedPieceType.Rook => AssessmentPieceType.Rook,
            SavedPieceType.Bishop => AssessmentPieceType.Bishop,
            SavedPieceType.Knight => AssessmentPieceType.Knight,
            SavedPieceType.King => AssessmentPieceType.King,
            _ => throw new ArgumentOutOfRangeException(nameof(piece))
        };
}
