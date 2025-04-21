using Fermion.EntityFramework.Core.Enums;

namespace Fermion.EntityFramework.Core.Models;

public class Sort
{
    public string Field { get; set; }
    public SortTypes SortType { get; set; }

    public Sort()
    {
        Field = string.Empty;
        SortType = SortTypes.Asc;
    }

    public Sort(string field, SortTypes sortType)
    {
        Field = field;
        SortType = sortType;
    }
}