namespace DATN.Entities
{
    public class InventoryLog : Time
    {
        public int LogId { get; set; }
        public int? ProductId { get; set; }
        public int ChangeQuantity { get; set; }
        public int price { get; set; }
        public string LogType { get; set; }
        public string Note { get; set; }
        public DateTime LogDate { get; set; }
        public Product Product { get; set; }
    }
}
