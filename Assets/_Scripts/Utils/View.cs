using System;
using System.Collections.Generic;
using UnityEngine;

public static class ViewUtils {
    public static float CalculateOrthoSize(float width) {
        return 0.5f * width / ((float)Display.main.renderingWidth / (float)Display.main.renderingHeight);
    }
}
