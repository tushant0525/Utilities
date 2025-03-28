using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    // Override the OnGUI method to draw the slider
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var minMaxAttribute = (MinMaxSliderAttribute)attribute;

        // Set the tooltip to show the min and max values in a readable format
        label.tooltip = minMaxAttribute.min.ToString("F2") + " to " + minMaxAttribute.max.ToString("F2");

        // PrefixLabel gives us the space for the label and returns the rect for the rest of the control
        Rect controlRect = EditorGUI.PrefixLabel(position, label);

        // Split the control rect into 3 parts: min field, slider, max field
        Rect[] splittedRect = SplitRect(controlRect, 3);
        
        // Ensure the property is a Vector2 (min, max)
        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            EditorGUI.BeginChangeCheck();

            // Get the current values of the min and max from the Vector2 property
            Vector2 vector = property.vector2Value;
            float minVal = vector.x;
            float maxVal = vector.y;

            // Display editable min and max fields
            minVal = EditorGUI.FloatField(splittedRect[0], minVal);
            maxVal = EditorGUI.FloatField(splittedRect[2], maxVal);

            // Display the MinMaxSlider in the middle section
            EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal, minMaxAttribute.min, minMaxAttribute.max);

            // Clamping min/max values within the provided range
            minVal = Mathf.Clamp(minVal, minMaxAttribute.min, maxVal);
            maxVal = Mathf.Clamp(maxVal, minVal, minMaxAttribute.max);

            vector = new Vector2(minVal, maxVal);

            // If values changed, update the property
            if (EditorGUI.EndChangeCheck())
            {
                property.vector2Value = vector;
            }
        }
        else if (property.type=="MinMaxFloat")
        {
            // Get the property for min and max values (these will be floats)
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            EditorGUI.BeginChangeCheck();

            // Get the current values of the min and max from the Vector2 property
            Vector2 vector = new Vector2(minProp.floatValue, maxProp.floatValue);
            float minVal = vector.x;
            float maxVal = vector.y;

            // Display editable min and max fields
            minVal = EditorGUI.FloatField(splittedRect[0], minVal);
            maxVal = EditorGUI.FloatField(splittedRect[2], maxVal);

            // Display the MinMaxSlider in the middle section
            EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal, minMaxAttribute.min, minMaxAttribute.max);

            // Clamping min/max values within the provided range
            minVal = Mathf.Clamp(minVal, minMaxAttribute.min, maxVal);
            maxVal = Mathf.Clamp(maxVal, minVal, minMaxAttribute.max);

            vector = new Vector2(minVal, maxVal);

            // If values changed, update the property
            if (EditorGUI.EndChangeCheck())
            {
                minProp.floatValue = vector.x;
                maxProp.floatValue = vector.y;
            }
            
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use MinMaxSlider with Vector2.");
        }
    }

    // Helper method to split the control rect into 3 parts
    Rect[] SplitRect(Rect rectToSplit, int n)
    {
        Rect[] rects = new Rect[n];

        // Split the rect into equal widths
        for (int i = 0; i < n; i++)
        {
            rects[i] = new Rect(
                rectToSplit.position.x + (i * rectToSplit.width / n),
                rectToSplit.position.y,
                rectToSplit.width / n,
                rectToSplit.height
            );
        }


        // Adjust the padding and space between the elements
        int padding = (int)rects[0].width - 50; // Fixed padding to ensure there's some gap
        int space = 10;    // Small space between fields and slider

        // Adjust width and positioning to ensure proper alignment
        rects[0].width -= padding + space;  // Min field width adjustment
        rects[2].width -= padding + space;  // Max field width adjustment

        rects[1].x -= padding;              // Slider rect adjustment
        rects[1].width += padding * 2;      // Slider width adjustment

        rects[2].x += padding + space;     // Max field x-position adjustment


        return rects;
    }
}
