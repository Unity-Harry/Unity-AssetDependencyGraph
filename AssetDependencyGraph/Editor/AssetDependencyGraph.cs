using System.Collections.Generic;
using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
#endif
using UnityEngine;

public class AssetGraphView : GraphView
{
    public AssetGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());

        VisualElement background = new VisualElement
        {
            style =
            {
                backgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f)
            }
        };
        Insert(0, background);

        background.StretchToParentSize();
    }
}

public class AssetDependencyGraph : EditorWindow
{
    private const float kNodeWidth = 250.0f;

    private GraphView m_GraphView;

    private readonly List<GraphElement>        m_AssetElements  = new List<GraphElement>();
    private readonly Dictionary<string, Node>  m_GUIDNodeLookup = new Dictionary<string, Node>();
    private readonly List<Node>                m_DependenciesForPlacement = new List<Node>();

#if !UNITY_2019_1_OR_NEWER
    private VisualElement rootVisualElement;
#endif

    [MenuItem("Window/Analysis/Asset Dependency Graph")]
    public static void CreateTestGraphViewWindow()
    {
        var window = GetWindow<AssetDependencyGraph>();
        window.titleContent = new GUIContent("Asset Dependency Graph");
    }

    public void OnEnable()
    {
        m_GraphView = new AssetGraphView
        {
            name = "Asset Dependency Graph",
        };

        var toolbar = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexGrow = 0,
                backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.75f)
            }
        };

        var options = new VisualElement
        {
            style = { alignContent = Align.Center }
        };

        toolbar.Add(options);
        toolbar.Add(new Button(ExplodeAsset)
        {
            text = "Explore Asset",
        });
        toolbar.Add(new Button(ClearGraph)
        {
            text = "Clear"
        });

#if !UNITY_2019_1_OR_NEWER
        rootVisualElement = this.GetRootVisualContainer();
#endif
        rootVisualElement.Add(toolbar);
        rootVisualElement.Add(m_GraphView);
        m_GraphView.StretchToParentSize();
        toolbar.BringToFront();
    }

    public void OnDisable()
    {
        rootVisualElement.Remove(m_GraphView);
    }

    private void ExplodeAsset()
    {
        Object obj = Selection.activeObject;
        if (!obj)
            return;

        string assetPath = AssetDatabase.GetAssetPath(obj);

        Group      groupNode      = new Group {title = obj.name};
        Object     mainObject     = AssetDatabase.LoadMainAssetAtPath(assetPath);

        string[] dependencies    = AssetDatabase.GetDependencies(assetPath, false);
        bool     hasDependencies = dependencies.Length > 0;

        Node mainNode  = CreateNode(mainObject, assetPath, true, hasDependencies);

        mainNode.SetPosition(new Rect(0, 0, 0, 0));
        m_GraphView.AddElement(groupNode);
        m_GraphView.AddElement(mainNode);

        groupNode.AddElement(mainNode);

        CreateDependencyNodes(dependencies, mainNode, groupNode, 1);

        m_AssetElements.Add(mainNode);
        m_AssetElements.Add(groupNode);
        groupNode.capabilities &= ~Capabilities.Deletable;

        groupNode.Focus();

        mainNode.RegisterCallback<GeometryChangedEvent>(UpdateDependencyNodePlacement);
    }

    private void CreateDependencyNodes(string[] dependencies, Node parentNode, Group groupNode, int depth)
    {
        foreach (string dependencyString in dependencies)
        {
            Object dependencyAsset = AssetDatabase.LoadMainAssetAtPath(dependencyString);
            string[] deeperDependencies = AssetDatabase.GetDependencies(dependencyString, false);

            Node dependencyNode = CreateNode(dependencyAsset, AssetDatabase.GetAssetPath(dependencyAsset),
                false, deeperDependencies.Length > 0);

            if (!m_AssetElements.Contains(dependencyNode))
                dependencyNode.userData = depth;

            CreateDependencyNodes(deeperDependencies, dependencyNode, groupNode, depth + 1);

            if (!m_GraphView.Contains(dependencyNode))
                m_GraphView.AddElement(dependencyNode);

            Edge edge = new Edge
            {
                input = dependencyNode.inputContainer[0] as Port,
                output = parentNode.outputContainer[0] as Port,
            };
            edge.input?.Connect(edge);
            edge.output?.Connect(edge);

            dependencyNode.RefreshPorts();
            m_GraphView.AddElement(edge);

            if (!m_AssetElements.Contains(dependencyNode))
                groupNode.AddElement(dependencyNode);

            edge.capabilities &= ~Capabilities.Deletable;
            m_AssetElements.Add(edge);
            m_AssetElements.Add(dependencyNode);

            if (!m_DependenciesForPlacement.Contains(dependencyNode))
                m_DependenciesForPlacement.Add(dependencyNode);
        }
    }

    private Node CreateNode(Object obj, string assetPath, bool isMainNode, bool hasDependencies)
    {
        Node resultNode;
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        if (m_GUIDNodeLookup.TryGetValue(assetGUID, out resultNode))
        {
            int currentDepth = (int)resultNode.userData;
            resultNode.userData = currentDepth + 1;
            return resultNode;
        }

        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var assetGuid, out long _))
        {
            var objNode = new Node
            {
                title = obj.name,
                style =
                {
                    width = kNodeWidth
                }
            };

            objNode.extensionContainer.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 0.8f);

            objNode.titleContainer.Add(new Button(() =>
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            })
                {
                    style =
                    {
                        height = 16.0f,
                        alignSelf = Align.Center,
                        alignItems = Align.Center
                    },
                    text = "Select"
                });

            var infoContainer = new VisualElement
            {
                style =
                {
                    paddingBottom = 4.0f,
                    paddingTop = 4.0f,
                    paddingLeft = 4.0f,
                    paddingRight = 4.0f
                }
            };

            infoContainer.Add(new Label
            {
                text = assetPath,
#if UNITY_2019_1_OR_NEWER
                style = { whiteSpace = WhiteSpace.Normal }
#else
                style = { wordWrap = true }
#endif
            });

            var typeName = obj.GetType().Name;
            if (isMainNode)
            {
                var prefabType = PrefabUtility.GetPrefabAssetType(obj);
                if (prefabType != PrefabAssetType.NotAPrefab)
                    typeName = $"{prefabType} Prefab";
            }

            var typeLabel = new Label
            {
                text = $"Type: {typeName}"
            };
            infoContainer.Add(typeLabel);

            objNode.extensionContainer.Add(infoContainer);

            Texture assetTexture = AssetPreview.GetAssetPreview(obj);
            if (!assetTexture)
                assetTexture = AssetPreview.GetMiniThumbnail(obj);

            if (assetTexture)
            {
                AddDivider(objNode);

                objNode.extensionContainer.Add(new Image
                {
                    image = assetTexture,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        paddingBottom = 4.0f,
                        paddingTop = 4.0f,
                        paddingLeft = 4.0f,
                        paddingRight = 4.0f
                    }
                });
            }

            // Ports
            if (!isMainNode)
            {
                Port realPort = objNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Object));
                realPort.portName = "Dependent";
                objNode.inputContainer.Add(realPort);
            }

            if (hasDependencies)
            {
#if UNITY_2018_1
                Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, typeof(Object));
#else
                Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Object));
#endif
                port.portName = "Dependencies";
                objNode.outputContainer.Add(port);
                objNode.RefreshPorts();
            }

            resultNode = objNode;

            resultNode.RefreshExpandedState();
            resultNode.RefreshPorts();
            resultNode.capabilities &= ~Capabilities.Deletable;
            resultNode.capabilities |= Capabilities.Collapsible;
        }

        m_GUIDNodeLookup[assetGUID] = resultNode;
        return resultNode;
    }

    private static void AddDivider(Node objNode)
    {
        var divider = new VisualElement {name = "divider"};
        divider.AddToClassList("horizontal");
        objNode.extensionContainer.Add(divider);
    }

    private void ClearGraph()
    {
        foreach (var edge in m_AssetElements)
        {
            m_GraphView.RemoveElement(edge);
        }
        m_AssetElements.Clear();

        foreach (var node in m_AssetElements)
        {
            m_GraphView.RemoveElement(node);
        }
        m_AssetElements.Clear();
        m_GUIDNodeLookup.Clear();
    }

    private void UpdateDependencyNodePlacement(GeometryChangedEvent e)
    {
        // The current y offset in per depth
        var depthYOffset  = new Dictionary<int, float>();

        foreach (var node in m_DependenciesForPlacement)
        {
            int depth = (int)node.userData;

            if (!depthYOffset.ContainsKey(depth))
                depthYOffset.Add(depth, 0.0f);

            depthYOffset[depth] += node.layout.height;
        }

        // Move half of the node into negative y space so they're on either size of the main node in y axis
        var depths = new List<int>(depthYOffset.Keys);
        foreach (int depth in depths)
        {
            if (depth == 0)
                continue;

            float offset = depthYOffset[depth];
            depthYOffset[depth] = (0f - offset / 2.0f);
        }

        foreach (var node in m_DependenciesForPlacement)
        {
            int depth = (int)node.userData;
            node.SetPosition(new Rect(kNodeWidth * 1.5f * depth, depthYOffset[depth], 0, 0));
            depthYOffset[depth] += node.layout.height;
        }

        m_DependenciesForPlacement.Clear();
    }
}
