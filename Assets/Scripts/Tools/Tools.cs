using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

namespace PalexUtilities
{
    public static class Tools
    {


        #region Text
            

            public static string Remove(this string s, string toRemove)
            {
                if (toRemove == "") return s;
                return s.Replace(toRemove, "");
            }


        #endregion



        #region Math
            

            public static bool IsOdd(this int i) => i % 2 == 1;
            public static bool IsEven(this int i) => i % 2 == 0;


        #endregion
        


        #region Lists / Arrays
            

            public static T[] RemoveDuplicates<T>(T[] arr)
            {
                List<T> list = new List<T>();
                foreach (T t in arr) {
                    if (!list.Contains(t)) {
                        list.Add(t);
                    }
                }
                return list.ToArray();
            }

            public static List<T> RemoveDuplicates<T>(List<T> arr)
            {
                List<T> list = new List<T>();
                foreach (T t in arr) {
                    if (!list.Contains(t)) {
                        list.Add(t);
                    }
                }
                return list;
            }


            public static void ShuffleArray<T>(T[] arr, int iterations)
            {
                for (int i = 0; i < iterations; i++) {
                    int rnd = UnityEngine.Random.Range(0, arr.Length);
                    T tmp = arr[rnd];
                    arr[rnd] = arr[0];
                    arr[0] = tmp;
                }
            }
            public static void ShuffleList<T>(List<T> list, int iterations)
            {
                for (int i = 0; i < iterations; i++) {
                    int rnd = UnityEngine.Random.Range(0, list.Count);
                    T tmp = list[rnd];
                    list[rnd] = list[0];
                    list[0] = tmp;
                }
            }


        #endregion


        #region Textures
            

            public static Texture2D CreateTexture2D(int width, int height, GraphicsFormat graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB, bool useMips = false)
            {
                return new Texture2D(width, height, graphicsFormat, useMips ? TextureCreationFlags.MipChain : TextureCreationFlags.None);
            }

            public static void ReadPixelsFrom(this Texture2D texture2D, RenderTexture renderTexture)
            {
                var prevActive = RenderTexture.active;

                RenderTexture.active = renderTexture;

                texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

                RenderTexture.active = prevActive;

            }

            public static Texture2D ToTexture2D(this RenderTexture rt)
            {
                var texture2D = CreateTexture2D(rt.width, rt.height, rt.graphicsFormat, rt.useMipMap);

                texture2D.ReadPixelsFrom(rt);
                texture2D.Apply();

                return texture2D;
            }

            //public static void SavePNG(this Texture2D texture2d, string path) => File.WriteAllBytes(path, texture2d.EncodeToPNG());

            public static Color GetRandomColor()
            {
                return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
            }


        #endregion



        #region Transform
            

            public static List<Transform> GetChildren(Transform transform = default)
            {
                var list = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                    list.Add(transform.GetChild(i));

                return list;
            }

            public static Transform DestroyChildren(Transform transform = default)
            {
                while (transform.childCount > 0)
                    if (Application.isPlaying)
                        Object.Destroy(transform.GetChild(0).gameObject);
                    else
                        Object.DestroyImmediate(transform.GetChild(0).gameObject);

                return transform;
            }


            #region Mouse Positions 2D
                public static Vector3 GetMouseWorldPosition2D()
                {
                    Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
                    vec.z = 0f;
                    return vec;
                }

                public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
                {
                    Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
                    return worldPosition;
                }

                public static Vector3 GetDirToMouse(Vector3 fromPosition)
                {
                    Vector3 mouseWorldPosition = GetMouseWorldPosition2D();
                    return (mouseWorldPosition - fromPosition).normalized;
                }
            #endregion


            public static Vector3 GetMouseWorldPosition3D()
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit, 100000, ~Physics.IgnoreRaycastLayer))
                    return hit.point;
                return Vector3.zero + ray.direction * 1000;
            }

            public static RaycastHit GetMouseRaycastHit3D()
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 100000, ~Physics.IgnoreRaycastLayer))
                    return hit;
                return default;
            }

            public static RaycastHit GetCameraForwardHit3D(float MaxDistance = 10000, Camera camera = null)
            {
                if(camera == null) camera = Camera.main;
                
                RaycastHit hit;
                if(Physics.Raycast(camera.transform.position, camera.transform.forward*10000, out hit, MaxDistance, ~Physics.IgnoreRaycastLayer))
                    return hit;
                return default;
            }


            public static Vector3 RandomVector3(float Offset)
            {
                return new Vector3(Random.Range(-Offset, Offset),
                                   Random.Range(-Offset, Offset),
                                   Random.Range(-Offset, Offset));
            }


        #endregion



        #region Navmesh
            

            public static float CalculatePathDistance(Vector3 startPos, Vector3 endPos, NavMeshAgent agent)
            {
                NavMeshPath path = new NavMeshPath();
                float distance = 0;

                if(NavMesh.CalculatePath(startPos, endPos, agent.areaMask, path))
                {
                    for(int i = 1; i < path.corners.Length; i++)
                    {
                        distance += Vector3.Distance(path.corners[i-1], path.corners[i]);
                    }
                }
                return distance;
            }


        #endregion



        #region Logic
            

            public static bool FrustumCheck(Collider collider, Camera cam)   // True if its in the Cameras Bounds
            {
                Bounds bounds = collider.bounds;
                Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

                return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
            }
            public static bool FrustumCheck(Collider[] colliders, Camera cam)   // True if one of them is in the Cameras Bounds
            {
                foreach(Collider collider in colliders)
                {
                    Bounds bounds = collider.bounds;
                    Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

                    if(GeometryUtility.TestPlanesAABB(frustumPlanes, bounds)) return true;
                }
                return false;
            }
            
            public static bool OcclusionCheck(this Transform transform, Transform target, float MaxDistance = 1000000, LayerMask layerMask = default) // True if its Occluded
            {
                if(layerMask == default) layerMask = ~Physics.IgnoreRaycastLayer;
                if(Physics.Raycast(transform.position, target.position-transform.position, out RaycastHit hit, MaxDistance, layerMask))
                {
                    return hit.transform.tag != "Player";
                }
                return true;
            }
            public static bool OcclusionCheck(Transform[] transforms, Transform target = null, float MaxDistance = 1000000, LayerMask layerMask = default) // True if all Points are Occluded
            {
                if(layerMask == default) layerMask = ~Physics.IgnoreRaycastLayer;
                foreach(Transform Point in transforms)
                {
                    if(Physics.Raycast(Point.position, target.position-Point.position, out RaycastHit hit, MaxDistance, layerMask))
                    {
                        if(hit.transform.tag == "Player") return false;
                    }
                }
                return true;
            }
            public static bool OcclusionCheck(Collider[] colliders, Transform target = null, float MaxDistance = 1000000, LayerMask layerMask = default) // True if all Points are Occluded
            {
                if(layerMask == default) layerMask = ~Physics.IgnoreRaycastLayer;
                foreach(Collider Point in colliders)
                {
                    if(Physics.Raycast(Point.transform.position, target.position-Point.transform.position, out RaycastHit hit, MaxDistance, layerMask))
                    {
                        if(hit.transform.tag == "Player") return false;
                    }
                }
                return true;
            }

            public static void ChangeScene(string sceneName)
            {
                SceneManager.LoadScene(sceneName);
            }

            
        #endregion


        
        #region Unity Editor
            

            public static void DrawThickRay(Vector3 start, Vector3 dir, Color color, float duration, float Thickness, int Iterations = 200)
            {
                for(int i = 0; i < 200; i++)
                {
                    start.x += UnityEngine.Random.Range(Thickness, -Thickness);
                    start.y += UnityEngine.Random.Range(Thickness, -Thickness);
                    start.z += UnityEngine.Random.Range(Thickness, -Thickness);
                    Debug.DrawRay(start, dir, color, duration);
                }
            }

            public static void ClearLogConsole()
            {
#if UNITY_EDITOR
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
                System.Type logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
                MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
                clearConsoleMethod.Invoke(new object(), null);
#endif
            }


        #endregion
    }
}
