using UnityEngine;

// port from: https://www.geeksforgeeks.org/min-heap-in-java/
// Test if the MinHeap class works:
// MinHeap minHeap = new MinHeap(15);
// minHeap.Insert(5);
// minHeap.Insert(3);
// minHeap.Insert(17);
// minHeap.Insert(10);
// minHeap.Insert(84);
// minHeap.Insert(19);
// minHeap.Insert(6);
// minHeap.Insert(22);
// minHeap.Insert(9);
// minHeap.Insert(1);
// minHeap.MinifyHeap();
// minHeap.Print();
// Debug.Log("The Min val is " + minHeap.Remove());
// as an alternative you can check:
// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/wiki/Getting-Started

public class MinHeap
{
	private int[] _heap;
	private int _size;
	private int _maxsize;

	private static int FRONT = 1;

	public MinHeap(int maxsize)
	{
		_maxsize = maxsize;
		_size = 0;
		_heap = new int[maxsize + 1];
		_heap[0] = int.MinValue;
	}

	// Function to return the position of 
	// the parent for the node currently 
	// at pos 
	private int Parent(int pos)
	{
		return pos / 2;
	}

	// Function to return the position of the 
	// left child for the node currently at pos 
	private int LeftChild(int pos)
	{
		return (2 * pos);
	}

	// Function to return the position of 
	// the right child for the node currently 
	// at pos 
	private int RightChild(int pos)
	{
		return (2 * pos) + 1;
	}

	// Function that returns true if the passed 
	// node is a leaf node 
	private bool IsLeaf(int pos)
	{
		if (pos >= (_size / 2) && pos <= _size)
		{
			return true;
		}
		return false;
	}

	// Function to swap two nodes of the heap 
	private void Swap(int fpos, int spos)
	{
		int tmp;
		tmp = _heap[fpos];
		_heap[fpos] = _heap[spos];
		_heap[spos] = tmp;
	}

	// Function to heapify the node at pos 
	private void MinHeapify(int pos)
	{

		// If the node is a non-leaf node and greater 
		// than any of its child 
		if (!IsLeaf(pos))
		{
			if (_heap[pos] > _heap[LeftChild(pos)]
				|| _heap[pos] > _heap[RightChild(pos)])
			{

				// Swap with the left child and heapify 
				// the left child 
				if (_heap[LeftChild(pos)] < _heap[RightChild(pos)])
				{
					Swap(pos, LeftChild(pos));
					MinHeapify(LeftChild(pos));
				}

				// Swap with the right child and heapify 
				// the right child 
				else
				{
					Swap(pos, RightChild(pos));
					MinHeapify(RightChild(pos));
				}
			}
		}
	}

	// Function to insert a node into the heap 
	public void Insert(int element)
	{
		if (_size >= _maxsize)
		{
			return;
		}
		_heap[++_size] = element;
		int current = _size;

		while (_heap[current] < _heap[Parent(current)])
		{
			Swap(current, Parent(current));
			current = Parent(current);
		}
	}

	// Function to print the contents of the heap 
	public void Print()
	{
		for (int i = 1; i <= _size / 2; i++)
		{
			Debug.Log(" PARENT : " + _heap[i]
							+ " LEFT CHILD : " + _heap[2 * i]
							+ " RIGHT CHILD :" + _heap[2 * i + 1]);
		}
	}

	// Function to build the min heap using 
	// the minHeapify 
	public void MinifyHeap()
	{
		for (int pos = (_size / 2); pos >= 1; pos--)
		{
			MinHeapify(pos);
		}
	}

	// Function to remove and return the minimum 
	// element from the heap 
	public int Remove()
	{
		int popped = _heap[FRONT];
		_heap[FRONT] = _heap[_size--];
		MinHeapify(FRONT);
		return popped;
	}
}
