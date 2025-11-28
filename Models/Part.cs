namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class Part
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        public int ID { get; set; }
        public string PartName { get; set; }
        public double Price { get; set; }
        public bool Stock { get; set; }
        public int Quantity { get; set; }

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public ICollection<PartService> PartServices { get; set; } = new List<PartService>();
        public ICollection<ReceiptPart> ReceiptParts { get; set; } = new List<ReceiptPart>();
    }
}
