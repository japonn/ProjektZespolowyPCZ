namespace WorkshopManager.Model.DataModels
{
    public class AdditionalCost
    {
        public int Id { get; set; }
        public int RepairOrderId { get; set; }
        public RepairOrder RepairOrder { get; set; } = null!;
        public decimal Cost { get; set; }
        public string Description { get; set; } = null!;
        public DateTime AddedDate { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? AcceptedDate { get; set; }
    }
}
