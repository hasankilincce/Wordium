using UnityEngine;

public class CellIndexer : MonoBehaviour
{
    [ContextMenu("Reindex Cells")]
    public void ReindexCells()
    {
        // Bu scripti, Cell’lerin parent’ına eklediğinizi varsayıyoruz
        CellManager[] cells = GetComponentsInChildren<CellManager>();

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].cellIndex = i;
        }
    }
}
