namespace MoogleEngine;

public static class ResultsPreparations
{
    public static SearchItem[] DescendingSort(SearchItem[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            for (int j = i + 1; j < array.Length; j++)
            {
                if (array[j].Score > array[i].Score)
                {
                    SearchItem max = array[j];
                    array[j] = array[i];
                    array[i] = max;
                }
            }
        }
        return array;
    }

    static public SearchItem[] SortSearchItems(SearchItem[] array)
    {
        var items = DescendingSort(array);
        int ScoreZero = items.Length;

        for (int i = 0; i < items.Length; i++)
        {
            System.Console.WriteLine("");
            if (items[i].Score == 0)
            {
                ScoreZero = i;
                break;
            }
        }

        SearchItem[] ans = new SearchItem[ScoreZero];
        for (int i = 0; i < ans.Length; i++)
        {
            // items[i] = items[0];
            ans[i] = items[i];
        }
        return ans;
    }

}