namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class Employee
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Shift { get; set; }
        public string WorkHours { get; set; }
        public double FeeByService { get; set; }

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
