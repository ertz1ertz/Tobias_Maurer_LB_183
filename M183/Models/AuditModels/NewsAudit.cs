namespace M183.Models.AuditModels
{
  public class NewsAudit
  {
    public int Id { get; set; }
    public string Action { get; set; } 
    public string TableName { get; set; } = "News";
    public string ChangedData { get; set; } 
    public DateTime ActionDate { get; set; }
    public string Username { get; set; } 
  }
}
