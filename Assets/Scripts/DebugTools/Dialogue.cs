using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Dialogue", fileName = "New Dialogue")]
public class Dialogue : ScriptableObject
{
    [TextArea]
    public string[] sentences;
}
