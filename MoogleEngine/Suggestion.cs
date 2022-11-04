namespace MoogleEngine
{
    class Suggestion
    {
        public static string GetSuggestion(List<string> AllTheWords, string query)
        {
            int max = 0;
            string MoreSimilarWord = query;
            string[] query_array = query.Split();

            foreach (string word in AllTheWords)
            {
                foreach (string q in query_array)
                {
                    if (IsSimilar(q, word) > max)
                    {
                        max = IsSimilar(q, word);
                        MoreSimilarWord = word;
                    }
                }
            }
            return MoreSimilarWord;
        }

        public static int IsSimilar(string query, string word)
        {
            int ans = 0;
            List<char> QueryList = query.ToList<char>();
            List<char> WordList = word.ToList<char>();

            foreach (char c in QueryList)
            {
                if (WordList.Contains(c))
                {
                    WordList.Remove(c);
                    ans += 1;
                }
                else
                    ans -= 1;

            }
            return ans;
        }
    }
}