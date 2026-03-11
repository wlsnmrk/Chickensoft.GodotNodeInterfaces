namespace Chickensoft.GodotNodeInterfaces;

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Godot;

public class FakeNodeTree
{
  private Node _node;
  // Map of node paths to FakeSceneTreeNode instances.
  private readonly OrderedDictionary _fakeNodes;

  private int _nextId;

  public FakeNodeTree(
    Node node,
    Dictionary<string, INode>? fakeNodes = null
  )
  {
    _node = node;
    _fakeNodes = [];

    if (fakeNodes is Dictionary<string, INode> dict)
    {
      foreach (var (path, fakeNode) in dict)
      {
        _fakeNodes.Add(path, fakeNode);
      }
    }
  }

  public Node? GetParent() => _node.GetParent();

  public T? GetParent<T>() where T : class, INode =>
    GodotInterfaces.AdaptOrNull<T>(_node.GetParent());

  public void AddChild(INode node)
  {
    var name = "";
    // We use try/catch to check node name since not all node mocks may
    // have stubbed the Name property.
    try
    {
      name = node.Name;
    }
    catch { }

    if (string.IsNullOrEmpty(name))
    {
      name = node.GetType().Name + "@" + _nextId++;
    }

    _fakeNodes.Add(name, node);
  }

  public INode? GetNode(string path) =>
    _fakeNodes.Contains(path) ? (INode)_fakeNodes[path]! : null;

  public T? GetNode<T>(string path) where T : class, INode =>
    _fakeNodes.Contains(path) ? (T)_fakeNodes[path]! : null;

  public INode? FindChild(string pattern)
  {
    foreach (string path in _fakeNodes.Keys)
    {
      var node = (INode)_fakeNodes[path]!;
      var name = "";
      // We use try/catch to check node name since not all node mocks may
      // have stubbed the Name property.
      try
      {
        name = node.Name;
      }
      catch { }

      if (!string.IsNullOrEmpty(name) && name.Match(pattern))
      {
        return node;
      }
    }

    return null;
  }

  public bool HasNode(NodePath path) => _fakeNodes.Contains((string)path);

  public INode[] FindChildren(string pattern)
  {
    var children = new List<INode>();

    foreach (string path in _fakeNodes.Keys)
    {
      var node = (INode)_fakeNodes[path]!;
      var name = "";
      try
      {
        name = node.Name;
      }
      catch { }

      if (!string.IsNullOrEmpty(name) && name.Match(pattern))
      {
        children.Add((INode)_fakeNodes[path]!);
      }
    }

    return [.. children];
  }

  public T? GetChild<T>(int index) where T : class, INode
  {
    if (index >= _fakeNodes.Count)
    {
      return null;
    }

    var actualIndex = index;
    if (actualIndex < 0)
    {
      // Negative indices access the children from the last one.
      actualIndex = _fakeNodes.Count + actualIndex;
    }

    var child = _fakeNodes[actualIndex];
    return child is null ? null : (T)child;
  }

  public INode? GetChild(int index) => GetChild<INode>(index);

  public int GetChildCount() => _fakeNodes.Count;

  public INode[] GetChildren() => [.. _fakeNodes.Values.Cast<INode>()];

  public void RemoveChild(INode node)
  {
    var path = _fakeNodes
      .Keys
      .Cast<string>()
      .First(k => _fakeNodes[k] == node);
    _fakeNodes.Remove(path);
  }

  public Dictionary<string, INode> GetAllNodes()
  {
    var nodes = new Dictionary<string, INode>();

    foreach (string path in _fakeNodes.Keys)
    {
      var node = (INode)_fakeNodes[path]!;
      nodes.Add(path, node);
    }

    return nodes;
  }
}
