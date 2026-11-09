using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NovaControlCenter.Models;

public record NavItem(string Tag, string Label, string Glyph);

public class ServerRow : INotifyPropertyChanged
{
    private string _status = "Online";
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Region { get; set; } = "";
    public double Cpu { get; set; }
    public double Memory { get; set; }
    public int UptimeDays { get; set; }
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public static class SampleData
{
    public static ObservableCollection<ServerRow> Servers(int count = 60)
    {
        var rnd = new Random(42); // deterministic demo data
        var regions = new[] { "EU-West", "US-East", "APAC", "EU-Central", "US-West" };
        var names = new[] { "nova-", "aurora-", "pulse-", "orbit-", "vertex-", "halo-" };
        var list = new ObservableCollection<ServerRow>();
        for (int i = 1; i <= count; i++)
            list.Add(new ServerRow
            {
                Id = i,
                Name = $"{names[i % names.Length]}{1000 + i}",
                Region = regions[rnd.Next(regions.Length)],
                Cpu = Math.Round(rnd.NextDouble() * 96 + 2, 1),
                Memory = Math.Round(rnd.NextDouble() * 88 + 5, 1),
                UptimeDays = rnd.Next(1, 420),
                Status = rnd.NextDouble() > 0.12 ? "Online" : (rnd.NextDouble() > 0.5 ? "Degraded" : "Offline")
            });
        return list;
    }
}
