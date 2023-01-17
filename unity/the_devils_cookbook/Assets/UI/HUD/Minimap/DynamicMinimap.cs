using System;
using System.Collections.Generic;
using System.Linq;
using TDC.Core.Manager;
using TDC.Ingredient;
using TDC.Patrons;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TDC.UI.HUD.Minimap
{
    public class DynamicMinimap : MonoBehaviour, IHideable
    {
        private static Mesh _QuadMesh;
        private static bool _IsMeshInitialised = false;

        private class RenderableData
        {
            public Transform ObjectTransform;
            public BoxCollider ObjectCollider;
        }

        private class IconData
        {
            public RenderableData RenderableData;
            public IconObject IconObject;
            public int IconIndex;
        }

        private class WindowIconData
        {
            public PatronWindow Window;
            public Matrix4x4 WindowMatrix;
            public int TextureIndex;
        }

        public float MinimapWorldSize = 20;
        public Vector3 MinimapWorldOrigin;

        public Color BackgroundColour = new Color(0, 0, 0, 0);

        public Color[] FlatColours = new[]
        {
            new Color(1,0,0,1),
            new Color(0,1,0,1),
            new Color(0,0,1,1)
        };

        public Texture2D MaggieIcon;
        public Vector3 MaggieIconSize = Vector3.one;
        [FormerlySerializedAs("IngredientTexture")] public Texture2D IngredientIcon;
        public Vector3 IngredientIconSize = Vector3.one;
        public Vector3 WindowIconSize = Vector3.one;
        
        /// <summary>
        /// Textures for use by minimap elements.
        /// </summary>
        private List<Texture> _ElementIcons = new List<Texture>();

        private Dictionary<Texture, int> _IconIndicesByTexture = new Dictionary<Texture, int>();

        /// <summary>
        /// Material used to render the background objects (Walls, floors, Windows)
        /// </summary>
        [SerializeField] private Material _StaticRenderMaterial;
        /// <summary>
        /// Material used to render foreground objects (Maggie, Window icons, ingredients, Appliances)
        /// </summary>
        [FormerlySerializedAs("_RenderMaterial")] [SerializeField] private Material _DynamicRenderMaterial;
        
        private List<IconData> _IconObjects = new List<IconData>();
        private List<WindowIconData> _WindowIconData = new List<WindowIconData>();
        private LinkedList<Transform> _CreatureTransforms = new LinkedList<Transform>();

        private Dictionary<Creature, LinkedListNode<Transform>> _NodesByCreature =
            new Dictionary<Creature, LinkedListNode<Transform>>();

        
        private Camera _RenderCam;
        private CommandBuffer _CommandBuffer;

        private Matrix4x4 _BackgroundRenderMatrix;
        
        private bool _HasBackgroundRendered = false;
        private RenderTexture _StaticTexture;
        private RenderTexture _MinimapTexture;
        private Texture2DArray _TextureArray;
        private ComputeBuffer _DynamicIndexBuffer;
        
        private static readonly int _TextureIndicesID = Shader.PropertyToID("textureIndices");
        private static readonly int _ColoursID = Shader.PropertyToID("colours");
        private static readonly int _IconsID = Shader.PropertyToID("icons");
        private static readonly int _ColourIndicesID = Shader.PropertyToID("colourIndices");
        private static readonly int _StaticTexID = Shader.PropertyToID("staticTex");

        // Start is called before the first frame update
        private async void Start()
        {
            if (!_IsMeshInitialised) InitialiseMesh();
            await GameManager.LevelInitialisedAsync.WaitAsync();
            try
            {
                InitialiseMinimap();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private static void InitialiseMesh()
        {
            _QuadMesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, 0, 0.5f),
                    new Vector3(0.5f, 0, 0.5f),
                    new Vector3(0.5f, 0, -0.5f),
                    new Vector3(-0.5f, 0, -0.5f)
                },
                triangles = new[]
                {
                    0, 1, 2,
                    0, 2, 3
                },
                uv = new[]
                {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0)
                }
            };
            _QuadMesh.RecalculateNormals();
            _QuadMesh.RecalculateBounds();
            _IsMeshInitialised = true;
        }

        private static void GetStaticData(ref List<Matrix4x4> matrices, ref List<int> textureIndices, 
            IEnumerable<BoxCollider> colliders, int texIndex)
        {
            foreach (BoxCollider objectCollider in colliders)
            {
                Transform staticTransform = objectCollider.transform;
                matrices.Add(Matrix4x4.TRS(staticTransform.position +
                                            staticTransform.localToWorldMatrix.MultiplyVector(objectCollider.center),
                    staticTransform.rotation, objectCollider.size));
                textureIndices.Add(texIndex);
            }
        }

        private const int IconSize = 64;
        private void CopyTexturesToArray()
        {
            // Appliances, MaggieIcon, IngredientIcon
            int texCount = _ElementIcons.Count + 2;
            _TextureArray = new Texture2DArray(IconSize, IconSize, texCount , TextureFormat.DXT5, false);
            var i = 0;
            foreach (Texture icon in _ElementIcons)
            {
                Graphics.CopyTexture(icon, 0, _TextureArray, i++);
            }
            Graphics.CopyTexture(MaggieIcon, 0, _TextureArray, i++);
            Graphics.CopyTexture(IngredientIcon, 0, _TextureArray, i);
            _DynamicRenderMaterial.SetTexture(_IconsID, _TextureArray);
            
            _IconIndicesByTexture = null;
        }
        
        private void InitialiseApplianceIcons()
        {
            IconObject[] components = FindObjectsOfType<IconObject>(true);
            foreach (IconObject iconObject in components)
            {
                var iconData = new IconData()
                {
                    IconObject = iconObject,
                    RenderableData = new RenderableData()
                    {
                        ObjectCollider = iconObject.GetComponentInChildren<BoxCollider>(),
                        ObjectTransform = iconObject.transform
                    }
                };
                if (_IconIndicesByTexture.TryGetValue(iconObject.Icon, out int index)) iconData.IconIndex = index;
                else
                {
                    _ElementIcons.Add(iconObject.Icon);
                    _IconIndicesByTexture.Add(iconObject.Icon, _ElementIcons.Count - 1);
                    iconData.IconIndex = _ElementIcons.Count - 1;
                }
                _IconObjects.Add(iconData);
            }
            

        }

        private void InitialiseWindowIcons()
        {
            foreach (PatronWindow window in GameManager.PatronManager.PatronWindows)
            {
                Transform windowTransform = window.transform;
                var data = new WindowIconData
                {
                    Window = window,
                    // WindowMatrix = Matrix4x4.TRS(windowTransform.position 
                    //                              + windowTransform.localToWorldMatrix.MultiplyVector(window.MinimapIconOffset),
                    WindowMatrix = Matrix4x4.TRS(windowTransform.position + window.MinimapIconOffset,
                        Quaternion.identity, WindowIconSize)
                };
                if (_IconIndicesByTexture.TryGetValue(window.MinimapIDIcon, out int index)) data.TextureIndex = index;
                else
                {
                    if (window.MinimapIDIcon == null)
                    {
                        Debug.LogWarning($"MinimapIDIcon for {window.gameObject.name} is null.");
                        continue;
                    }
                    _ElementIcons.Add(window.MinimapIDIcon);
                    _IconIndicesByTexture.Add(window.MinimapIDIcon, _ElementIcons.Count - 1);
                    data.TextureIndex = _ElementIcons.Count - 1;
                }

                _WindowIconData.Add(data);
            }
        }
        
        private void RenderStaticObjects()
        {
            var staticMatrices = new List<Matrix4x4>();
            var staticColourIndices = new List<int>();
            
            IEnumerable<BoxCollider> floors = GameObject.FindGameObjectsWithTag("WalkableSurface")
                .Select(go => go.GetComponentInChildren<BoxCollider>()).Where(c => c);
            IEnumerable<BoxCollider> walls = GameObject.FindGameObjectsWithTag("Wall")
                .Select(go => go.GetComponentInChildren<BoxCollider>()).Where(c => c);
            IEnumerable<BoxCollider> windows = GameObject.FindObjectsOfType<PatronWindow>(true)
                .Select(go => go.GetComponentInChildren<BoxCollider>()).Where(c => c);
            
            GetStaticData(ref staticMatrices, ref staticColourIndices, floors, 0);
            GetStaticData(ref staticMatrices, ref staticColourIndices, walls, 1);
            GetStaticData(ref staticMatrices, ref staticColourIndices, windows, 2);

            var colourBuffer = new ComputeBuffer(FlatColours.Length, 16);
            colourBuffer.SetData(FlatColours);
            var indexBuffer = new ComputeBuffer(staticColourIndices.Count, 4);
            indexBuffer.SetData(staticColourIndices);
            
            _StaticRenderMaterial.SetBuffer(_ColoursID, colourBuffer);
            _StaticRenderMaterial.SetBuffer(_ColourIndicesID, indexBuffer);

            var cmd = new CommandBuffer();
            cmd.SetRenderTarget(_StaticTexture);
            cmd.ClearRenderTarget(false, true, BackgroundColour);
            cmd.SetViewMatrix(_RenderCam.worldToCameraMatrix);
            cmd.SetProjectionMatrix(_RenderCam.projectionMatrix);
            cmd.DrawMeshInstanced(_QuadMesh, 0, _StaticRenderMaterial, 0, 
                staticMatrices.ToArray(), staticMatrices.Count);
            Graphics.ExecuteCommandBuffer(cmd);

            cmd.Release();
            colourBuffer.Release();
            indexBuffer.Release();
            
            _DynamicRenderMaterial.SetTexture(_StaticTexID, _StaticTexture);
            Debug.Log($"Floors: {floors.Count()}, walls: {walls.Count()}, windows: {windows.Count()}");
        }

        private void GenerateCamera()
        {
            _RenderCam = new GameObject("Minimap_Camera").AddComponent<Camera>();
            Transform camTransform = _RenderCam.transform;
            camTransform.position = new Vector3(MinimapWorldOrigin.x, 50, MinimapWorldOrigin.z);
            camTransform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            _RenderCam.orthographic = true;
            _RenderCam.orthographicSize = MinimapWorldSize;
            _RenderCam.aspect = 1;
            _RenderCam.enabled = false;
        }

        private void CreateRenderTextures()
        {
            _MinimapTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBFloat)
            {
                name = "MinimapRT"
            };
            _MinimapTexture.Create();
            _StaticTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBFloat)
            {
                name = "MinimapStaticRT"
            };
            _StaticTexture.Create();
            GetComponent<RawImage>().texture = _MinimapTexture;
        }

        private List<Matrix4x4> GenerateDynamicData()
        {
            WindowIconData[] activeWindows = _WindowIconData.Where(d => d.Window.Occupant != null).ToArray();
            int count = 2 + _IconObjects.Count + activeWindows.Length + _CreatureTransforms.Count;
            if (_DynamicIndexBuffer == null || _DynamicIndexBuffer.count != count)
            {
                _DynamicIndexBuffer?.Release();
                _DynamicIndexBuffer = new ComputeBuffer(count, 4, ComputeBufferType.Default);
            }
            
            // Background
            var currentBufferOffset = 0;
            _DynamicIndexBuffer.SetData(new[] {0}, 0, currentBufferOffset++, 1);
            var matrices = new List<Matrix4x4>(count) { _BackgroundRenderMatrix };
            
            // Appliances
            _DynamicIndexBuffer.SetData(_IconObjects.Select(i => i.IconIndex).ToArray(), 0, 
                currentBufferOffset, _IconObjects.Count);
            currentBufferOffset += _IconObjects.Count;
            
            matrices.AddRange(_IconObjects.Select(iconData =>
                Matrix4x4.TRS(
                    iconData.RenderableData.ObjectTransform.position +
                    iconData.RenderableData.ObjectTransform.localToWorldMatrix.MultiplyVector(iconData.IconObject
                        .IconLocalOffset), Quaternion.LookRotation(Vector3.forward),
                    iconData.IconObject.Size)));
            
            // Windows
            _DynamicIndexBuffer.SetData(activeWindows.Select(d => d.TextureIndex).ToArray(), 
                0, currentBufferOffset, activeWindows.Length);
            currentBufferOffset += activeWindows.Length;
            matrices.AddRange(activeWindows.Select(d => d.WindowMatrix));
            
            // Maggie
            matrices.Add(Matrix4x4.TRS(GameManager.PlayerCharacter.transform.position, 
                Quaternion.identity, MaggieIconSize));
            _DynamicIndexBuffer.SetData(new[] { _ElementIcons.Count }, 0, currentBufferOffset++, 1);

            // Creatures
            matrices.AddRange(_CreatureTransforms.Select(t => Matrix4x4.TRS(
                t.position, Quaternion.identity, IngredientIconSize)));
            var creatureIndices = new int[_CreatureTransforms.Count];
            for (var i = 0; i < creatureIndices.Length; i++)
            {
                creatureIndices[i] = _ElementIcons.Count + 1;
                
            }
            _DynamicIndexBuffer.SetData(creatureIndices, 0, currentBufferOffset, _CreatureTransforms.Count);
            
            return matrices;
        }
        
        private void PopulateCommandBuffer(List<Matrix4x4> matrices)
        {
            _CommandBuffer.Clear();
            _CommandBuffer.SetRenderTarget(_MinimapTexture, _MinimapTexture);
            _CommandBuffer.ClearRenderTarget(false, true, Color.clear);
            _CommandBuffer.SetViewMatrix(_RenderCam.worldToCameraMatrix);
            _CommandBuffer.SetProjectionMatrix(_RenderCam.projectionMatrix);

            _DynamicRenderMaterial.SetBuffer(_TextureIndicesID, _DynamicIndexBuffer);
            _CommandBuffer.DrawMeshInstanced(_QuadMesh, 0, _DynamicRenderMaterial, 0, 
                matrices.ToArray(), matrices.Count);
        }
        
        private void InitialiseMinimap()
        {
            CreatureManager.CreatureCreated += OnCreatureCreated;
            CreatureManager.CreatureDestroyed += OnCreatureDestroyed;
            
            GenerateCamera();
            CreateRenderTextures();
            _BackgroundRenderMatrix = Matrix4x4.TRS(MinimapWorldOrigin, Quaternion.identity,
                new Vector3(MinimapWorldSize, MinimapWorldSize, MinimapWorldSize) * 2);
            _CommandBuffer = new CommandBuffer();
            
            InitialiseApplianceIcons();
            InitialiseWindowIcons();
            
            CopyTexturesToArray();
            
            RenderPipelineManager.beginFrameRendering += Render;
        }

        private void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            if (!_HasBackgroundRendered)
            {
                RenderStaticObjects();
                _HasBackgroundRendered = true;
            }
            List<Matrix4x4> matrices = GenerateDynamicData();
            PopulateCommandBuffer(matrices);

            Graphics.ExecuteCommandBuffer(_CommandBuffer);
        }

        private void OnCreatureCreated(Creature creature)
        {
            _NodesByCreature.Add(creature, _CreatureTransforms.AddLast(creature.transform));
        }

        private void OnCreatureDestroyed(Creature creature)
        {
            if (!_NodesByCreature.TryGetValue(creature, out LinkedListNode<Transform> node)) return;
            _NodesByCreature.Remove(creature);
            _CreatureTransforms.Remove(node);
        }
        
        private void OnDestroy()
        {
            _MinimapTexture.Release();
            _StaticTexture.Release();
            RenderPipelineManager.beginFrameRendering -= Render;
            _CommandBuffer.Release();
            _DynamicIndexBuffer.Release();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(MinimapWorldOrigin, new Vector3(MinimapWorldSize * 2, 20, MinimapWorldSize * 2));
        }

        public void SetHidden(bool isHidden)
        {
            transform.parent.gameObject.SetActive(!isHidden);
        }
    }
}
