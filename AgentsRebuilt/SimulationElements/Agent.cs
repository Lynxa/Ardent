using System;
using System.Collections.Generic;
using WindowsFormsApplication1;

public class Agent
{
    public String ID;
    public List<Item> Items;
    public bool IsNew = true;
    public Agent(String id, List<KVP> items)
    {
        ID = id;
        Items = Item.KvpToItems(items);
    }
}
