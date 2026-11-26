using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog data")]
public class DialogData : ScriptableObject
{
    [System.Serializable]
    public class Line
    {
        public string Speaker;
        public Sprite portrait;
        [TextArea(2, 5)] public string Text;
    }

    public string NextScene;
    public bool PlayOncePerSession = true;

    public List<Line> Lines = new List<Line>();
}
