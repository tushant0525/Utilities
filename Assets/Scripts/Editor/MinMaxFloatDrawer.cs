using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxFloat))]
public class MinMaxFloatDrawer :  PropertyDrawer
{
     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the property for min and max values (these will be floats)
        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");

        // Set the tooltip to show the min and max values in a readable format
        label.tooltip = minProp.floatValue.ToString("F2") + " to " + maxProp.floatValue.ToString("F2");

        // PrefixLabel gives us the space for the label and returns the rect for the rest of the control
        Rect controlRect = EditorGUI.PrefixLabel(position, label);

        // Split the control rect into 3 parts: min field, slider, max field
        Rect[] splittedRect = SplitRect(controlRect, 3);

        // Start drawing the custom GUI components for the min and max values
        EditorGUI.BeginChangeCheck();

        // Display editable min and max fields
        minProp.floatValue = EditorGUI.FloatField(splittedRect[0], minProp.floatValue);
        maxProp.floatValue = EditorGUI.FloatField(splittedRect[2], maxProp.floatValue);

        // Store the values into local variables to use with the slider
        float minValue = minProp.floatValue;
        float maxValue = maxProp.floatValue;

        // Ensure the values are within their allowed range
        minValue = Mathf.Min(minValue, maxValue);
        maxValue = Mathf.Max(minValue, maxValue);

        // Display the MinMaxSlider in the middle section
        EditorGUI.MinMaxSlider(splittedRect[1], ref minValue, ref maxValue, minProp.floatValue, maxProp.floatValue);

        // After the slider, update the min and max properties with the new values
        minProp.floatValue = minValue;
        maxProp.floatValue = maxValue;

        // If any value changed, update the property
        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
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

        // Adjust height: making the min and max fields smaller
        // float heightModifier = 0.75f; // Make min and max fields shorter by 25%
        // rects[0].height *= heightModifier; // Min field height smaller
        // rects[2].height *= heightModifier; // Max field height smaller
        // rects[1].height = rectToSplit.height; // Slider field height takes full space

        return rects;
    }
}
