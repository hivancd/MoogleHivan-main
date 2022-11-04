using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)//Este es el metodo PrincipalMAIN METHOD
    {

        //string content = @"E:\Prog\moogle\moogle-main\Content";
        string content = @"..\Content";
        string search_query = QueryProcessing.ProcessQuery(query);
        query = QueryProcessing.erase_whitespace(query);

        string[] files = Directory.GetFiles(content);
        var Dictionarys = MatricesWork.GetWordDictionarys(files);
        var AllTheWords = MatricesWork.GetAllTheWords(Dictionarys);
        var QueryVec = MatricesWork.Sentence2Vec(search_query, AllTheWords);
        var PreMatrix = MatricesWork.GetMatrix(files, Dictionarys, AllTheWords);
        var Matrix = MatricesWork.tf_idf(PreMatrix);
        Matrix = Cercania(Matrix, AllTheWords, files, query);
        Matrix = GetImportance(query, AllTheWords, Matrix);
        Matrix = NotAllowedandNecessaryOp(query, AllTheWords, Matrix);
        var SearchValues = MatricesWork.VecXMatrix(QueryVec, Matrix);
        SearchItem[] searchItems = GetSearchItems(SearchValues);
        string Suggestion = GetSuggestion(AllTheWords, search_query);
        SearchResult result = new SearchResult(searchItems, Suggestion);

        return result;


        SearchItem[] GetSearchItems(double[] SearchValues)
        {
            List<SearchItem> items = new List<SearchItem>();

            // SearchItem(string title, string snippet, float score)
            for (int i = 0; i < SearchValues.Length; i++)
            {
                string FileName = "";
                string Snippet = "";
                float Score = 0;

                if (SearchValues[i] != 0)
                {
                    FileName = Path.GetFileNameWithoutExtension(files[i]);
                    Snippet = GetSnip(files[i], search_query);
                    Score = (float)SearchValues[i];
                    SearchItem item = new(FileName, Snippet, Score);
                    items.Add(item);
                }
            }
            var itemArray = ResultsPreparations.SortSearchItems(items.ToArray());
            return itemArray;
        }

    }
    public static string GetSnip(string filePath, string query)//Este metodo coge el snip 
    {
        string[] query_array = query.Split();
        string text = File.ReadAllText(filePath);
        int middle = -1;
        int SnipLeng = 500;

        foreach (string word in query_array)
        {
            middle = text.IndexOf(word);
            if (middle >= 0)
            {
                if (text.Length <= SnipLeng)
                    return text;
                if (text.Length - middle <= SnipLeng)
                    return text.Substring(middle, text.Length - middle);

                if (middle < 250)
                    return text.Substring(0, SnipLeng);
                else
                    return text.Substring(middle - 250, SnipLeng);
            }
        }
        return " ";
    }

    static double[,] Cercania(double[,] Matrix, List<string> AllTheWords, string[] files, string query)// Metodo de el operador de cercania ~
    {
        string pattern = @"[ ~ ]||[~]||[ ~]||[~ ]";
        Regex obj = new Regex(pattern);
        string[] query_array = query.Split();
        List<int> TextIndexes = new List<int>();

        if (query_array.Length >= 3)
        {
            for (int i = 0; i < query_array.Length - 2; i++)
            {
                string LeftWord = query_array[i];
                string RightWord = query_array[i + 2];
                string[] NearWords = { LeftWord, RightWord };

                if (obj.IsMatch(query_array[i + 1]) && AllTheWords.Contains(LeftWord) && AllTheWords.Contains(RightWord))
                {
                    TextIndexes = GetTextIndexes(AllTheWords, Matrix, LeftWord, RightWord);
                    foreach (int index in TextIndexes)
                    {
                        string text = File.ReadAllText(files[index]);
                        double dist = GetShortestDistance(text, LeftWord, RightWord);
                        for (int y = 0; y < Matrix.GetLength(1); y++)
                        {
                            if (Matrix[index, y] != 0)
                            {
                                Matrix[index, y] = Matrix[index, y] * dist;
                            }
                        }
                    }

                }
            }
        }

        return Matrix;

    }

    static double GetShortestDistance(string text, string LeftWord, string RightWord)//Metodo auxiliar del operador de cercania
    {
        
        var LeftWordIndexes = GetIndexes(text, LeftWord);
        var RightWordIndexes = GetIndexes(text, RightWord);
        List<int> distances = new List<int>();

        if(LeftWordIndexes.Count == 0 || RightWordIndexes.Count == 0)
            return 1;
        foreach (int indexL in LeftWordIndexes)
        {
            foreach (int indexR in RightWordIndexes)
            {
                int distance = (Math.Abs(indexL - indexR));
                distances.Add(distance);
            }
        }

        double ans = (double)distances[0];
        if (distances.Count >= 1)
        {
            foreach (int dist in distances)
            {
                if (dist < ans)
                    ans = dist;
            }
        }
        return (double)1 + (double)1 / ans* 2;

    }
    static List<int> GetTextIndexes(List<string> AllTheWords, double[,] Matrix, string LeftWord, string RightWord)
    {
        List<int> TextIndexes = new List<int>();
        for (int y = 0; y < AllTheWords.Count; y++)
        {
            if (AllTheWords[y] == LeftWord || AllTheWords[y] == RightWord)
            {
                for (int x = 0; x < Matrix.GetLength(0); x++)
                {
                    if (Matrix[x, y] != 0)
                    {
                        TextIndexes.Add(x);
                    }
                }
            }
            if (AllTheWords[y] == LeftWord || AllTheWords[y] == RightWord)
            {
                for (int x = 0; x < Matrix.GetLength(0); x++)
                {
                    if (Matrix[x, y] == 0 && TextIndexes.Contains(x))
                    {
                        TextIndexes.Remove(x);
                    }
                }
            }
        }
        return TextIndexes;
    }

     static List<int> GetIndexes(string text, string word)
    {
        string[] text_array = text.Split();
        List<int> AllIndexes = new List<int>();

        for (int i = 0; i < text_array.Length; i++)
        {
            if(text_array[i] == word)
                AllIndexes.Add(i);
        }
        return AllIndexes;
    }

    static double[,] NotAllowedandNecessaryOp(string query, List<string> AllTheWords, double[,] Matrix)
    {
        string[] query_array = query.Split();
        List<string> NecessaryWords = new List<string>();
        List<string> NotAllowedWords = new List<string>();

        if (query == "")
            return Matrix;

        foreach (string word in query_array)
        {
            if (word[0].ToString() == "^")
                NecessaryWords.Add(word.Substring(1));
            if (word[0].ToString() == "!")
                NotAllowedWords.Add(word.Substring(1));
        }

        List<int> Necessaryindexes = new List<int>();
        List<int> NotAllowedindexes = new List<int>();

        foreach (string word in NecessaryWords)
        {
            if (AllTheWords.Contains(word))
            {
                Necessaryindexes.Add(AllTheWords.IndexOf(word));
            }
        }
        foreach (string word in NotAllowedWords)
        {
            if (AllTheWords.Contains(word))
            {
                NotAllowedindexes.Add(AllTheWords.IndexOf(word));
            }
        }

        foreach (int i in Necessaryindexes)
        {
            for (int x = 0; x < Matrix.GetLength(0); x++)
            {
                if (Matrix[x, i] == 0)
                {
                    for (int y = 0; y < Matrix.GetLength(1); y++)
                        Matrix[x, y] = 0;
                }
            }
        }
        foreach (int i in NotAllowedindexes)
        {
            for (int x = 0; x < Matrix.GetLength(0); x++)
            {
                if (Matrix[x, i] != 0)
                {
                    for (int y = 0; y < Matrix.GetLength(1); y++)
                        Matrix[x, y] = 0;
                }
            }
        }

        return Matrix;
    }


    static double[,] GetImportance(string query, List<string> AllTheWords, double[,] Matrix)
    {
        string pattern = @"^\*";
        Regex obj = new Regex(pattern);

        Dictionary<string, int> Importance = new Dictionary<string, int>();
        string[] query_array = query.Split();

        foreach (string word in query_array)
        {
            if (obj.IsMatch(word))
            {
                int asterisc = 0;
                int index = 0;
                while (word[index].ToString() == "*")
                {
                    index += 1;
                    asterisc += 1;
                }
                Importance[word.Substring(index)] = asterisc;
            }
        }

        foreach (string word in Importance.Keys)
        {
            int index = 0;
            int raise = Importance[word];
            if (AllTheWords.Contains(word))
            {
                index = AllTheWords.IndexOf(word);
                for (int x = 0; x < Matrix.GetLength(0); x++)
                {
                    Matrix[x, index] = (double)raise * Matrix[x, index];
                }
            }
        }
        return Matrix;
    }
    public static string GetSuggestion(List<string> AllTheWords, string query)
    {
        string MoreSimilarSentence = "";
        string[] query_array = query.Split();

        foreach (string q in query_array)
        {
            int max = 0;
            string MoreSimilarWord = query;
            foreach (string word in AllTheWords)
            {
                if (IsSimilar(q, word) > max)
                {
                    max = IsSimilar(q, word);
                    MoreSimilarWord = word;
                }
            }
            MoreSimilarSentence += " " + MoreSimilarWord;
        }
        return MoreSimilarSentence;
    }

    public static int IsSimilar(string query, string word)
    {
        int ans = 0;
        List<char> QueryList = query.ToList<char>();
        List<char> WordList = word.ToList<char>();
        int i = 0;

        while (i < QueryList.Count & i < WordList.Count)
        {
            if (WordList[i] == QueryList[i])
                ans += 1;
            else
                ans -= 1;
            i++;
        }
        return ans;
    }


}