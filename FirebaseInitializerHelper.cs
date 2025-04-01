using Firebase;
using System.Threading.Tasks;

public static class FirebaseInitializerHelper
{
    private static bool isInitialized = false;
    public static Task<DependencyStatus> InitializationTask { get; private set; }

    public static Task<DependencyStatus> InitializeIfNeeded()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            InitializationTask = FirebaseApp.CheckAndFixDependenciesAsync();
        }
        return InitializationTask;
    }
}
