using System.ComponentModel.DataAnnotations;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class User
    {
        /////////////////////////////////////////////////////////////////////// ATTRIBUTES ///////////////////////////////////////////////////////////////////////
        [Key]
        public int ID { get; set; }
        public int RoleID { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }

        /////////////////////////////////////////////////////////////////////// RELATIONSHIPS ///////////////////////////////////////////////////////////////////////
        public Customer Customer { get; set; }
        public Admin Admin { get; set; }
        public Employee Employee { get; set; }
        public Role Role { get; set; }
        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
