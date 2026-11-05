using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectData", menuName="ScriptableObjects/ObjectData", order=1)]
public class ObjectData : ScriptableObject
{
    public string name; // 오브젝트 이름
    public Sprite icon; // 오브젝트 이미지
}
