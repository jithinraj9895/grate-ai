public class SearchResultDto
{
    public Guid ProductId { get; set; }
    public string Text { get; set; } = string.Empty;
    public double Similarity { get; set; }
}

public class SearchResponseDto
{
    public List<SearchResultDto> Results { get; set; } = [];
}