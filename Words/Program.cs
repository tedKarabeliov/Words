using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Server;

namespace Words
{
    using System;
    using System.Collections.Generic;

    class Program
    {
        private static int foundWords = 0;
        private static ICollection<string> results = new List<string>();
        private static ICollection<string> resultPaths = new List<string>();
        private static StringBuilder builder = new StringBuilder();
        private static StringBuilder builderPath = new StringBuilder();
        private static int maxWordLength = 10;

        static void Main(string[] args)
        {
            var words = InitializeWordsMatrix();
            var visitedWithLastRow = new int[4, 4];
            var visitedWithLastCol = new int[4, 4];
            var visited = new bool[4, 4];

            for (int i = 0; i < words.GetLength(0); i++)
            {
                for (int k = 0; k < words.GetLength(1); k++)
                {
                    GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, i, k, -1, -1);
                }
            }

            var stringBuilder = new StringBuilder();

            var enum1 = results.GetEnumerator();
            var enum2 = resultPaths.GetEnumerator();
            var index = 0;
            while (enum1.MoveNext() && enum2.MoveNext())
            {
                stringBuilder.Append($"insert into wordInputs (Name, [Path]) values (N'{enum1.Current}', N'{enum2.Current}') ");
                index++;
                if (index % 1000 == 0)
                {
                    using (var connection = new SqlConnection(@"data source=.\SQLEXPRESS;initial catalog=Words; Integrated security = true; Timeout = 0"))
                    {
                        SqlCommand cmd = new SqlCommand(stringBuilder.ToString(), connection);
                        cmd.CommandTimeout = 0;
                        connection.Open();
                        cmd.ExecuteReader();
                    }

                    stringBuilder.Clear();
                }
            }

            using (var connection = new SqlConnection(@"data source=.\SQLEXPRESS;initial catalog=Words; Integrated security = true; Timeout = 0"))
            {
                SqlCommand cmd = new SqlCommand(stringBuilder.ToString(), connection);
                cmd.CommandTimeout = 0;
                connection.Open();
                cmd.ExecuteReader();
            }

            using (var connection = new SqlConnection(@"data source=.\SQLEXPRESS;initial catalog=Words; Integrated security = true"))
            {
                SqlCommand cmd = new SqlCommand("select Name, Path from V_WordResults v ORDER BY LEN(v.Name)", connection);
                connection.Open();

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    using (StreamWriter writetext = new StreamWriter("clickerInput.txt"))
                    {
                        while (rdr.Read())
                        {
                            //Console.OutputEncoding = Encoding.UTF8;
                            //Console.WriteLine(rdr.GetString(0));
                            //Console.WriteLine(rdr.GetString(1));
                            writetext.WriteLine(rdr.GetString(1));
                        }
                    }
                    
                }
            }

            using (var connection = new SqlConnection(@"data source=.\SQLEXPRESS;initial catalog=Words; Integrated security = true"))
            {
                SqlCommand cmd = new SqlCommand("delete from wordinputs", connection);
                connection.Open();

                cmd.ExecuteNonQuery();
            }

            //Console.WriteLine(foundWords);
        }

        private static char[,] InitializeWordsMatrix()
        {
            //var inputLine1 = Console.ReadLine().Split(new char[] {','});
            //var inputLine2 = Console.ReadLine().Split(new char[] {','});
            //var inputLine3 = Console.ReadLine().Split(new char[] {','});
            //var inputLine4 = Console.ReadLine().Split(new char[] {','});

            //var words = new char[4, 4]
            //{
            //    {char.Parse(inputLine1[0]), char.Parse(inputLine1[1]), char.Parse(inputLine1[2]), char.Parse(inputLine1[3])},
            //    {char.Parse(inputLine2[0]), char.Parse(inputLine2[1]), char.Parse(inputLine2[2]), char.Parse(inputLine2[3])},
            //    {char.Parse(inputLine3[0]), char.Parse(inputLine3[1]), char.Parse(inputLine3[2]), char.Parse(inputLine3[3])},
            //    {char.Parse(inputLine4[0]), char.Parse(inputLine4[1]), char.Parse(inputLine4[2]), char.Parse(inputLine4[3])},
            //};

            var input = "дбрблтипнемццмаа";
            maxWordLength = 5;

            var words = new char[4, 4]
            {
                {input[0], input[1], input[2], input[3]},
                {input[4], input[5], input[6], input[7] },
                {input[8], input[9], input[10], input[11] },
                {input[12], input[13], input[14], input[15] }
            };

            return words;
        }

        private static void GenerateWord(char[,] words, bool[,] visited, int[,] visitedWithLastRow, int[,] visitedWithLastCol, int row, int col, int lastRow, int lastCol)
        {
            if (row > 3 || row < 0 || col > 3 || col < 0
                || visited[row, col])
            {
                return;
            }

            visited[row, col] = true;
            visitedWithLastRow[row, col] = lastRow;
            visitedWithLastCol[row, col] = lastCol;

            // Right
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row, col + 1, row, col);

            // Down
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row + 1, col, row, col);

            // Left
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row, col - 1, row, col);

            // Up
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row - 1, col, row, col);

            // Up + Left
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row - 1, col - 1, row, col);

            // Up + Right
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row - 1, col + 1, row, col);

            // Down + Left
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row + 1, col - 1, row, col);

            // Down + Right
            GenerateWord(words, visited, visitedWithLastRow, visitedWithLastCol, row + 1, col + 1, row, col);

            visited[row, col] = false;
            PrintResult(words, visitedWithLastRow, visitedWithLastCol, row, col);
            foundWords++;
        }

        private static void PrintResult(char[,] words, int[,] visitedWithLastRow, int[,] visitedWithLastCol, int row, int col)
        {
            if (row == -1 || col == -1)
            {
                if (builder.Length >= 3 && builder.Length <= maxWordLength)
                {
                    results.Add(builder.ToString());
                    resultPaths.Add(builderPath.ToString().TrimEnd());
                }

                builder.Clear();
                builderPath.Clear();
                return;
            }

            builder.Append(words[row, col]);
            builderPath.Append(ConvertToCoordinates(row * 4 + col));
            builderPath.Append(" ");

            PrintResult(words, visitedWithLastRow, visitedWithLastCol, visitedWithLastRow[row, col], visitedWithLastCol[row, col]);
        }

        private static string ConvertToCoordinates(int position)
        {
            var result = string.Empty;

            switch (position)
            {
                case 0: result = "828 406"; break;
                case 1: result = "977 406"; break;
                case 2: result = "1124 406"; break;
                case 3: result = "1273 406"; break;
                case 4: result = "828 554"; break;
                case 5: result = "977 554"; break;
                case 6: result = "1124 554"; break;
                case 7: result = "1273 406"; break;
                case 8: result = "828 697"; break;
                case 9: result = "988 697"; break;
                case 10: result = "1124 697"; break;
                case 11: result = "1273 697"; break;
                case 12: result = "828 848"; break;
                case 13: result = "988 848"; break;
                case 14: result = "1124 848"; break;
                case 15: result = "1273 848"; break;

                default: break;
            }

            return result;
        }
    }
}
