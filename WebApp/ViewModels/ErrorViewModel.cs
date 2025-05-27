using WebApp.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WebApp.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string? ErrorMessage { get; set; }
    public string? UserFriendlyMessage { get; set; }
    public string DisplayMessage => UserFriendlyMessage ?? ErrorMessage ?? "An error occurred";
}