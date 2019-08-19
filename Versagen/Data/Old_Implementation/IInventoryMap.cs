namespace Versagen.Data
{
    public class InventoryMap
    {
        public string DbName { get; set; }
        public ulong EntityID { get; set; }
        public bool IDIsPlayer { get; set; }
        public int Quantity { get; set; }
        public string CustomXML { get; set; }
    }
}
