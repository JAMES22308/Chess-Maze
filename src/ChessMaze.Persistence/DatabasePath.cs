
namespace ChessMaze.Persistence;

public static class DatabasePath
{
    public static string Resolve()
    {
        // Allow a custom database path when configured.
        var configured = Environment.GetEnvironmentVariable("CHESSMAZE_DB_PATH");

        if (!string.IsNullOrWhiteSpace(configured))
            return Path.GetFullPath(configured);

        // Find the root of our Chess-Maze project.
        var root = FindSolutionRoot(AppContext.BaseDirectory)
                   ?? Directory.GetCurrentDirectory();

        var dataFolder = Path.Combine(root, "data");
        Directory.CreateDirectory(dataFolder);

        return Path.Combine(dataFolder, "saved-games-test.db");
    }

    private static string? FindSolutionRoot(string start)
    {
        var directory = new DirectoryInfo(start);

        while (directory is not null)
        {
            if (directory.GetFiles("BCIT615.Assessment1.GamePlayer.sln").Length > 0)
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }
}
