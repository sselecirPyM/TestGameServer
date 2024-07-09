using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Simulation
{
    public abstract class ReadOnlyKDTree3<T>
    {
        public abstract void ForRange(Vector3 position, float radius, Action<T> action);
        public abstract void ForRange(Vector3 min, Vector3 max, Action<T> action);
        public abstract void Nearest(Vector3 position, float maxDistance, Predicate<T> filter, Action<T> action);
    }

    public enum SplitType
    {
        BranchX,
        BranchY,
        BranchZ,
        Leaf,
    }
    public sealed class Tree3Node
    {
        public int left;
        public int right;
        public float midValue;
        public SplitType splitType;
        public Tree3Node childL;
        public Tree3Node childR;
        public int childCount;
    }

    public struct Tree3NodeValue<T>
    {
        public Vector3 position;
        public T value;
    }
    public sealed class KDTree3Naive<T> : ReadOnlyKDTree3<T>
    {
        struct BB
        {
            public Vector3 min;
            public Vector3 max;

            public bool Contains(Vector3 position)
            {
                return Vector3.Clamp(position, min, max) == position;
            }
        }

        public Tree3Node root;
        public List<Tree3NodeValue<T>> values = new List<Tree3NodeValue<T>>();

        public void Add(T value, Vector3 position)
        {
            values.Add(new Tree3NodeValue<T>()
            {
                position = position,
                value = value
            });
        }

        public void Clear()
        {
            root = null;
            values.Clear();
        }

        public void Build()
        {
            if (root != null)
                throw new Exception("build a tree twice");

            Vector3 min = values[0].position;
            Vector3 max = values[0].position;
            for (int i = 1; i < values.Count; i++)
            {
                Tree3NodeValue<T> v = values[i];
                min = Vector3.Min(min, v.position);
                max = Vector3.Max(max, v.position);
            }
            root = _Build(0, values.Count, 0, new BB() { min = min, max = max });
        }

        public void ParallelBuild()
        {
            if (root != null)
                throw new Exception("build a tree twice");


            Vector3 min = values[0].position;
            Vector3 max = values[0].position;
            for (int i = 1; i < values.Count; i++)
            {
                Tree3NodeValue<T> v = values[i];
                min = Vector3.Min(min, v.position);
                max = Vector3.Max(max, v.position);
            }
            root = _ParallelBuild(0, values.Count, 0, new BB() { min = min, max = max });
        }

        public override void ForRange(Vector3 position, float radius, Action<T> action)
        {
            _Search(root, position, radius, radius * radius, action);
        }

        public override void ForRange(Vector3 min, Vector3 max, Action<T> action)
        {
            _Search(root, new BB() { min = Vector3.Min(min, max), max = Vector3.Max(min, max), }, action);
        }

        public override void Nearest(Vector3 position, float maxDistance, Predicate<T> filter, Action<T> action)
        {
            int resultIndex = -1;
            float md = maxDistance;
            float md2 = md * md;
            _Nearest(root, position, filter, ref resultIndex, ref md, ref md2);
            if (resultIndex >= 0)
            {
                action(values[resultIndex].value);
            }
        }

        void _Search(Tree3Node node, Vector3 position, float radius, float r2, Action<T> action)
        {
            if (node.childCount == 0)
            {
                return;
            }
            if (node.splitType == SplitType.Leaf)
            {
                for (int i = node.left; i < node.right; i++)
                {
                    var v = values[i];
                    if (Vector3.DistanceSquared(position, v.position) <= r2)
                    {
                        action(v.value);
                    }
                }
            }
            else
            {
                float l;
                float r;
                if (node.splitType == SplitType.BranchX)
                {
                    l = position.X - radius;
                    r = position.X + radius;
                }
                else if (node.splitType == SplitType.BranchY)
                {
                    l = position.Y - radius;
                    r = position.Y + radius;
                }
                else
                {
                    l = position.Z - radius;
                    r = position.Z + radius;
                }

                if (l <= node.midValue)
                {
                    _Search(node.childL, position, radius, r2, action);
                }
                if (r >= node.midValue)
                {
                    _Search(node.childR, position, radius, r2, action);
                }
            }
        }

        void _Search(Tree3Node node, BB bb, Action<T> action)
        {
            if (node.childCount == 0)
            {
                return;
            }
            if (node.splitType == SplitType.Leaf)
            {
                for (int i = node.left; i < node.right; i++)
                {
                    var v = values[i];
                    if (bb.Contains(v.position))
                    {
                        action(v.value);
                    }
                }
            }
            else
            {
                float l;
                float r;
                if (node.splitType == SplitType.BranchX)
                {
                    l = bb.min.X;
                    r = bb.max.X;
                }
                else if (node.splitType == SplitType.BranchY)
                {
                    l = bb.min.Y;
                    r = bb.max.Y;
                }
                else
                {
                    l = bb.min.Z;
                    r = bb.max.Z;
                }

                if (l <= node.midValue)
                {
                    _Search(node.childL, bb, action);
                }
                if (r >= node.midValue)
                {
                    _Search(node.childR, bb, action);
                }
            }
        }

        void _Nearest(Tree3Node node, Vector3 position, Predicate<T> filter, ref int result, ref float md, ref float md2)
        {
            if (node.splitType == SplitType.Leaf)
            {
                for (int i = node.left; i < node.right; i++)
                {
                    var v = values[i];
                    var xd2 = Vector3.DistanceSquared(position, v.position);
                    if (xd2 <= md2 && filter(v.value))
                    {
                        result = i;
                        md2 = xd2;
                        if (xd2 > 0)
                            md = MathF.Sqrt(xd2);
                        else
                            md = 0;
                    }
                }
            }
            else
            {
                float value;
                if (node.splitType == SplitType.BranchX)
                {
                    value = position.X;
                }
                else if (node.splitType == SplitType.BranchY)
                {
                    value = position.Y;
                }
                else
                {
                    value = position.Z;
                }
                if (value - md <= node.midValue)
                {
                    _Nearest(node.childL, position, filter, ref result, ref md, ref md2);
                }
                if (value + md >= node.midValue)
                {
                    _Nearest(node.childR, position, filter, ref result, ref md, ref md2);
                }
            }
        }

        Tree3Node _Build(int left, int right, int deep, in BB bb)
        {
            if (right - left < 12 || deep > 8)
            {
                var child = new Tree3Node()
                {
                    left = left,
                    right = right,
                    childCount = right - left,
                    splitType = SplitType.Leaf,
                };
                return child;
            }

            Vector3 min = bb.min;
            Vector3 max = bb.max;

            Vector3 size = max - min;

            if (size.X > size.Y && size.X > size.Z)
            {
                float midValue = (max.X + min.X) * 0.5f;
                int m = PartitionX(left, right, midValue);
                return _AddChild(left, right, m, midValue, deep, SplitType.BranchX, bb);
            }
            else if (size.Y > size.Z)
            {
                float midValue = (max.Y + min.Y) * 0.5f;
                int m = PartitionY(left, right, midValue);
                return _AddChild(left, right, m, midValue, deep, SplitType.BranchY, bb);
            }
            else
            {
                float midValue = (max.Z + min.Z) * 0.5f;
                int m = PartitionZ(left, right, midValue);
                return _AddChild(left, right, m, midValue, deep, SplitType.BranchZ, bb);
            }
        }

        Tree3Node _AddChild(int left, int right, int m, float midValue, int deep, SplitType splitType, in BB bb)
        {
            BB bb0 = bb;
            BB bb1 = bb;
            switch (splitType)
            {
                case SplitType.BranchX:
                    bb0.max.X = midValue;
                    bb1.min.X = midValue;
                    break;
                case SplitType.BranchY:
                    bb0.max.Y = midValue;
                    bb1.min.Y = midValue;
                    break;
                case SplitType.BranchZ:
                    bb0.max.Z = midValue;
                    bb1.min.Z = midValue;
                    break;
            }
            var c0 = _Build(left, m, deep + 1, bb0);
            var c1 = _Build(m, right, deep + 1, bb1);
            var child = new Tree3Node()
            {
                left = left,
                right = right,
                childCount = right - left,
                midValue = midValue,
                childL = c0,
                childR = c1,
                splitType = splitType,
            };
            return child;
        }

        Tree3Node _ParallelBuild(int left, int right, int deep, in BB bb)
        {
            if (right - left < 12 || deep > 8)
            {
                var child = new Tree3Node()
                {
                    left = left,
                    right = right,
                    childCount = right - left,
                    splitType = SplitType.Leaf,
                };
                return child;
            }

            Vector3 min = bb.min;
            Vector3 max = bb.max;

            Vector3 size = max - min;

            if (size.X > size.Y && size.X > size.Z)
            {
                float midValue = (max.X + min.X) * 0.5f;
                int m = PartitionX(left, right, midValue);
                return _ParallelAddChild(left, right, m, midValue, deep, SplitType.BranchX, bb);
            }
            else if (size.Y > size.Z)
            {
                float midValue = (max.Y + min.Y) * 0.5f;
                int m = PartitionY(left, right, midValue);
                return _ParallelAddChild(left, right, m, midValue, deep, SplitType.BranchY, bb);
            }
            else
            {
                float midValue = (max.Z + min.Z) * 0.5f;
                int m = PartitionZ(left, right, midValue);
                return _ParallelAddChild(left, right, m, midValue, deep, SplitType.BranchZ, bb);
            }
        }

        Tree3Node _ParallelAddChild(int left, int right, int m, float midValue, int deep, SplitType splitType, in BB bb)
        {
            BB bb0 = bb;
            BB bb1 = bb;
            switch (splitType)
            {
                case SplitType.BranchX:
                    bb0.max.X = midValue;
                    bb1.min.X = midValue;
                    break;
                case SplitType.BranchY:
                    bb0.max.Y = midValue;
                    bb1.min.Y = midValue;
                    break;
                case SplitType.BranchZ:
                    bb0.max.Z = midValue;
                    bb1.min.Z = midValue;
                    break;
            }

            Tree3Node c0 = null;
            Tree3Node c1 = null;

            if (deep < 3)
            {
                Parallel.Invoke(() =>
                {
                    c0 = _ParallelBuild(left, m, deep + 1, bb0);
                },
                () =>
                {
                    c1 = _ParallelBuild(m, right, deep + 1, bb1);
                });
            }
            else
            {
                c0 = _Build(left, m, deep + 1, bb0);
                c1 = _Build(m, right, deep + 1, bb1);
            }
            var child = new Tree3Node()
            {
                left = left,
                right = right,
                childCount = right - left,
                midValue = midValue,
                childL = c0,
                childR = c1,
                splitType = splitType,
            };
            return child;
        }

        int PartitionX(int left, int right, float midValue)
        {
            if (left == right)
                return left;
            for (int i = left; i < right; i++)
            {
                if (values[i].position.X < midValue)
                {
                    (values[i], values[left]) = (values[left], values[i]);
                    left++;
                }
            }
            return left;
        }
        int PartitionY(int left, int right, float midValue)
        {
            if (left == right)
                return left;
            for (int i = left; i < right; i++)
            {
                if (values[i].position.Y < midValue)
                {
                    (values[i], values[left]) = (values[left], values[i]);
                    left++;
                }
            }
            return left;
        }
        int PartitionZ(int left, int right, float midValue)
        {
            if (left == right)
                return left;
            for (int i = left; i < right; i++)
            {
                if (values[i].position.Z < midValue)
                {
                    (values[i], values[left]) = (values[left], values[i]);
                    left++;
                }
            }
            return left;
        }
    }


}
