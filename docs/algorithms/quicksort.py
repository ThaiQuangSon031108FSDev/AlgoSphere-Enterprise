# QuickSort Implementation with Lomuto Partition
# Optimized for AlgoSphere Enterprise Engine

def quicksort(arr, low, high):
    if low < high:
        # pi is partitioning index, arr[p] is now at right place
        pi = partition(arr, low, high)

        # Separately sort elements before partition and after partition
        quicksort(arr, low, pi - 1)
        quicksort(arr, pi + 1, high)

def partition(arr, low, high):
    pivot = arr[high]
    i = low - 1  # index of smaller element

    for j in range(low, high):
        if arr[j] <= pivot:
            i = i + 1
            arr[i], arr[j] = arr[j], arr[i]

    arr[i + 1], arr[high] = arr[high], arr[i + 1]
    return i + 1

# Example
data = [10, 7, 8, 9, 1, 5]
quicksort(data, 0, len(data) - 1)
print(f"Sorted array: {data}")
