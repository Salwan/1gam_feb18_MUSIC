using UnityEngine;
using System.Collections;

public class Utils
{

	public static int IndexFromColumnRow(int column, int row, int columns_count) {
		return (row * columns_count) + column;
	}

	public static Rect RectTransformToScreenSpace(RectTransform transform)
     {
         Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
         Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
         rect.x -= (transform.pivot.x * size.x);
         rect.y -= ((1.0f - transform.pivot.y) * size.y);
         return rect;
     }
}

