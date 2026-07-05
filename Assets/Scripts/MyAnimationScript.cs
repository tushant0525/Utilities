using UnityEngine;

public class MyAnimationScript : MonoBehaviour
{
	#region Variables
    
	[SerializeField] private AnimatorStateReference m_StateName1;
	[SerializeField] private AnimatorStateReference m_StateName2;
    
	#endregion

	#region Debug
    
	[ContextMenu("Print")]
	private void Print()
	{
		Debug.Log($"{m_StateName1} :: {m_StateName1.LayerID}");
		Debug.Log($"{m_StateName2} :: {m_StateName2.LayerID}");
	}
    
	#endregion
}
