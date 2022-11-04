using System.IO;
using System.Text.RegularExpressions;
using System;

namespace MoogleEngine
{

    public class MatricesWork
    {
        public static Dictionary<string, int>[] GetWordDictionarys(string[] files)
        {
            int FilesLength = files.Length;
            Dictionary<string, int>[] Dictionarys = new Dictionary<string, int>[FilesLength];

            for (int i = 0; i < FilesLength; i++)
            {
                string[] CurrentFile = File.ReadAllText(files[i]).Split();
                Dictionary<string, int> CurrentDict = new Dictionary<string, int>();

                foreach (string word in CurrentFile)
                {
                    if (CurrentDict.Keys.Contains(word))
                        CurrentDict[word] += 1;
                    else
                        CurrentDict[word] = 1;
                }
                Dictionarys[i] = CurrentDict;
            }
            return Dictionarys;
        }

        public static int[,] GetMatrix(string[] files, Dictionary<string, int>[] Dictionarys, List<string> AllTheWords)
        {
            int[,] Matrix = new int[files.Length, AllTheWords.Count];

            for (int x = 0; x < files.Length; x++)
            {
                foreach (string s in Dictionarys[x].Keys)
                {
                    int y = AllTheWords.IndexOf(s);
                    Matrix[x, y] = Dictionarys[x][s];
                }
            }
            return Matrix;
        }


        public static int[] Sentence2Vec(string sentence, List<string> AllTheWords)
        {
            string[] sentenceArray = sentence.Split();
            int[] vec = new int[AllTheWords.Count];

            for (int i = 0; i < sentenceArray.Length; i++)
            {
                if (AllTheWords.Contains(sentenceArray[i]))
                    vec[AllTheWords.IndexOf(sentenceArray[i])] = 1;
            }

            return vec;
        }


        public static List<string> GetAllTheWords(Dictionary<string, int>[] Dictionarys)
        {
            List<string> AllTheWords = new List<string>();
            for (int i = 0; i < Dictionarys.Length; i++)
            {
                foreach (string s in Dictionarys[i].Keys)
                {
                    if (!AllTheWords.Contains(s))
                        AllTheWords.Add(s);
                }
            }
            return AllTheWords;
        }



        // MAIN: Gets a tfidf Matrix out of the WordsXFiles Matrix
        public static double[,] tf_idf(int[,] Matrix)
        {
            double[,] tf_idf = new double[Matrix.GetLength(0), Matrix.GetLength(1)];

            var TotalWords = GetTotalWords(Matrix);
            var NumberOfDocsEachWordAppears = GetNumberOfDocsEachWordAppears(Matrix);

            for (int x = 0; x < tf_idf.GetLength(0); x++)
            {
                for (int y = 0; y < tf_idf.GetLength(1); y++)
                {
                    tf_idf[x, y] = (double)Matrix[x, y] / (double)TotalWords[x] * (double)Math.Log((double)Matrix.GetLength(0) / (double)NumberOfDocsEachWordAppears[y]);
                }
            }
            return tf_idf;

            int[] GetTotalWords(int[,] Matrix)
            {
                int[] TotalWords = new int[Matrix.GetLength(0)];

                for (int x = 0; x < Matrix.GetLength(0); x++)
                {
                    int sum = 0;
                    for (int y = 0; y < Matrix.GetLength(1); y++)
                    {
                        sum += Matrix[x, y];
                    }
                    TotalWords[x] = sum;
                }
                return TotalWords;
            }

            int[] GetNumberOfDocsEachWordAppears(int[,] Matrix)
            {
                int[] NumberOfDocsEachWordAppears = new int[Matrix.GetLength(1)];

                for (int y = 0; y < Matrix.GetLength(1); y++)
                {
                    for (int x = 0; x < Matrix.GetLength(0); x++)
                    {
                        if (Matrix[x, y] != 0)
                            NumberOfDocsEachWordAppears[y] += 1;
                    }
                }
                return NumberOfDocsEachWordAppears;
            }
        }

        public static double[] VecXMatrix(int[] Vec, double[,] Matrix)
        {
            var ans = new double[Matrix.GetLength(0)];

            for (int x = 0; x < ans.Length; x++)
            {
                for (int y = 0; y < Matrix.GetLength(1); y++)
                {
                    if (Matrix[x, y] != 0 & Vec[y] != 0)
                    {
                        ans[x] += Matrix[x, y] * (double)Vec[y];
                    }
                }
            }
            return ans;
        }

        // AUXILIARI

        public static void PrintMatrix(int[,] Matrix)
        {
            for (int x = 0; x < Matrix.GetLength(0); x++)
            {
                for (int y = 0; y < Matrix.GetLength(1); y++)
                    System.Console.Write(Matrix[x, y] + "  ");
                System.Console.WriteLine();
            }
        }
    }
}