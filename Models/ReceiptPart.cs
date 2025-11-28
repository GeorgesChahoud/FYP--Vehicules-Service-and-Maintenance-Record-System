namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class ReceiptPart
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        public int ID { get; set; }
        public int ReceiptID { get; set; }
        public int PartID { get; set; }
        public int QuantityUsed { get; set; }
        public double PriceAtTime { get; set; } // Store price at time of receipt to maintain history

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public Receipt Receipt { get; set; }
        public Part Part { get; set; }
    }
}
