namespace Az_PostAL_23.Services
{
    public interface IContainerService
    {
        Task<List<string>> GetAllContainer();
        Task CreateContainer(string containerName, string containerType);
        Task DeleteContainer(string containerName);
    }
}
