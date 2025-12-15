using WorkshopManager.Model.DataModels;

namespace WorkshopManager.Web.Helpers
{
    public static class StatusHelper
    {
        public static string GetStatusDisplayName(RepairOrderStatusValue status)
        {
            return status switch
            {
                RepairOrderStatusValue.Created => "Utworzone",
                RepairOrderStatusValue.PendingApproval => "Do akceptacji",
                RepairOrderStatusValue.Approved => "Wycena zaakceptowana",
                RepairOrderStatusValue.InProgress => "W realizacji",
                RepairOrderStatusValue.ReadyForPickup => "Gotowy do odbioru",
                RepairOrderStatusValue.Completed => "Zakończone",
                RepairOrderStatusValue.Cancelled => "Anulowane",
                _ => "Nieznany status"
            };
        }

        public static string GetStatusBadgeClass(RepairOrderStatusValue status)
        {
            return status switch
            {
                RepairOrderStatusValue.Created => "badge rounded-pill bg-secondary",
                RepairOrderStatusValue.PendingApproval => "badge rounded-pill bg-warning text-dark",
                RepairOrderStatusValue.Approved => "badge rounded-pill bg-info text-dark",
                RepairOrderStatusValue.InProgress => "badge rounded-pill bg-primary",
                RepairOrderStatusValue.ReadyForPickup => "badge rounded-pill bg-success",
                RepairOrderStatusValue.Completed => "badge rounded-pill bg-dark",
                RepairOrderStatusValue.Cancelled => "badge rounded-pill bg-danger",
                _ => "badge rounded-pill bg-secondary"
            };
        }

        public static string GetStatusBadgeHtml(RepairOrderStatusValue status)
        {
            var cssClass = GetStatusBadgeClass(status);
            var displayName = GetStatusDisplayName(status);
            return $"<span class=\"{cssClass}\">{displayName}</span>";
        }
    }
}
