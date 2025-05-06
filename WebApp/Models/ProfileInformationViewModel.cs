using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class ProfileInformationViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [DataType(DataType.PhoneNumber)]

        public string? PhoneNumber { get; set; }
        public string? StreetName { get; set; }
        [DataType(DataType.PostalCode)]
        public string? PostalCode { get; set; }
        public string? City { get; set; }
    }
}
