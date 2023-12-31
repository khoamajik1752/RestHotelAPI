﻿
namespace Hotel.DataAccess.Entities;

public class ServiceCategory
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<HotelService> HotelServices { get; set; } = new List<HotelService>();
}
