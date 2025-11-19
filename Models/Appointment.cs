namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class Appointment
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        public int ID { get; set; }
        public int CarID { get; set; }
        public int StatusID { get; set; }
        public int? ServiceID { get; set; }
        public DateTime ScheduleAppointment { get; set; }

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public Car Car { get; set; }
        public Status Status { get; set; }
        public Service Service { get; set; }
        public Receipt Receipt { get; set; }
    }
}
