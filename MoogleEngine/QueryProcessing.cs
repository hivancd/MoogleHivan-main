using System.Text.RegularExpressions;

namespace MoogleEngine;

public class QueryProcessing
{
    public static string ProcessQuery(string query)
    {
        return erase_asterisc(erase_whitespace(erase_stopwords(query)));
    }
    public static string erase_stopwords(string sentence)//This method erases stopwords from the queryQUERY PROCESSING
    {
        string[] array_sentence = sentence.Split();
        string ans = "";

        for (int i = 0; i < array_sentence.Length; i++)
        {
            if (!Is_stop_word(array_sentence[i]))
                ans = ans + array_sentence[i] + " ";
        }
        return ans;
    }
    public static string erase_whitespace(string sentence)//this method erases whitespace from the queryQUERY PROCESSING
    {
        string ans = "";

        for (int i = 0; i < sentence.Length; i++)
        {
            if (!(sentence[i].ToString() == " " && ((i + 1 == sentence.Length) || (sentence[i + 1].ToString() == " "))))
                ans = ans + sentence[i];
        }
        return ans;
    }
    public static bool Is_stop_word(string word)//this method dtermines if a word is a stopwordQUERY PROCESSING
    {
        string stop_words_archive = @"..\stopwords.txt";
        string[] stopwords = File.ReadAllLines(stop_words_archive);

        if (stopwords.Contains(word.ToLower()))
            return true;
        return false;
    }

    public static string erase_asterisc(string sentence)
    {
        var ans = "";
        for(int i = 0; i<sentence.Length; i++)
        {
            if(!(sentence[i].ToString() == "*"))
                ans += sentence[i];
        }
        return ans;
    }
}
