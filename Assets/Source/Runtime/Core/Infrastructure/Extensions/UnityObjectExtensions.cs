namespace Core.Infrastructure.Extensions
{
    public static class UnityObjectExtensions
    {
        public static void SafeDestroy(this UnityEngine.Object obj)
        {
            if (obj == null) return;

            if (UnityEngine.Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
