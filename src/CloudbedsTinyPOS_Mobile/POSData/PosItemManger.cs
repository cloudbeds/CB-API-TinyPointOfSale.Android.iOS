using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
/// <summary>
/// Manages the items (e.g. menu items) in our Point of Sale
/// </summary>
class PosItemManager
{

    List<PosItem> _posItems = new List<PosItem>();

    Dictionary<string, List<PosItem>> _categories = new Dictionary<string, List<PosItem>>();

    public ICollection<PosItem> Items
    {
        get
        {
            return _posItems.AsReadOnly();
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public PosItemManager()
    {
        AddPosItem(new PosItem((decimal) 10.20, (decimal) 0.90, "", "iws-408076e2-5317-11ed-bdc3-0242ac120002", "Pancakes", "Food"));
        AddPosItem(new PosItem((decimal)12.00, (decimal)1.30, ""  , "iws-408076e2-5317-11ed-bdc3-0242ac120003", "French Toast", "Food"));
        AddPosItem(new PosItem((decimal) 9.20, (decimal)0.72, ""  , "iws-720e40cc-5317-11ed-bdc3-0242ac120004", "Bacon", "Food"));
        AddPosItem(new PosItem((decimal)8.00, (decimal)1.20,  ""  , "iws-720e40cc-5317-11ed-bdc3-0242ac120004", "Eggs (2)", "Food"));
        AddPosItem(new PosItem((decimal) 4.60, (decimal)0.52, ""  , "iws-8a105e08-5317-11ed-bdc3-0242ac120005", "Orange Juice", "Beverage (non-alcoholic)"));
        AddPosItem(new PosItem((decimal)7.00, (decimal)0.52, ""   , "iws-8a105e08-5317-11ed-bdc3-0242ac120006", "Mimosa", "Beverage (alcoholic)"));
        AddPosItem(new PosItem((decimal)7.00, (decimal)0.52, ""   , "iws-8a105e08-5317-11ed-bdc3-0242ac120007", "Beer (Craft)", "Beverage (alcoholic)"));
    }

    /// <summary>
    /// Get the set of categories
    /// </summary>
    /// <returns></returns>
    public ICollection<string> GetCategories()
    {
        return _categories.Keys;
    }


    /// <summary>
    /// The count
    /// </summary>
    public int ItemCount
    {
        get
        {
            var items = _posItems;
            if (items == null) return 0;
            return items.Count;
        }
    }
    /// <summary>
    /// Returns NULL of the category does not exist
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public ICollection<PosItem> GetPosItemsInCategory(string category)
    {
        List<PosItem> items;
        _categories.TryGetValue(category, out items);
        return items;
    }

    public void AddPosItem(PosItem item)
    {
        string category = item.Item_CategoryName;

        EnsureCategoryExists(category);

        //Add it to the categories and the general list
        _categories[category].Add(item);
        _posItems.Add(item);
    }

    /// <summary>
    /// Add the coategory if it does not exist
    /// </summary>
    /// <param name="category"></param>
    public void EnsureCategoryExists(string category)
    {
        if (_categories.ContainsKey(category)) return;
        _categories.Add(category, new List<PosItem>());
    }

}
