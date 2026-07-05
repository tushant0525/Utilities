using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimatorStateReference))]
public sealed class AnimatorStateReferenceDrawer : PropertyDrawer
{
	#region Constants
    
	private const string CONTROLLER_PROPERTY_NAME = "m_Controller";
	private const string STATE_NAME_PROPERTY_NAME = "m_StateName";
	private const string LAYER_ID_PROPERTY_NAME = "m_LayerID";
	private const string NONE_LABEL = "<None>";
	private const string DUPLICATE_WARNING = "Duplicate state names detected. Use LayerID with the state name.";
	private const float DUPLICATE_WARNING_HEIGHT = 36f;
    
	#endregion

	#region Nested Types
    
	private readonly struct StateOption
	{
		public GUIContent Label { get; }
		public string Value { get; }
		public int LayerID { get; }

		public StateOption(string a_DisplayPath, string a_Value, int a_LayerID)
		{
			Label = new GUIContent(a_DisplayPath);
			Value = a_Value;
			LayerID = a_LayerID;
		}
	}
    
	#endregion

	#region Property Drawer
    
	public override void OnGUI(Rect a_Position, SerializedProperty a_Property, GUIContent a_Label)
	{
		EditorGUI.BeginProperty(a_Position, a_Label, a_Property);

		try
		{
			Rect l_FieldPosition = a_Position;
			l_FieldPosition.height = EditorGUIUtility.singleLineHeight;

			SerializedProperty l_ControllerProperty = a_Property.FindPropertyRelative(CONTROLLER_PROPERTY_NAME);
			SerializedProperty l_StateNameProperty = a_Property.FindPropertyRelative(STATE_NAME_PROPERTY_NAME);
			SerializedProperty l_LayerIDProperty = a_Property.FindPropertyRelative(LAYER_ID_PROPERTY_NAME);

			if (l_ControllerProperty == null || l_StateNameProperty == null || l_LayerIDProperty == null)
			{
				DrawMessage(l_FieldPosition, a_Label, "AnimatorStateReference data could not be found.");
				return;
			}

			SplitFieldPosition(l_FieldPosition, a_Label, out Rect l_ControllerPosition, out Rect l_StatePosition);

			EditorGUI.PropertyField(l_ControllerPosition, l_ControllerProperty, GUIContent.none);

			if (l_ControllerProperty.hasMultipleDifferentValues)
			{
				DrawDisabledPopup(l_StatePosition,"Multiple Controllers");
				return;
			}

			if (l_ControllerProperty.objectReferenceValue == null)
			{
				ClearStateReference(l_StateNameProperty, l_LayerIDProperty);
				DrawDisabledPopup(l_StatePosition, "Select Controller");
				return;
			}

			AnimatorController l_Controller = l_ControllerProperty.objectReferenceValue as AnimatorController;

			if (l_Controller == null)
			{
				DrawDisabledPopup(l_StatePosition, "Invalid Controller");
				return;
			}
			List<StateOption> l_States = new List<StateOption>();
			CollectStates(l_Controller, l_States);

			if (l_States.Count == 0)
			{
				DrawDisabledPopup(l_StatePosition, "No States Found");
				return;
			}

			HashSet<string> l_DuplicateNames = GetDuplicateStateNames(l_States);

			DrawStatePopup(l_StatePosition,
				           l_StateNameProperty,
				           l_LayerIDProperty,
				           l_States,
				           l_DuplicateNames);

			if (l_DuplicateNames.Count > 0)
			{
				Rect l_WarningPosition = new Rect(a_Position.x,
                                                  l_FieldPosition.yMax + EditorGUIUtility.standardVerticalSpacing,
					                              a_Position.width,
					                              DUPLICATE_WARNING_HEIGHT);

				EditorGUI.HelpBox(l_WarningPosition, DUPLICATE_WARNING, MessageType.Warning);
			}
		}
		finally
		{
			EditorGUI.EndProperty();
		}
	}
	public override float GetPropertyHeight(SerializedProperty a_Property, GUIContent a_Label)
	{
		float l_Height = EditorGUIUtility.singleLineHeight;

		if (HasDuplicateStateNames(a_Property))
		{
			l_Height += EditorGUIUtility.standardVerticalSpacing;
			l_Height += DUPLICATE_WARNING_HEIGHT;
		}

		return l_Height;
	}
    
	#endregion

	#region Drawing
    
	private static void SplitFieldPosition(Rect a_Position, GUIContent a_Label, out Rect a_ControllerPosition, out Rect a_StatePosition)
	{
		Rect l_ContentPosition = EditorGUI.PrefixLabel(a_Position, a_Label);
		const float SPACING = 4f;
		float l_ControllerWidth = Mathf.Min(180f, l_ContentPosition.width * 0.45f);

		a_ControllerPosition = new Rect(l_ContentPosition.x,
			                            l_ContentPosition.y,
			                            l_ControllerWidth,
			                            l_ContentPosition.height);

		a_StatePosition = new Rect(a_ControllerPosition.xMax + SPACING,
			                       l_ContentPosition.y,
                                   Mathf.Max(0f, l_ContentPosition.width - l_ControllerWidth - SPACING),
			                       l_ContentPosition.height);
	}
	private static void DrawStatePopup(Rect a_Position,
		                               SerializedProperty a_StateNameProperty,
		                               SerializedProperty a_LayerIDProperty,
		                               List<StateOption> a_States,
		                               HashSet<string> a_DuplicateNames)
	{
		List<GUIContent> l_Labels = new List<GUIContent>(a_States.Count + 2)
		                            {
		                            	new GUIContent(NONE_LABEL)
		                            };
		List<string> l_Values = new List<string>(a_States.Count + 2)
		                            {
		                            	string.Empty
		                            };
		List<int> l_LayerIDs = new List<int>(a_States.Count + 2)
		                       {
			                       -1
		                       };

		for (int i = 0; i < a_States.Count; i++)
		{
			StateOption l_State = a_States[i];

			if (a_DuplicateNames.Contains(l_State.Value))
			{
				l_Labels.Add(new GUIContent(
					        $"{l_State.Label.text}  [Duplicate]",
					        $"More than one state is named '{l_State.Value}'."));
			}
			else
			{
				l_Labels.Add(l_State.Label);
			}

			l_Values.Add(l_State.Value);
			l_LayerIDs.Add(l_State.LayerID);
		}

		bool l_HasMixedValues = a_StateNameProperty.hasMultipleDifferentValues ||
			                    a_LayerIDProperty.hasMultipleDifferentValues;
		int l_CurrentIndex = FindStateOptionIndex(l_Values,
			                                      l_LayerIDs,
			                                      a_StateNameProperty.stringValue,
			                                      a_LayerIDProperty.intValue);

		if (!l_HasMixedValues && l_CurrentIndex < 0)
		{
			int l_NameIndex = l_Values.IndexOf(a_StateNameProperty.stringValue);
			if (l_NameIndex >= 0)
			{
				l_CurrentIndex = l_NameIndex;
				a_LayerIDProperty.intValue = l_LayerIDs[l_CurrentIndex];
			}
		}

		if (!l_HasMixedValues &&
			l_CurrentIndex < 0 &&
			TryGetLegacyShortName(a_StateNameProperty.stringValue, l_Values, out string l_ShortName))
		{
			a_StateNameProperty.stringValue = l_ShortName;
			l_CurrentIndex = l_Values.IndexOf(l_ShortName);
			a_LayerIDProperty.intValue = l_LayerIDs[l_CurrentIndex];
		}

		if (l_CurrentIndex < 0)
		{
			l_CurrentIndex = l_Values.Count;
			l_Labels.Add(new GUIContent(
				$"<Missing> {a_StateNameProperty.stringValue}"));
			l_Values.Add(a_StateNameProperty.stringValue);
			l_LayerIDs.Add(a_LayerIDProperty.intValue);
		}

		bool l_PreviousMixedValue = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = l_HasMixedValues;

		try
		{
			EditorGUI.BeginChangeCheck();
			int l_NewIndex = EditorGUI.Popup(a_Position, l_CurrentIndex, l_Labels.ToArray());

			if (EditorGUI.EndChangeCheck() &&
				l_NewIndex >= 0 &&
				l_NewIndex < l_Values.Count)
			{
				a_StateNameProperty.stringValue = l_Values[l_NewIndex];
				a_LayerIDProperty.intValue = l_LayerIDs[l_NewIndex];
			}
		}
		finally
		{
			EditorGUI.showMixedValue = l_PreviousMixedValue;
		}
	}
	private static int FindStateOptionIndex(List<string> a_StateNames,
		                                    List<int> a_LayerIDs,
		                                    string a_StateName,
		                                    int a_LayerID)
	{
		for (int i = 0; i < a_StateNames.Count; i++)
		{
			if (a_StateNames[i] == a_StateName && a_LayerIDs[i] == a_LayerID)
				return i;
		}
		return -1;
	}
	private static void DrawMessage(Rect a_Position, GUIContent a_Label, string a_Message)
	{
		EditorGUI.LabelField(a_Position, a_Label, new GUIContent(a_Message));
	}
	private static void DrawDisabledPopup(Rect a_Position, string a_Message)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUI.Popup(a_Position, 0, new[] { new GUIContent(a_Message) });
		}
	}
	private static void ClearStateReference(SerializedProperty a_StateNameProperty,
		                                    SerializedProperty a_LayerIDProperty)
	{
		if (a_StateNameProperty.hasMultipleDifferentValues ||
			a_LayerIDProperty.hasMultipleDifferentValues)
		{
			return;
		}

		a_StateNameProperty.stringValue = string.Empty;
		a_LayerIDProperty.intValue = -1;
	}
    
	#endregion

	#region Animator Data
    
	private static bool HasDuplicateStateNames(SerializedProperty a_Property)
	{
		SerializedProperty l_ControllerProperty = a_Property.FindPropertyRelative(CONTROLLER_PROPERTY_NAME);

		if (l_ControllerProperty == null ||
			l_ControllerProperty.hasMultipleDifferentValues)
		{
			return false;
		}

		AnimatorController l_Controller = l_ControllerProperty.objectReferenceValue as AnimatorController;

		if (l_Controller == null) return false;

		var lStates = new List<StateOption>();
		CollectStates(l_Controller, lStates);
		return GetDuplicateStateNames(lStates).Count > 0;
	}
	private static HashSet<string> GetDuplicateStateNames(List<StateOption> a_States)
	{
		HashSet<string> l_EncounteredNames = new HashSet<string>();
		HashSet<string> l_DuplicateNames = new HashSet<string>();

		for (int i = 0; i < a_States.Count; i++)
		{
			string l_StateName = a_States[i].Value;

			if (!l_EncounteredNames.Add(l_StateName))
				l_DuplicateNames.Add(l_StateName);
		}

		return l_DuplicateNames;
	}
	private static bool TryGetLegacyShortName(string a_CurrentValue, List<string> a_AvailableValues, out string a_ShortName)
	{
		a_ShortName = null;

		if (string.IsNullOrEmpty(a_CurrentValue)) return false;

		int l_SeparatorIndex = Mathf.Max(a_CurrentValue.LastIndexOf('.'), a_CurrentValue.LastIndexOf('/'));

		if (l_SeparatorIndex < 0 || l_SeparatorIndex >= a_CurrentValue.Length - 1)
		{
			return false;
		}

		string l_Candidate = a_CurrentValue.Substring(l_SeparatorIndex + 1);

		if (!a_AvailableValues.Contains(l_Candidate)) return false;

		a_ShortName = l_Candidate;
		return true;
	}
	private static void CollectStates(AnimatorController a_Controller, List<StateOption> a_States)
	{
		AnimatorControllerLayer[] l_Layers = a_Controller.layers;
		for (int i = 0; i < l_Layers.Length; i++)
		{
			AnimatorControllerLayer l_Layer = l_Layers[i];
			if (l_Layer.stateMachine == null) continue;

			CollectStatesFromStateMachine(l_Layer.stateMachine, a_States, l_Layer.name, i);
		}
	}
	private static void CollectStatesFromStateMachine(AnimatorStateMachine a_StateMachine,
		                                               List<StateOption> a_States,
		                                               string a_DisplayPath,
		                                               int a_LayerID)
	{
		foreach (ChildAnimatorState l_ChildState in a_StateMachine.states)
		{
			if (l_ChildState.state == null) continue;

			a_States.Add(new StateOption(
				        $"{a_DisplayPath}/{l_ChildState.state.name}",
				        l_ChildState.state.name,
				        a_LayerID));
		}

		foreach (ChildAnimatorStateMachine l_ChildMachine in a_StateMachine.stateMachines)
		{
			if (l_ChildMachine.stateMachine == null) continue;

			string l_StateMachineName = l_ChildMachine.stateMachine.name;

			CollectStatesFromStateMachine(l_ChildMachine.stateMachine,
				                          a_States,
                                          $"{a_DisplayPath}/{l_StateMachineName}",
				                          a_LayerID);
		}
	}
    
	#endregion
}
