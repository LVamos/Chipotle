using System;
using System.Collections.Generic;

/// <summary>
/// Jednoduchá min-heap PriorityQueue pro Unity, 
/// inspirovaná .NET 6 PriorityQueue.
/// </summary>
/// <typeparam name="TElement">Typ ukládaného objektu</typeparam>
/// <typeparam name="TPriority">Typ priority, musí být IComparable</typeparam>
public class PriorityQueue<TElement, TPriority>
	where TPriority : IComparable<TPriority>
{
	private List<HeapItem> _heap = new();

	/// <summary>
	/// Počet prvků v haldě
	/// </summary>
	public int Count => _heap.Count;

	private struct HeapItem
	{
		public TElement Element;
		public TPriority Priority;

		public HeapItem(TElement element, TPriority priority)
		{
			Element = element;
			Priority = priority;
		}
	}

	/// <summary>
	/// Přidá prvek s danou prioritou do fronty.
	/// </summary>
	public void Enqueue(TElement element, TPriority priority)
	{
		_heap.Add(new HeapItem(element, priority));
		BubbleUp(_heap.Count - 1);
	}

	/// <summary>
	/// Vyjme a vrátí prvek s nejmenší prioritou (vrchol haldy).
	/// Pokud je prázdná, vyhodí výjimku.
	/// </summary>
	public TElement Dequeue()
	{
		if (_heap.Count == 0)
			throw new InvalidOperationException("PriorityQueue is empty.");

		// Vrchol
		TElement rootElement = _heap[0].Element;

		// Poslední prvek na místo 0, odstraň ho z konce
		int lastIndex = _heap.Count - 1;
		_heap[0] = _heap[lastIndex];
		_heap.RemoveAt(lastIndex);

		// Srovnat haldu dolů
		if (_heap.Count > 0)
		{
			BubbleDown(0);
		}

		return rootElement;
	}

	/// <summary>
	/// Podívá se na nejmenší prvek (vrchol haldy), ale nevyndá ho.
	/// </summary>
	public TElement Peek()
	{
		return _heap.Count == 0 ? throw new InvalidOperationException("PriorityQueue is empty.") : _heap[0].Element;
	}

	/// <summary>
	/// Pomůže srovnat nově přidaný prvek nahoru v haldě.
	/// </summary>
	private void BubbleUp(int index)
	{
		while (index > 0)
		{
			int parentIndex = (index - 1) / 2;

			if (_heap[index].Priority.CompareTo(_heap[parentIndex].Priority) < 0)
			{
				Swap(index, parentIndex);
				index = parentIndex;
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	/// Pomůže srovnat vrchol dolů v haldě (po Dequeue).
	/// </summary>
	private void BubbleDown(int index)
	{
		int count = _heap.Count;
		while (true)
		{
			int leftChild = 2 * index + 1;
			int rightChild = 2 * index + 2;
			int smallest = index;

			if (leftChild < count &&
				_heap[leftChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
			{
				smallest = leftChild;
			}

			if (rightChild < count &&
				_heap[rightChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
			{
				smallest = rightChild;
			}

			if (smallest == index)
				break;

			Swap(index, smallest);
			index = smallest;
		}
	}

	private void Swap(int i, int j)
	{
		HeapItem temp = _heap[i];
		_heap[i] = _heap[j];
		_heap[j] = temp;
	}
}
