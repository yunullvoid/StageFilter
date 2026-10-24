using System.Collections.Generic;

namespace StageFilter.Stage.Models;

public class Page<T>
{
    public int Number { get; }
    public List<T> Items { get; } = [];
    public Page(int number) => Number = number;
    public bool IsFull => Items.Count >= 8;
    public bool TryAdd(T item)
    {
        if (IsFull)
            return false;

        Items.Add(item);
        return true;
    }
}

public static class BanList
{
    public static readonly List<Page<StageInfo>> pages = [];

    public static void FillPages(StageInfo[] stages)
    {
        foreach (StageInfo stage in stages)
        {
            AddItem(pages, (int)stage.StageSet, stage);
        }
    }

    public static void AddItem<T>(List<Page<T>> pages, int pageNumber, T item)
    {
        Page<T> page = pages.Find(
            p => p.Number == pageNumber && !p.IsFull
        );

        if (page is null)
        {
            page = new Page<T>(pageNumber);
            pages.Add(page);
        }

        page.TryAdd(item);
    }

    public static void EmptyPages()
    {
        pages.Clear();
    }
}