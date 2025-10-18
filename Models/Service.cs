namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class Service
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        public int ID { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public ICollection<ReceiptService> ReceiptServices = new List<ReceiptService>();
        public ICollection<PartService> PartServices = new List<PartService>();

    }
}
