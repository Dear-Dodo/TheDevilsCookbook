using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nito.AsyncEx;
using TDC.Core.Type;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TDC.Core.Utility
{
    public static class SingletonInitialiser
    {
        public static readonly AsyncManualResetEvent FinishedIntialising = new AsyncManualResetEvent();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Initialise()
        {
            // System.Type[] types = Assembly.GetAssembly(typeof(Singleton<>)).GetTypes()
            //     .Where(t => !t.IsGenericType && t.BaseType?.IsGenericType == true 
            //                                  && t.BaseType.GetGenericTypeDefinition() == typeof(Singleton<>)).ToArray();


            // if (types.Length == 0) return;
            
            AsyncOperationHandle<IResourceLocator> addressablesLoad = Addressables.InitializeAsync();
            addressablesLoad.Completed += FinishInitialisation;
            // await addressablesLoad.Task;
            // if (!addressablesLoad.IsDone)
            // {
            //     Debug.LogError($"Addressables failed to initialise.");
            //     return;
            // }
            //
            // await types.Select(t => t.InvokeMember("InitialiseAuto",
            //     BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod,
            //     null, null, new object[] { addressablesLoad.Result }) as Task).ToArray().WhenAll();
            // FinishedIntialising.Set();
        }

        private static async void FinishInitialisation(AsyncOperationHandle<IResourceLocator> addressablesLoad)
        {
            System.Type[] types = Assembly.GetAssembly(typeof(Singleton<>)).GetTypes()
                .Where(t => !t.IsGenericType && t.BaseType?.IsGenericType == true 
                                             && t.BaseType.GetGenericTypeDefinition() == typeof(Singleton<>)).ToArray();
            
            await types.Select(t => t.InvokeMember("InitialiseAuto",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod,
                null, null, new object[] { addressablesLoad.Result }) as Task).ToArray().WhenAll();
            FinishedIntialising.Set();
        }
    }
}
