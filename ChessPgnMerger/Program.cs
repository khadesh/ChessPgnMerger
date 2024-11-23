using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class ChessPGNProcessor
{
    public static void ProcessPGNsByECO(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine("Directory does not exist.");
            return;
        }

        // Create the output directory
        string outputDirectory = Path.Combine(directoryPath, "output");
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // Dictionary to group PGNs by ECO code
        var ecoGroups = new Dictionary<string, List<string>>();

        // Regex patterns for cleaning the PGN
        var evalDataRegex = new Regex(@"\{\%.*?\}"); // Matches {%*} data
        var moveNumbersRegex = new Regex(@"\d+\.\.\."); // Matches move numbers with ...
        var periodSpaceRegex = new Regex(@"\.(?!\s)"); // Matches periods not followed by a space

        // Read all files in the directory
        foreach (var file in Directory.GetFiles(directoryPath))
        {
            if (!file.EndsWith(".pgn", StringComparison.OrdinalIgnoreCase))
                continue;

            string[] lines = File.ReadAllLines(file);
            string ecoCode = null;
            var gameMoves = new List<string>();

            // Extract ECO code and process moves
            foreach (var line in lines)
            {
                if (line.StartsWith("[ECO"))
                {
                    // Extract ECO code from the line
                    ecoCode = line.Split('"')[1];
                }
                else if (!line.StartsWith("[") && !string.IsNullOrWhiteSpace(line))
                {
                    // Clean the moves
                    string cleanedLine = evalDataRegex.Replace(line, ""); // Remove {%*} data
                    cleanedLine = moveNumbersRegex.Replace(cleanedLine, ""); // Remove move numbers with ...
                    cleanedLine = periodSpaceRegex.Replace(cleanedLine, ". "); // Ensure periods are followed by a space
                    cleanedLine = Regex.Replace(cleanedLine, @"\s+", " ").Trim(); // Normalize spaces
                    gameMoves.Add(cleanedLine);
                }
            }

            if (ecoCode == null || gameMoves.Count == 0)
                continue;

            // Add cleaned moves to the appropriate ECO group
            if (!ecoGroups.ContainsKey(ecoCode))
            {
                ecoGroups[ecoCode] = new List<string>();
            }
            ecoGroups[ecoCode].Add(string.Join(" ", gameMoves) + "\r\n");
        }

        // Write each ECO group to its own file
        foreach (var eco in ecoGroups)
        {
            string outputFilePath = Path.Combine(outputDirectory, $"{eco.Key}.pgn");
            File.WriteAllLines(outputFilePath, eco.Value);
            Console.WriteLine($"Written {eco.Value.Count} games to {outputFilePath}");
        }

        Console.WriteLine($"Processing complete. Output files are located in: {outputDirectory}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"C:\Users\pigsc\Downloads";
        ChessPGNProcessor.ProcessPGNsByECO(directoryPath);
    }
}
