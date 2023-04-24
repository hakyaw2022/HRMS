using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using HRMS.Models;

namespace HRMS.ViewModels
{
    public class ServiceViewModel
    {
        // common
        public int? Quantity { get; set; }
        public string? DisplayText => $"{Name} - {Price.ToString("C0", new CultureInfo("my-MM"))} / {PricingType}";
        // services related
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public PricingType PricingType { get; set; }
        public ServiceStatus Status { get; set; }
    }

    public class RestaurantItemViewModel
    {

        // common
        public int? Quantity { get; set; }
        public string? DisplayText => $"{Category} - " +
                                      $"{ItemCode} ({Name}) - " +
                                      $"{UnitPrice.ToString("C0", new CultureInfo("my-MM"))}";

        // restaurant items related
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ItemCode { get; set; }
        public int UnitPrice { get; set; }
        public string? Category { get; set; }
    }

    public class AllServicesViewModel
    {
        public List<ServiceViewModel>? Services { get; set; }
        public List<RestaurantItemViewModel>? RestaurantItems { get; set; }

        public List<Room>? Rooms { get; set; }

    }
}
