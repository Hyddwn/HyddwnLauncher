namespace HyddwnLauncher.Extensibility.Interfaces
{
    public interface IClientProfile
    {
        string Location { get; set; }
        string Name { get; set; }
        string Guid { get; }
    }
}