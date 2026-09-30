using MasCleaners.Models;

namespace MasCleaners.ViewModels
{
    public class ServiceOptionsVM
    {
        public ServiceCategory? ServiceCategory { get; set; }

        public IEnumerable<ServiceOption> ServiceOptions { get; set; }
            = new List<ServiceOption>();

        public int CartItemCount { get; set; }

        public decimal CartTotal { get; set; }
    }
}
