using System.Linq;
using Mono.Cecil;

namespace Spectrum.Prism
{
    class DispersionHelper
    {
        public static MethodDefinition FindInitializationMethodDefinition(ModuleDefinition targetModule)
        {
            var targetType = targetModule.GetType(Resources.GameManagerTypeName);
            return targetType.Methods.Single(m => m.Name == Resources.GameManagerInitMethodName);
        }

        public static MethodReference ImportBootstrapMethodReference(ModuleDefinition targetModule, ModuleDefinition bootstrapModule)
        {
            var bootstrapType = bootstrapModule.GetType(Resources.SpectrumLoaderTypeName);
            var bootstrapInitMethod = bootstrapType.Methods.Single(m => m.Name == Resources.SpectrumInitMethodName);

            return targetModule.Import(bootstrapInitMethod);
        }

        public static MethodDefinition FindUpdateMethodDefinition(ModuleDefinition targetModule)
        {
            var targetType = targetModule.GetType(Resources.GameManagerTypeName);
            return targetType.Methods.Single(m => m.Name == Resources.GameManagerUpdateMethodName);
        }

        public static MethodReference ImportUpdateMethodReference(ModuleDefinition targetModule, ModuleDefinition bootstrapModule)
        {
            var bootstrapUpdateType = bootstrapModule.GetType(Resources.SpectrumUpdaterTypeName);
            var bootstrapUpdateMethod = bootstrapUpdateType.Methods.Single(m => m.Name == Resources.SpectrumUpdateMethodName);

            return targetModule.Import(bootstrapUpdateMethod);
        }
    }
}
