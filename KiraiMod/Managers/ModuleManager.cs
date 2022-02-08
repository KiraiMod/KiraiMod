using System.Linq;

namespace KiraiMod.Managers
{
    public static class ModuleManager
    {
        // in the future this class may be responsible for UI stuff, a la old kiraimod
        static ModuleManager()
        {
            System.Reflection.Assembly.GetExecutingAssembly()
                .GetExportedTypes()
                .Where(x => x.Namespace == "KiraiMod.Modules")
                .ToList()
                .ForEach(x => x.Initialize());
        }
    }
}
