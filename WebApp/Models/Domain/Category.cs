﻿namespace WebApp.Models.Domain;

public class Category
{
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

}