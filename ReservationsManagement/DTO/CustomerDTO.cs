using Microsoft.AspNetCore.Mvc.Rendering;
using ReservationsManagement.Models;

namespace ReservationsManagement.DTO
{
    public class CustomerDTO
    {
        public Customer Customer { get; set; }
        public List<SelectListItem> GenderList { get; set; }
    }
}
