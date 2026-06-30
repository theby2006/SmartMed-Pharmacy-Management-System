namespace SmartMed.Models.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        PrescriptionReviewRequired = 2,
        Approved = 3,
        Processing = 4,
        Completed = 5,
        Cancelled = 6,
        Rejected = 7
    }
}
