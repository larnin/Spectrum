namespace Spectrum.API.Interfaces.Systems
{
    public interface IExecutor
    {
        void Execute(string name);
        void ExecuteAll();
    }
}
