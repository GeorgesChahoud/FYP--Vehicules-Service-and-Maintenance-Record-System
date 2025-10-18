namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class Receipt
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        public int ID { get; set; }
        public int AppointmentID { get; set; }
        public DateTime DateANDTime { get; set; }
        public double Total { get; set; }

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public Appointment Appointment { get; set; }

        public ICollection<ReceiptService> ReceiptServices = new List<ReceiptService>();
    }
}
