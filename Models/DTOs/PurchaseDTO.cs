namespace Models.DTOs
{
    public class PurchaseDTO
    {
        public int Id { get; set; }

        public int DigitalResourceId { get; set; }

        public string ResourceTitle { get; set; }  = string.Empty; // Extra info from DigitalResource
        
        public decimal? Price { get; set; }

        public DateTime PurchaseDate { get; set; }
    }
}
