// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/08 15:23:45
// FileName：Assets/Assets/Scripts/Tools/LRU.cs
// ========================================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRULInkedListNode<T>
{
    public T val;
    public LRULInkedListNode<T> prev;
    public LRULInkedListNode<T> Next;
}


public class LRUCache<T>
{

    private int capacity;

    public int Capacity { get { return capacity; } }
    public LRULInkedListNode<T> head;
    public LRULInkedListNode<T> tail;
    public Dictionary<T, LRULInkedListNode<T>> cache;
    public List<LRULInkedListNode<T>> pool;
    private const int POOL_EXTRA_SIZE = 5;

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        this.cache = new Dictionary<T, LRULInkedListNode<T>>(this.capacity);
        this.pool = new List<LRULInkedListNode<T>>(this.capacity + POOL_EXTRA_SIZE);
        GeneratePool();
        this.head = new LRULInkedListNode<T>();
        this.tail = new LRULInkedListNode<T>();

        Clear();
    }

    public void ModifyCapacity(int value)
    {
        capacity = value;
    }


    private void GeneratePool()
    {
        int cap = this.pool.Capacity;
        for(int i = 0; i < cap; i++)
        {
            LRULInkedListNode<T> node = new LRULInkedListNode<T>();
            pool.Add(node);
        }
    }

    public void Clear()
    {
        foreach(var p in this.cache)
        {
            this.pool.Add(p.Value);
        }
        this.cache.Clear();

        head.prev = null;
        head.Next = tail;
        tail.prev = head;
        tail.Next = null;
    }

    public int Count
    {
        get
        {
            return cache.Count;
        }
    }    


    public bool VisitAndTryRemove(T val , out T removeVal)
    {
        LRULInkedListNode<T> valNode = null;
        removeVal = default(T);
        bool isCacheContaionVal = cache.TryGetValue(val, out valNode);
        bool ret = false;

        if (isCacheContaionVal)
        {
            MoveNodeToTheFront(valNode);
        }
        else
        {
            LRULInkedListNode<T> node = PopNode();
            node.val = val;
            cache.Add(val, node);
            LinkAtTheFront(node);
            if (capacity < cache.Count)
            {
                removeVal = tail.prev.val;
                cache.Remove(tail.prev.val);
                RemoveTheLastNode();
                ret = true;
            }
        }
        return ret;
    }

    private LRULInkedListNode<T> PopNode()
    {
        LRULInkedListNode<T> node;
        if (pool.Count > 0)
        {
            int last = pool.Count - 1;
            node = pool[last];
            pool.RemoveAt(last);
        }
        else
        {
            node = new LRULInkedListNode<T>();
        }

        return node;
    }


    private void MoveNodeToTheFront(LRULInkedListNode<T> node)
    {
        LRULInkedListNode<T> prevNode = node.prev;
        LRULInkedListNode<T> nextNode = node.Next;

        prevNode.Next = nextNode;
        nextNode.prev = prevNode;
        LinkAtTheFront(node);

    }

    private void RemoveTheLastNode()
    {
        LRULInkedListNode<T> tailPrev = tail.prev;
        LRULInkedListNode<T> tailPrevPrev = tail.prev.prev;
        tailPrevPrev.Next = tail;
        tailPrev.prev = tailPrevPrev;
        RecycleNode(tailPrev);
        tailPrev = null;
    }

    private void LinkAtTheFront(LRULInkedListNode<T> node)
    {
        LRULInkedListNode<T> headNext = head.Next;
        head.Next = node;
        node.prev = head;
        node.Next = headNext;
        headNext.prev = node;
    }

    private void RecycleNode(LRULInkedListNode<T> node)
    {
        pool.Add(node);
    }
}
