namespace EventsWebApp.Models.DTOs;

public class PagedEventsResponse
{
    public List<EventDto> Items { get; set; } = new List<EventDto>();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public bool HasItems => Items.Any();
    public bool IsEmpty => !Items.Any();
    public int ItemCount => Items.Count;
    
    public bool CanLoadMore => HasNext && CurrentPage < TotalPages;
}