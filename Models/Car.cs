namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class Car
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string PlateNumber { get; set; }
        public string? Vin { get; set; }

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();


    }
}
