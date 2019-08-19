namespace Versagen.Items
{
    public struct InventoryListing
    {
        public string ItemBaseName { get; }
        public string IsForQuest { get; }

        public int cost { get; }
        public string UniqueDescription { get; set; }
    }
}
