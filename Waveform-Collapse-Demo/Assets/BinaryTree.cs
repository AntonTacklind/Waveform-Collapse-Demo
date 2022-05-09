using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryTree<T>
{
    public BinaryTree<T> parent;
    public BinaryTree<T> lessLeaf;
    public BinaryTree<T> moreLeaf;
    public T value;
    public IComparer comparer;
    public bool withered = false;

    public void Insert(T obj)
    {
        //If its the same object, dont do anything
        if (EqualityComparer<T>.Default.Equals(value, obj))
        {
            return;
        }
        if (value == null)
        {
            value = obj;
            return;
        }
        int comp = comparer.Compare(value, obj);
        if (comp < 0)
        {
            if (lessLeaf == null)
            {
                lessLeaf = new BinaryTree<T>();
                lessLeaf.parent = this;
                lessLeaf.value = obj;
                lessLeaf.comparer = comparer;
            }
            else
            {
                lessLeaf.Insert(obj);
            }
        }
        else
        {
            if (moreLeaf == null)
            {
                moreLeaf = new BinaryTree<T>();
                moreLeaf.parent = this;
                moreLeaf.value = obj;
                moreLeaf.comparer = comparer;
            }
            else
            {
                moreLeaf.Insert(obj);
            }
        }
    }

    public T Extract()
    {
        if (lessLeaf != null)
        {
            T ret = lessLeaf.Extract();
            if (lessLeaf.withered)
            {
                lessLeaf = null;
            }
            return ret;
        }
        else
        {
            T ret = value;
            value = default(T);
            if (moreLeaf != null)
            {
                CopyFrom(moreLeaf);
            }
            else
            {
                withered = true;
            }
            return ret;
        }
    }

    public int Count()
    {
        int count = 0;
        if (lessLeaf != null)
        {
            count += lessLeaf.Count();
        }
        if (value != null)
        {
            count++;
        }
        if (moreLeaf != null)
        {
            count += moreLeaf.Count();
        }
        return count;
    }

    public void CopyFrom(BinaryTree<T> other)
    {
        lessLeaf = other.lessLeaf;
        value = other.value;
        moreLeaf = other.moreLeaf;
        comparer = other.comparer;
    }
}
